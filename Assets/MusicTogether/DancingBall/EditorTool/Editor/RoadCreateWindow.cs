using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.EditorTool.UIManager;
using MusicTogether.DancingBall.Scene;
using MusicTogether.DancingBall.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.Editor
{
    public class RoadCreateWindow : UnityEditor.EditorWindow
    {
        private const string UxmlPath = "Assets/MusicTogether/DancingBall/UI/RoadCreateWindow.uxml";
        private Action<string, int, int, int> onCreate;
        private IRoad templateRoad;
        private RoadCreateWindowManager _windowManager;

        public static void ShowWindow(IRoad template, Action<string, int, int, int> onCreate)
        {
            var window = CreateInstance<RoadCreateWindow>();
            window.titleContent = new GUIContent("Create Road");
            window.minSize = new Vector2(360, 260);
            window.onCreate = onCreate;
            window.templateRoad = template;
            window.ShowUtility();
        }

        private void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                Debug.LogError($"[RoadCreateWindow] UXML not found at path: {UxmlPath}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            _windowManager = new RoadCreateWindowManager(rootVisualElement);
            _windowManager.CreateRequested = OnCreateRequested;
            _windowManager.CancelRequested = Close;

            var data = templateRoad?.RoadData;
            var sceneData = templateRoad?.Map?.SceneData;
            _windowManager.SetSegmentOptions(GetSegmentDisplayNames(sceneData), GetSegmentIndices(sceneData));
            _windowManager.SetDefaults(
                data == null ? "Road_New" : $"{data.roadName}_New",
                data?.targetSegmentIndex ?? 0,
                data?.noteBeginIndex ?? 0,
                data?.noteEndIndex ?? 0);
        }

        private void OnCreateRequested(string roadName, int segmentIndex, int noteBegin, int noteEnd)
        {
            onCreate?.Invoke(roadName, segmentIndex, noteBegin, noteEnd);
            Close();
        }

        private static List<string> GetSegmentDisplayNames(SceneData sceneData)
        {
            var result = new List<string>();
            if (sceneData?.SegmentList == null) return result;
            foreach (var segment in sceneData.SegmentList.OrderBy(seg => seg.Index))
            {
                var displayName = string.IsNullOrWhiteSpace(segment.name) ? "Unnamed" : segment.name;
                result.Add($"{segment.Index} | {displayName}");
            }
            return result;
        }

        private static List<int> GetSegmentIndices(SceneData sceneData)
        {
            var result = new List<int>();
            if (sceneData?.SegmentList == null) return result;
            foreach (var segment in sceneData.SegmentList.OrderBy(seg => seg.Index))
            {
                result.Add(segment.Index);
            }
            return result;
        }
    }
}

using MusicTogether.DancingBall.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MusicTogether.DancingBall.EditorTool.UIManager;
using MusicTogether.DancingBall.Scene;

namespace MusicTogether.DancingBall.EditorTool.Editor
{
    public class InspectorWindow : UnityEditor.EditorWindow
    {
        private const string UxmlPath = "Assets/MusicTogether/DancingBall/UI/InspectorWindow.uxml";
        private EditorCenter EditorCenter => EditorCenter.Instance;
        private InspectorWindowManager _windowManager;

        [MenuItem("MusicTogether/DancingBall/Editor Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<InspectorWindow>("DancingBall Editor");
            window.minSize = new Vector2(520, 360);
        }

        private void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                Debug.LogError($"[DancingBallEditorWindow] UXML not found at path: {UxmlPath}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);

            _windowManager = new InspectorWindowManager(rootVisualElement);
            _windowManager.LoadShortcutSettings(EditorShortcutConfig.Config);
            _windowManager.RetryBind = BindEditorCenter;
            BindEditorCenter();
        }
        
        private void BindEditorCenter()
        {
            if (!ValidateEditorCenter()) return;

            // TODO: Here you can bind your other EditorCenter events logically if added later.
            EditorCenter.OnRoadSelectionChanged += OnRoadSelected;
            EditorCenter.OnBlockSelectionChanged += OnBlockSelected;
            _windowManager.SetBindedViewVisible(true);
        }
        
        private bool ValidateEditorCenter()
        {
            if (EditorCenter == null)
            {
                _windowManager.SetBindedViewVisible(false);
                return false;
            }
            return true;
        }
        
        //绑定函数
        private void OnRoadSelected(IRoad road)
        {
            _windowManager.SetRoadNoteRange(road.RoadData.noteBeginIndex, road.RoadData.noteEndIndex);
            _windowManager.SetRoadTargetDataName(road.RoadData.roadName);
        }

        private void OnBlockSelected(IBlock block, IBlockDisplacementData displacementData)
        {
            //如果为空，让玩家自己选择目标类型，并new一个目标类型的对象
            if (displacementData == null)
            {
                
            }
            //如果不为空，根据类型显示数据
            else switch (displacementData)
            {
                case ClassicBlockDisplacementData classicData:
                    _windowManager.SetClassicBlockTurnType(classicData.turnType);
                    _windowManager.SetClassicBlockDisplacementType(classicData.displacementType);
                    break;
            }
            
        }
    }
}

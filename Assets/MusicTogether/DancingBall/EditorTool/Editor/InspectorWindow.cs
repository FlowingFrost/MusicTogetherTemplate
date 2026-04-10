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

    [MenuItem("MusicTogether/DancingBall/Inspector")]
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
            _windowManager.RetryBind = BindEditorCenter;
            BindEditorCenter();
        }
        
        private void BindEditorCenter()
        {
            if (!VerifyEditorCenter()) return;
            
            // TODO: Here you can bind your other EditorCenter events logically if added later.
            EditorCenter.OnRoadSelectionChanged += OnRoadSelected;
            EditorCenter.OnBlockSelectionChanged += OnBlockSelected;

            _windowManager.RetryBind = BindEditorCenter;
            _windowManager.MapMissBindingRetryRequested = BindEditorCenter;
            
            _windowManager.SetBindedViewVisible(true);
            if (VerifyMap()) _windowManager.SetMapContainersVisibility(true, true, false);
            OnRoadSelected(EditorCenter.selectedRoad);
            OnBlockSelected(EditorCenter.selectedBlock, EditorCenter.selectedDisplacementData);
        }

        private bool VerifyEditorCenter() { if (EditorCenter == null) { _windowManager.SetBindedViewVisible(false); return false; } return true; }
        private bool VerifyMap() { if (EditorCenter.targetMap == null) { _windowManager.SetMapContainersVisibility(false, false, true); return false; } return true; }
        private bool VerifyRoad() { if (EditorCenter.selectedRoad == null) { _windowManager.SetRoadContainersVisibility(false, false, true); return false; } return true; }
        private bool VerifyBlock() { if (EditorCenter.selectedBlock == null) { _windowManager.SetBlockContainersVisibility(false, false, true); return false; } return true; }
        //绑定函数
        private void OnRoadSelected(IRoad road)
        {
            if (!VerifyRoad()) return;
            _windowManager.SetRoadContainersVisibility(true, true, false);
            _windowManager.SetRoadNoteRange(road.RoadData.noteBeginIndex, road.RoadData.noteEndIndex);
            _windowManager.SetRoadTargetDataName(road.RoadData.roadName);
        }
        private void OnBlockSelected(IBlock block, IBlockDisplacementData displacementData)
        {
            if (!VerifyBlock()) return;
            _windowManager.SetBlockContainersVisibility(true, true, false);
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
        
        //功能按钮
        private void MapRebuildRoadsRequested() { if (!VerifyMap()) return; EditorCenter.MapRebuildRoadsRequested(); }
    }
}

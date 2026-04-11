using System.Collections.Generic;
using System.Linq;
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
    private ClassicBlockDisplacementUIManager _classicBlockDisplacementManager;

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
            var classicRoot = rootVisualElement.Q<VisualElement>("classic-root");
            if (classicRoot != null)
            {
                _classicBlockDisplacementManager = new ClassicBlockDisplacementUIManager(classicRoot);
                _classicBlockDisplacementManager.DataChanged = OnClassicDisplacementDataChanged;
            }
            _windowManager.RetryBind = BindEditorCenter;
            BindEditorCenter();
        }
        
        private void BindEditorCenter()
        {
            if (!VerifyEditorCenter()) return;
            
            // TODO: Here you can bind your other EditorCenter events logically if added later.
            EditorCenter.OnRoadSelectionChanged += OnRoadSelected;
            EditorCenter.OnBlockSelectionChanged += OnBlockSelected;

            _windowManager.MapRebuildRoadsRequested = MapRebuildRoadsRequested;
            _windowManager.MapRefreshAllRoadsRequested = MapRefreshAllRoadsRequested;
            _windowManager.RoadRefreshBlocksRequested = RoadRefreshBlocksRequested;
            _windowManager.RoadUpdateBlockTransformRequested = RoadUpdateBlockTransformRequested;
            _windowManager.RoadRefreshBlockDisplayRequested = RoadRefreshBlockDisplayRequested;
            _windowManager.RoadModifyTargetSegmentRequested = RoadModifyTargetSegmentRequested;
            _windowManager.RoadModifyNoteRangeRequested = RoadModifyNoteRangeRequested;
            _windowManager.RoadModifyTargetDataNameRequested = RoadModifyTargetDataNameRequested;
            _windowManager.RoadListSelectionChanged = RoadListSelectionChanged;
            _windowManager.BlockDisplacementSelectionChanged = BlockDisplacementSelectionChanged;
            _windowManager.RoadRefreshRequested = RoadRefreshRequested;
            _windowManager.BlockDisplacementApplyBatchRequested = BlockDisplacementApplyBatchRequested;
            _windowManager.RoadCreateRequested = RoadCreateRequested;
            _windowManager.RoadDeleteRequested = RoadDeleteRequested;
            _windowManager.RoadDuplicateRequested = RoadDuplicateRequested;
            _windowManager.BlockDisplacementCreateRequested = BlockDisplacementCreateRequested;
            _windowManager.BlockDisplacementDeleteRequested = BlockDisplacementDeleteRequested;

            _windowManager.RetryBind = BindEditorCenter;
            _windowManager.MapMissBindingRetryRequested = BindEditorCenter;
            
            _windowManager.SetBindedViewVisible(true);
            if (VerifyMap())
            {
                _windowManager.SetMapContainersVisibility(true, false, false);
                _windowManager.BindRoadList(EditorCenter.targetMap.SceneData.roadDataList, EditorCenter.SelectedRoadIndex);
            }
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
            _windowManager.SetRoadContainersVisibility(true, false, false);
            _windowManager.SetRoadNoteRange(road.RoadData.noteBeginIndex, road.RoadData.noteEndIndex);
            _windowManager.SetRoadTargetDataName(road.RoadData.roadName);
            _windowManager.SetRoadSegmentOptions(GetSegmentDisplayNames(EditorCenter.targetMap?.SceneData), GetSegmentIndices(EditorCenter.targetMap?.SceneData), road.RoadData.targetSegmentIndex);
            _windowManager.BindRoadList(EditorCenter.targetMap.SceneData.roadDataList, EditorCenter.SelectedRoadIndex);
            _windowManager.BindBlockDisplacementList(road.RoadData.blockDisplacementDataList, EditorCenter.SelectedBlockIndex);
        }
        private void OnBlockSelected(IBlock block, IBlockDisplacementData displacementData)
        {
            if (!VerifyBlock()) return;
            _windowManager.SetBlockContainersVisibility(false, true, false);
            _windowManager.BindBlockDisplacementList(EditorCenter.selectedRoad.RoadData.blockDisplacementDataList, block.BlockLocalIndex);
            //如果为空，让玩家自己选择目标类型，并new一个目标类型的对象
            if (displacementData == null)
            {
                _windowManager.SetBlockDisplacementCreateVisible(true);
                _windowManager.SetBlockDisplacementDetailVisible(false);
                _classicBlockDisplacementManager?.SetData(null);
            }
            //如果不为空，根据类型显示数据
            else
            {
                _windowManager.SetBlockDisplacementCreateVisible(false);
                _windowManager.SetBlockDisplacementDetailVisible(true);
                switch (displacementData)
                {
                    case ClassicBlockDisplacementData classicData:
                        _classicBlockDisplacementManager?.SetData(classicData);
                        break;
                    default:
                        _classicBlockDisplacementManager?.SetData(null);
                        break;
                }
            }
            
        }
        
        //功能按钮
        private void MapRebuildRoadsRequested() { if (!VerifyMap()) return; EditorCenter.MapRebuildRoadsRequested(); }
        private void MapRefreshAllRoadsRequested() { if (!VerifyMap()) return; EditorCenter.MapRefreshAllRoadsRequested(); }

        private void RoadRefreshRequested()
        {
            if (!VerifyMap()) return;
            EditorCenter.targetMap.SceneData.RefreshRoadDataList();
            EditorCenter.RefreshSelection();
        }

        private void RoadListSelectionChanged(int roadIndex)
        {
            if (!VerifyMap()) return;
            EditorCenter.JumpTo(roadIndex);
        }

        private void BlockDisplacementSelectionChanged(int blockLocalIndex)
        {
            if (!VerifyRoad()) return;
            EditorCenter.JumpTo(EditorCenter.SelectedRoadIndex, blockLocalIndex);
        }

        private void BlockDisplacementApplyBatchRequested()
        {
            if (!VerifyRoad()) return;
            var selectedData = EditorCenter.selectedDisplacementData;
            if (selectedData == null) return;

            var targetIndices = _windowManager.GetSelectedBlockDisplacementIndices();
            if (targetIndices.Count == 0) return;

            foreach (var blockLocalIndex in targetIndices)
            {
                IBlockDisplacementData newData;
                if (selectedData is ClassicBlockDisplacementData classicData)
                {
                    var clone = new ClassicBlockDisplacementData(blockLocalIndex)
                    {
                        turnType = classicData.turnType,
                        displacementType = classicData.displacementType
                    };
                    newData = clone;
                }
                else
                {
                    newData = EditorCenter.selectedRoad.RoadData.CreateBlockDisplacementData(blockLocalIndex, selectedData.GetType());
                }

                if (newData != null)
                {
                    EditorCenter.selectedRoad.RoadData.AddOrReplace_BlockData(newData);
                }
            }

            EditorCenter.selectedRoad.OnBlockDisplacementRuleChanged();
            _windowManager.BindBlockDisplacementList(EditorCenter.selectedRoad.RoadData.blockDisplacementDataList, EditorCenter.SelectedBlockIndex);
        }

        private void RoadCreateRequested()
        {
            if (!VerifyMap()) return;
            RoadCreateWindow.ShowWindow(EditorCenter.selectedRoad, (roadName, segmentIndex, noteBegin, noteEnd) =>
            {
                EditorCenter.CreateRoad(roadName, segmentIndex, noteBegin, noteEnd);
                _windowManager.BindRoadList(EditorCenter.targetMap.SceneData.roadDataList, EditorCenter.SelectedRoadIndex);
            });
        }

        private void RoadDeleteRequested()
        {
            if (!VerifyMap()) return;
            EditorCenter.DeleteSelectedRoad();
            _windowManager.BindRoadList(EditorCenter.targetMap.SceneData.roadDataList, EditorCenter.SelectedRoadIndex);
        }

        private void RoadDuplicateRequested()
        {
            if (!VerifyMap()) return;
            EditorCenter.DuplicateSelectedRoad();
            _windowManager.BindRoadList(EditorCenter.targetMap.SceneData.roadDataList, EditorCenter.SelectedRoadIndex);
        }

        private void BlockDisplacementCreateRequested()
        {
            if (!VerifyRoad()) return;
            var selectedType = _windowManager.GetSelectedDisplacementDataType();
            var dataType = selectedType switch
            {
                BlockDisplacementDataType.Classic => typeof(ClassicBlockDisplacementData),
                _ => typeof(ClassicBlockDisplacementData)
            };
            EditorCenter.CreateBlockDisplacementDataForSelected(dataType);
            _windowManager.BindBlockDisplacementList(EditorCenter.selectedRoad.RoadData.blockDisplacementDataList, EditorCenter.SelectedBlockIndex);
        }

        private void BlockDisplacementDeleteRequested()
        {
            if (!VerifyRoad()) return;
            EditorCenter.RemoveBlockDisplacementDataForSelected();
            _windowManager.BindBlockDisplacementList(EditorCenter.selectedRoad.RoadData.blockDisplacementDataList, EditorCenter.SelectedBlockIndex);
        }

        private void RoadRefreshBlocksRequested()
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.RebuildBlocks();
        }

        private void RoadUpdateBlockTransformRequested()
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.OnBlockDisplacementRuleChanged();
        }

        private void RoadRefreshBlockDisplayRequested()
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.RefreshBlockInfoDisplay();
        }

        private void RoadModifyTargetSegmentRequested(int segmentIndex)
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.ModifyTargetSegmentIndex(segmentIndex);
        }

        private void RoadModifyNoteRangeRequested(int begin, int end)
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.ModifyNoteRange(begin, end);
        }

        private void RoadModifyTargetDataNameRequested(string value)
        {
            if (!VerifyRoad()) return;
            EditorCenter.selectedRoad.ModifyTargetRoadDataName(value);
        }

        private void OnClassicDisplacementDataChanged(IBlockDisplacementData data)
        {
            if (!VerifyRoad()) return;
            if (data == null) return;
            EditorCenter.selectedRoad.ModifyDisplacementData(data.BlockIndex_Local, data);
            EditorCenter.RefreshSelection();
            _windowManager.BindBlockDisplacementList(EditorCenter.selectedRoad.RoadData.blockDisplacementDataList, EditorCenter.SelectedBlockIndex);
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

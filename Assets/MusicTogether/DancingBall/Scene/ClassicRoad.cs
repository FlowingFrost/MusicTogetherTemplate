using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.EditorTool;
using MusicTogether.DancingBall.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Scene
{
    public class ClassicRoad : SerializedMonoBehaviour, IRoad
    {
        //外部引用
        public IMap Map { get; private set; }
        
        private RoadData cachedRoadData;
        private string targetRoadDataName;
        public RoadData RoadData
        {
            get
            {
                if (cachedRoadData != null) return cachedRoadData;
                Map.SceneData.Get_RoadData_ByRoadName(targetRoadDataName, out cachedRoadData);
                return cachedRoadData;
            }
            private set
            {
                cachedRoadData = value;
                targetRoadDataName = value.roadName;
            }
        }
        private EditorConfig EditorConfig => EditorConfig.Config;
        //本体绑定信息
        public Transform Transform => transform;
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private List<IBlock> blocks = new List<IBlock>();
        public List<IBlock> Blocks => blocks;
        
        //表达式
        private bool IsDataValid => Map != null && RoadData != null;

        //函数
        public void Init(IMap map, RoadData roadData)
        {
            Map = map;
            RoadData = roadData;
        }

        #region Road_Operations //操作功能
            //物体操作================================================================================================
            /// <summary>
            /// 暂未优化该函数，仅仅完成了错误处理
            /// </summary>
            public void RefreshRoadBlocks()
            {
                if (!IsDataValid) return;

                int blockCount = Mathf.Max(0, RoadData.BlockCount);

                // 去重：保留 blockLocalIndex 最小的
                var duplicates = blocks
                    .GroupBy(b => b.BlockLocalIndex)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderBy(b => b.BlockLocalIndex).Skip(1))
                    .ToList();
                if (duplicates.Count > 0)
                {
                    RemoveBlocks(duplicates);
                }

                int formerCount = blocks.Count;
                if (formerCount < blockCount)
                {
                    CreateBlocks(formerCount, blockCount - formerCount);
                }
                else if (formerCount > blockCount)
                {
                    var toRemove = blocks.Where(b => b.BlockLocalIndex >= blockCount).ToList();
                    RemoveBlocks(toRemove);
                }

                // 重新赋值 localIndex 按顺序
                var sorted = blocks.OrderBy(b => b.BlockLocalIndex).ToList();
                blocks.Clear();
                blocks.AddRange(sorted);
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].BlockLocalIndex = i;
                }

                OnBlockDisplacementRuleChanged();
            }

            /// <summary>
            /// 暂未优化该函数，仅仅完成了错误处理
            /// </summary>
            public void OnBlockDisplacementRuleChanged()
            {
                if (!IsDataValid) return;
                int blockCount = Mathf.Max(0, RoadData.BlockCount);
                var sortedBlocks = blocks
                    .Where(b => b.BlockLocalIndex >= 0 && b.BlockLocalIndex < blockCount)
                    .OrderBy(b => b.BlockLocalIndex)
                    .ToList();
                if (sortedBlocks.Count == 0) return;

                // 分组：遇到具有位移规则的块时开启新组
                var groups = new List<List<IBlock>>();
                var currentGroup = new List<IBlock>();
                groups.Add(currentGroup);

                foreach (var block in sortedBlocks)
                {
                    bool isNewGroupStart = currentGroup.Count > 0 && RoadData.blockDisplacementDataList.Exists(b => b.BlockIndex_Local == block.BlockLocalIndex);
                    if (isNewGroupStart)
                    {
                        currentGroup.Add(block);
                        currentGroup = new List<IBlock> { block };
                        groups.Add(currentGroup);
                    }
                    else
                    {
                        currentGroup.Add(block);
                    }
                }

                foreach (var group in groups)
                {
                    if (group.Count == 0) continue;
                    var rootBlock = group[0];
                    if (!RoadData.Get_BlockData(rootBlock.BlockLocalIndex, out var blockData) || blockData == null) continue;
                    blockData.ApplyDisplacementRule(group);
                }

                RefreshBlockInfoDisplay();
            }
            public void RefreshBlockInfoDisplay()
            {
                if (!IsDataValid) return;
                if (blocks == null || blocks.Count == 0) return;

                foreach (var block in blocks)
                {
                    var color = GetBlockColor(RoadData, block.BlockLocalIndex);
                    block.BlockDisplay?.RefreshBlockDisplay(color);
                }
            }
            
            //数据操作================================================================================================
            //Road级别
            public void ModifyBlockBeginIndex(int newBeginIndex)
            {
                if (!IsDataValid) return;
                RoadData.noteBeginIndex = newBeginIndex;
                RefreshRoadBlocks();//Map.EditManager?.OnRoadBlockCountChanged(EditorTool.EditorActionContext.ForRoad(Map, GetRoadIndex()));
            }

            public void ModifyBlockEndIndex(int newEndIndex)
            {
                if (!IsDataValid) return;
                RoadData.noteEndIndex = newEndIndex;
                RefreshRoadBlocks();//Map.EditManager?.OnRoadBlockCountChanged(EditorTool.EditorActionContext.ForRoad(Map, GetRoadIndex()));
            }

            public void ModifyTargetRoadDataName(string newName)
            {
                if (!IsDataValid) return;
                RoadData.roadName = newName;
                targetRoadDataName = newName;
            }
            //Block级别
            public void ModifyDisplacementData(int blockLocalIndex, IBlockDisplacementData newDisplacementData)
            {
                if (!IsDataValid) return;
                if (newDisplacementData == null) return;
                RoadData.Set_BlockData(newDisplacementData);
                OnBlockDisplacementRuleChanged();//Map.EditManager?.OnBlockDisplacementRuleChanged(EditorTool.EditorActionContext.ForRoadAndBlock(Map, GetRoadIndex(), blockLocalIndex));
            }
            
            //数据获取
            public List<MovementData> GetBlockMovementDatum(int blockBeginIndex)
            {
                return new List<MovementData>();
            }
        #endregion
        
        private int GetRoadIndex()
        {
            if (Map?.SceneData == null || RoadData == null) return -1;
            return Map.SceneData.roadDataList.IndexOf(RoadData);
        }
        
        //操作功能

        //数据操作
        private Color GetBlockColor(RoadData roadData, int blockLocalIndex)
        {
            if (!IsDataValid) return EditorConfig.problemBlockColor;
            bool hasRule = roadData.blockDisplacementDataList.Exists(b => b.BlockIndex_Local == blockLocalIndex);
            bool hasTap = Map.SceneData.Exists_NoteIndex(roadData.targetSegmentIndex,
                roadData.noteBeginIndex + blockLocalIndex);
            if (hasTap)
                return hasRule
                    ? EditorConfig.tapBlockWithDisplacementColor
                    : EditorConfig.tapBlockWithoutDisplacementColor;
            return hasRule ? EditorConfig.normalBlockWithDisplacementColor : EditorConfig.normalBlockColor;
        }
        //地图操作
        /// <summary>
        /// 从prefab创建Block。未预装Block脚本时自动添加ClassicBlock
        /// </summary>
        public IBlock CreateBlock(int blockLocalIndex)
        {
            if (!IsDataValid) return null;
            if (blocks.Exists(b => b.BlockLocalIndex == blockLocalIndex))
            {
                Debug.LogError($"Block with local index {blockLocalIndex} already exists in the road.");
                return null;
            }
            
            var parent = Transform;

            if (blockPrefab == null)
            {
                Debug.LogError($"Block prefab does not exist.");
                return null;
            }

            GameObject blockObj = Instantiate(blockPrefab, parent, false);
            blockObj.name = $"Block_{blockLocalIndex}";

            blockObj.transform.localPosition = Vector3.zero;
            blockObj.transform.localRotation = Quaternion.identity;

            if (!blockObj.TryGetComponent<IBlock>(out var block))
            {
                block = blockObj.AddComponent<ClassicBlock>();
            }

            block.Init(this, blockLocalIndex);

            blocks.Add(block);
            return block;
        }
        public List<IBlock> CreateBlocks(IEnumerable<int> index)
        {
            var list = new List<IBlock>();
            foreach (var i in index)
            {
                if (blocks.Exists(b => b.BlockLocalIndex == i)) continue;
                var block = CreateBlock(i);
                if (block != null) list.Add(block);
            }
            return list;
        }
        public List<IBlock> CreateBlocks(int indexBegin, int count)
        {
            var indexList = new List<int>();
            for (int i = 0; i < count; i++)
            {
                indexList.Add(indexBegin + i);
            }
            return CreateBlocks(indexList);
        }
        public void RemoveBlocks(List<IBlock> blocksToRemove)
        {
            foreach (var block in blocksToRemove)
            {
                if (block == null) continue;
                blocks.Remove(block);
                if (block is MonoBehaviour blockBehaviour)
                {
                    DestroyImmediate(blockBehaviour.gameObject);
                }
            }
        }
    }
}
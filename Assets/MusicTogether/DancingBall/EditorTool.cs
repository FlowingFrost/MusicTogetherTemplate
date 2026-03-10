using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class EditorTool : MonoBehaviour
    {
        const int MaxBlockCountThreshold = 5000;
        private EditorConfig EditorConfig => EditorConfig.Config;

        /// <summary>
        /// 重构road列表。优化代码暂不开放
        /// </summary>
        /// <param name="targetMap"></param>
        public void RecreateMapRoadList(Map targetMap)
        {
            var mapData = targetMap.mapData;
            var roadList = targetMap.roads;
            var roadDataList = mapData.roadDataList;

            // 检查重复 roadIndex，保留 blocks 最多的那一个，其余标记为待删除
            var duplicateRoads = roadList.GroupBy(r => r.roadIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.blocks.Count).Skip(1))
                .ToList();
            if (duplicateRoads.Count > 0)
            {
                Debug.LogWarning($"Found {duplicateRoads.Count} road(s) with duplicate roadIndex in map {targetMap.name}. Removing duplicates.");
                RemoveRoads(targetMap, duplicateRoads);
            }
            
            //检查超出数据规定范围的road
            var roadToBeRemove = roadList.Where(r => r.roadIndex >= mapData.totalRoadCount || r.roadIndex < 0).ToList();
            RemoveRoads(targetMap, roadToBeRemove);
            //检查数据规定范围内但未创建的road
            var roadToBeAdded = roadDataList.Select(d => d.index).Where(i => !roadList.Exists(r => r.roadIndex == i) ).ToList();
            AddRoads(targetMap, roadToBeAdded);
           
            RemoveRoads(targetMap, roadList);
            AddRoads(targetMap, roadDataList.Select(d => d.index).ToList());
        }
        
        private bool IsMapHasValidReference(Map targetMap)
        {
            if (targetMap == null) return false;
            if (targetMap.factory == null) return false;
            if (targetMap.mapData == null) return false;
            return true;
        }
        private bool CheckRoadList(Map targetMap)
        {
            for (int i = 0; i < targetMap.roads.Count; i++)
            {
                if (targetMap.roads[i].roadIndex != i)
                {
                    Debug.LogError($"Road index mismatch at position {i} in map {targetMap.name}");
                    return false;
                }
            }
            return true;
        }
        private void RemoveRoads(Map targetMap, List<Road> roadsToRemove)
        {
            foreach (var road in roadsToRemove)
            {
                targetMap.roads.Remove(road);
                DestroyImmediate(road.gameObject);
            }
        }
        private void AddRoads(Map targetMap, List<int> roadIndicesToAdd)
        {
            foreach (var index in roadIndicesToAdd)
            {
                var newRoad = targetMap.factory.CreateRoad(targetMap, index);
                if (newRoad != null) targetMap.roads.Add(newRoad);
            }
        }

        public void RemoveBlocks(Road targetRoad, List<Block> blocksToRemove)
        {
            foreach (var block in blocksToRemove)
            {
                targetRoad.blocks.Remove(block);
                DestroyImmediate(block.gameObject);
            }
        }

        public void RefreshRoadBlocks(Map targetMap, int targetRoadIndex)
        {
            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(targetMap);
            if (targetRoadIndex >= targetMap.roads.Count) return;
            
            var targetRoad = targetMap.roads[targetRoadIndex];
            var mapData = targetMap.mapData;
            mapData.GetRoadData(targetRoadIndex, out var roadData);
            
            //剔除正无穷项
            int indexBegin = roadData.beginBlockIndex;
            int indexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);
            int blockCount = indexEnd - indexBegin + 1;
            if (blockCount > MaxBlockCountThreshold) return;
            
            OnRoadBlockCountChanged(targetMap, targetRoadIndex, indexBegin, blockCount);
            OnBlockDisplacementRuleChanged(targetMap, indexBegin);
        }
        
        public void OnRoadBlockBeginIndexChanged(Map targetMap, int targetRoadIndex, int formerBlockBeginIndex, int newBlockBeginIndex)
        {
            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(targetMap);
            if (targetRoadIndex >= targetMap.roads.Count) return;
            
            //var targetRoad = targetMap.roads[targetRoadIndex];
            var mapData = targetMap.mapData;
            
            int indexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);
            
            int blockCount = indexEnd - newBlockBeginIndex + 1;
            if (blockCount > MaxBlockCountThreshold) return;
            OnRoadBlockCountChanged(targetMap, targetRoadIndex, newBlockBeginIndex, blockCount);
            
            if (targetRoadIndex <= 0) return;
            
            var formerRoad = targetMap.roads[targetRoadIndex - 1];
            if (formerRoad == null) return;
            
            mapData.GetRoadData(formerRoad.roadIndex, out var formerRoadData);
            var formerRoadBlockBeginIndex = formerRoadData.beginBlockIndex;
            var formerRoadBlockCount = mapData.GetRoadEndBlockIndex(formerRoad.roadIndex) - formerRoadBlockBeginIndex + 1;
            OnRoadBlockCountChanged(targetMap, formerRoad.roadIndex, formerRoadBlockBeginIndex, formerRoadBlockCount);
            OnBlockDisplacementRuleChanged(targetMap, newBlockBeginIndex);
        }
        
        private void OnRoadBlockCountChanged(Map targetMap, int targetRoadIndex, int blockIndexBegin, int newCount)
        {
            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(targetMap);
            if (targetRoadIndex >= targetMap.roads.Count) return;
            
            var targetRoad = targetMap.roads[targetRoadIndex];

            // 查重：保留 blockGlobalIndex 最小的，其余重复项销毁
            var duplicateBlocks = targetRoad.blocks
                .GroupBy(b => b.globalBlockIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(b => b.globalBlockIndex).Skip(1))
                .ToList();
            if (duplicateBlocks.Count > 0)
            {
                Debug.LogWarning($"Found {duplicateBlocks.Count} block(s) with duplicate blockGlobalIndex in road {targetRoad.name}. Removing duplicates.");
                duplicateBlocks.ForEach(b => { targetRoad.blocks.Remove(b); DestroyImmediate(b.gameObject); });
            }

            // 补充不足的 block
            int formerCount = targetRoad.blocks.Count;
            if (formerCount < newCount)
            {
                targetMap.factory.CreateBlocks(targetRoad, formerCount, newCount - formerCount);
            }
            // 移除多余的 block
            else if (formerCount > newCount)
            {
                targetRoad.blocks
                    .Where(b => b.globalBlockIndex >= blockIndexBegin + newCount || b.globalBlockIndex < blockIndexBegin)
                    .ToList()
                    .ForEach(b => { targetRoad.blocks.Remove(b); DestroyImmediate(b.gameObject); });
            }

            // 重新赋值所有 block 的全局索引
            targetRoad.blocks
                .Select((b, i) => (block: b, index: i + blockIndexBegin))
                .ToList()
                .ForEach(t => t.block.globalBlockIndex = t.index);
        }

        //BlockMethods
        public void OnBlockDisplacementRuleChanged(Map targetMap, int targetBlockIndex)
        {
            if (!IsMapHasValidReference(targetMap)) return;

            var mapData = targetMap.mapData;
            int targetRoadIndex = targetMap.FindRoadIndexByBlockIndex(targetBlockIndex);
            if (targetRoadIndex < 0) return;
            var targetRoad = targetMap.roads[targetRoadIndex];
            if (targetRoad.blocks == null || targetRoad.blocks.Count == 0) return;

            mapData.GetRoadData(targetRoadIndex, out var roadData);
            int blockIndexBegin = roadData.beginBlockIndex;
            int blockIndexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);

            // 按 globalBlockIndex 排序，筛选属于本 Road 范围内的 blocks
            var sortedBlocks = targetRoad.blocks
                .Where(b => b.globalBlockIndex >= blockIndexBegin && b.globalBlockIndex <= blockIndexEnd)
                .OrderBy(b => b.globalBlockIndex)
                .ToList();
            if (sortedBlocks.Count == 0) return;

            // ── 分组逻辑 ──────────────────────────────────────────────────────
            // 规则：
            // 1. Road 起点（blockIndexBegin）强制作为第一组的起始 block
            // 2. 每组从起始 block 开始，向后延伸，直到遇到下一个满足 HasBlockData 的 block
            //    该 HasBlockData 的 block 作为本组的最后一个元素（同时也是下一组的起始 block）
            // 3. 每组执行 ApplyDisplacement，起始位置/旋转取该组第一个 block 的当前 localPosition/localRotation
            var groups = new List<List<Block>>();
            var currentGroup = new List<Block>();
            groups.Add(currentGroup);

            foreach (var block in sortedBlocks)
            {
                bool isNewGroupStart = currentGroup.Count > 0
                                       && mapData.HasBlockData(block.globalBlockIndex);
                if (isNewGroupStart)
                {
                    // 当前 block 既是上一组的末尾，也是新组的起始
                    currentGroup.Add(block);
                    currentGroup = new List<Block> { block };
                    groups.Add(currentGroup);
                }
                else
                {
                    currentGroup.Add(block);
                }
            }
            string debugInfo = $"Block groups for Road {targetRoad.name} (Block index range: {blockIndexBegin}-{blockIndexEnd}):\n";
            // ── 依次执行 ApplyDisplacement ────────────────────────────────────
            foreach (var group in groups)
            {
                if (group.Count == 0) continue;
                mapData.GetBlockData(group[0].globalBlockIndex, out var data);
                debugInfo += $"index : {data.blockGlobalIndex}, Turn: {data.turnType}, Displacement: {data.displacementType}\n";
                var firstTransform = group[0].transform;
                Vector3 startPosition = firstTransform.localPosition;
                Quaternion startRotation = firstTransform.localRotation;

                targetMap.displacementApplier.ApplyDisplacement(targetMap, group, startPosition, startRotation);
            }
            Debug.Log(debugInfo);
        }

        public void RefreshBlockInfoDisplay(Map targetMap, int targetRoadIndex)
        {
            if (!IsMapHasValidReference(targetMap)) return;

            if (targetRoadIndex < 0) return;
            var targetRoad = targetMap.roads[targetRoadIndex];
            if (targetRoad.blocks == null || targetRoad.blocks.Count == 0) return;
            var displays = targetRoad.blocks.Select(b => (b.globalBlockIndex,b.blockInformationDisplay)).ToList();
            if (displays.Count == 0) return;
            foreach (var displayInfo in displays)
            {
                var color = GetBlockColor(targetMap.mapData, displayInfo.globalBlockIndex);
                displayInfo.blockInformationDisplay.RefreshBlockDisplay(color);
            }
        }
        
        private Color GetBlockColor(MapData mapData, int blockGlobalIndex)
        {
            if (mapData == null) return EditorConfig.problemBlockColor;
            mapData.GetBlockData(blockGlobalIndex, out var blockData);
            bool hasTurn = blockData.HasTurn;
            bool hasDisplacement = blockData.HasDisplacement;
            
            if (mapData.BlockHasTap(blockGlobalIndex))
            {
                if (hasTurn || hasDisplacement) return EditorConfig.tapBlockWithDisplacementColor;
                else return EditorConfig.tapBlockWithoutDisplacementColor;
            }
            else
            {
                if (hasTurn || hasDisplacement) return EditorConfig.normalBlockWithDisplacementColor;
                else return EditorConfig.normalBlockColor;
            }
        }
    }
}
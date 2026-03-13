using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Archived_SceneMap;
using UnityEngine;

namespace MusicTogether.DancingBall.Archived_EditorTool
{
    public class EditorTool : MonoBehaviour
    {
        const int MaxBlockCountThreshold = 5000;
        private EditorConfig EditorConfig => EditorConfig.Config;

        // ══════════════════════════════════════════════════════════════════
        //  Map / Road 级操作
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// 重构 road 列表。完成后自动刷新所有 Road 的 Block 数量与位移规则。
        /// </summary>
        [AfterAction(nameof(RefreshAllRoads))]
        public void RecreateMapRoadList(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            var mapData = targetMap.mapData;
            var roadList = targetMap.roads;
            var roadDataList = mapData.roadDataList;

            // 检查重复 roadIndex，保留 blocks 最多的那一个，其余标记为待删除
            var duplicateRoads = roadList.GroupBy(r => r.roadGlobalIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.blocks.Count).Skip(1))
                .ToList();
            if (duplicateRoads.Count > 0)
            {
                Debug.LogWarning($"Found {duplicateRoads.Count} road(s) with duplicate roadIndex in map {targetMap.name}. Removing duplicates.");
                RemoveRoads(targetMap, duplicateRoads);
            }

            //检查超出数据规定范围的road
            var roadToBeRemove = roadList.Where(r => r.roadGlobalIndex >= mapData.totalRoadCount || r.roadGlobalIndex < 0).ToList();
            RemoveRoads(targetMap, roadToBeRemove);
            //检查数据规定范围内但未创建的road
            var roadToBeAdded = roadDataList.Select(d => d.roadGlobalIndex).Where(i => !roadList.Exists(r => r.roadGlobalIndex == i)).ToList();
            AddRoads(targetMap, roadToBeAdded);

            RemoveRoads(targetMap, roadList);
            AddRoads(targetMap, roadDataList.Select(d => d.roadGlobalIndex).ToList());
        }

        /// <summary>
        /// 刷新地图上所有 Road 的 Block 数量与位移（供后置链调用）。
        /// </summary>
        public void RefreshAllRoads(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            if (!IsMapHasValidReference(targetMap)) return;
            for (int i = 0; i < targetMap.roads.Count; i++)
            {
                OnRoadBlockCountChanged(EditorActionContext.ForRoad(targetMap, i));
                OnBlockDisplacementRuleChanged(EditorActionContext.ForRoadAndBlock(targetMap, i,
                    targetMap.mapData.roadDataList[i].beginBlockIndex));
            }
        }
        
        // ══════════════════════════════════════════════════════════════════
        //  Road 级操作
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// 刷新指定 Road 的 Block 数量，完成后自动更新位移规则与颜色显示。
        /// </summary>
        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void RefreshRoadBlocks(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            int targetRoadIndex = ctx.RoadIndex;

            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(ctx);
            if (targetRoadIndex >= targetMap.roads.Count) return;

            var mapData = targetMap.mapData;
            mapData.GetRoadData(targetRoadIndex, out var roadData);

            int indexBegin = roadData.beginBlockIndex;
            int indexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);
            int blockCount = indexEnd - indexBegin + 1;
            if (blockCount > MaxBlockCountThreshold) return;

            ctx.BlockIndex = indexBegin;
            OnRoadBlockCountChanged(ctx);
        }

        /// <summary>
        /// Road 起始 Block 索引发生变化时调用。
        /// 完成后自动更新位移规则与颜色显示。
        /// </summary>
        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void OnRoadBlockBeginIndexChanged(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            int targetRoadIndex = ctx.RoadIndex;

            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(ctx);
            if (targetRoadIndex >= targetMap.roads.Count) return;

            var mapData = targetMap.mapData;
            mapData.GetRoadData(targetRoadIndex, out var roadData);
            int newBlockBeginIndex = roadData.beginBlockIndex;

            int indexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);
            int blockCount = indexEnd - newBlockBeginIndex + 1;
            if (blockCount > MaxBlockCountThreshold) return;

            ctx.BlockIndex = newBlockBeginIndex;
            OnRoadBlockCountChanged(ctx);

            if (targetRoadIndex <= 0) return;

            var formerRoad = targetMap.roads[targetRoadIndex - 1];
            if (formerRoad == null) return;

            mapData.GetRoadData(formerRoad.roadGlobalIndex, out var formerRoadData);
            var formerCtx = EditorActionContext.ForRoad(targetMap, formerRoad.roadGlobalIndex);
            formerCtx.BlockIndex = formerRoadData.beginBlockIndex;
            OnRoadBlockCountChanged(formerCtx);
        }

        // ══════════════════════════════════════════════════════════════════
        //  Block 级核心操作
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// 根据数据同步 Road 的 Block 数量（新增/删除/重排索引）。
        /// 完成后自动更新位移规则与颜色显示。
        /// </summary>
        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void OnRoadBlockCountChanged(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            int targetRoadIndex = ctx.RoadIndex;

            if (!IsMapHasValidReference(targetMap)) return;
            if (!CheckRoadList(targetMap)) RecreateMapRoadList(ctx);
            if (targetRoadIndex < 0 || targetRoadIndex >= targetMap.roads.Count) return;

            var mapData = targetMap.mapData;
            mapData.GetRoadData(targetRoadIndex, out var roadData);
            int blockIndexBegin = roadData.beginBlockIndex;
            int indexEnd = mapData.GetRoadEndBlockIndex(targetRoadIndex);
            int newCount = indexEnd - blockIndexBegin + 1;

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
                targetMap.factory.CreateBlocks(targetRoad, blockIndexBegin + formerCount, newCount - formerCount);
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

            // 将 BlockIndex 写回上下文，供后置链使用
            ctx.BlockIndex = blockIndexBegin;
        }

        /// <summary>
        /// Block 位移/转向规则发生变化时调用，重新计算并应用所属 Road 内的所有位移分组。
        /// 完成后自动刷新颜色显示。
        /// </summary>
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void OnBlockDisplacementRuleChanged(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            int targetBlockIndex = ctx.BlockIndex;

            if (!IsMapHasValidReference(targetMap)) return;

            var mapData = targetMap.mapData;
            int targetRoadIndex = ctx.RoadIndex;
            if (targetRoadIndex < 0) return;

            // 同步 RoadIndex，保持上下文一致
            ctx.RoadIndex = targetRoadIndex;

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
            var groups = new List<List<Block>>();
            var currentGroup = new List<Block>();
            groups.Add(currentGroup);

            foreach (var block in sortedBlocks)
            {
                bool isNewGroupStart = currentGroup.Count > 0
                                       && mapData.HasDisplacementRule_ByBlockGlobalIndex(block.globalBlockIndex);
                if (isNewGroupStart)
                {
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
            
            foreach (var group in groups)
            {
                if (group.Count == 0) continue;
                mapData.GetBlockData_ByBlockGlobalIndex(group[0].globalBlockIndex, out var data);
                debugInfo += $"index : {data.blockGlobalIndex}, Turn: {data.turnType}, Displacement: {data.displacementType}\n";
                var firstTransform = group[0].transform;
                //bool isFirstGroup = groups.First() == group;
                targetMap.displacementApplier.ApplyDisplacement(
                    targetMap, group, firstTransform.localPosition, firstTransform.localRotation);
            }
            Debug.Log(debugInfo);
        }

        /// <summary>
        /// 刷新指定 Road 内所有 Block 的颜色显示（InfoDisplay）。
        /// </summary>
        public void RefreshBlockInfoDisplay(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            int targetRoadIndex = ctx.RoadIndex;

            if (!IsMapHasValidReference(targetMap)) return;
            if (targetRoadIndex < 0 || targetRoadIndex >= targetMap.roads.Count) return;

            var targetRoad = targetMap.roads[targetRoadIndex];
            if (targetRoad.blocks == null || targetRoad.blocks.Count == 0) return;

            foreach (var block in targetRoad.blocks)
            {
                var color = GetBlockColor(targetMap, targetRoadIndex, block.globalBlockIndex);
                block.blockInformationDisplay.RefreshBlockDisplay(color);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  辅助方法（不参与后置链）
        // ══════════════════════════════════════════════════════════════════

        public void RemoveBlocks(Road targetRoad, List<Block> blocksToRemove)
        {
            foreach (var block in blocksToRemove)
            {
                if (block == null) continue;
                targetRoad.blocks.Remove(block);
                DestroyImmediate(block.gameObject);
            }
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
                if (targetMap.roads[i].roadGlobalIndex != i)
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

        private Color GetBlockColor(Map map, int roadIndex, int blockGlobalIndex)
        {
            var mapData = map.mapData;
            if (mapData == null) return EditorConfig.problemBlockColor;
            mapData.GetBlockData_ByBlockGlobalIndex(blockGlobalIndex, out var blockData);
            bool hasTurn = blockData.HasTurn;
            bool hasDisplacement = blockData.HasDisplacement;
            

            if (mapData.HasTap_ByBlockGlobalIndex(roadIndex, blockGlobalIndex))
            {
                return (hasTurn || hasDisplacement)
                    ? EditorConfig.tapBlockWithDisplacementColor
                    : EditorConfig.tapBlockWithoutDisplacementColor;
            }
            else
            {
                return (hasTurn || hasDisplacement)
                    ? EditorConfig.normalBlockWithDisplacementColor
                    : EditorConfig.normalBlockColor;
            }
        }
    }
}
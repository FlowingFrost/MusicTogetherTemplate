using System.Collections.Generic;
using System.Linq;
using MusicTogether.Archived_DancingBall.Scene;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.EditorTool
{
    public class EditorTool : MonoBehaviour
    {
        private const int MaxBlockCountThreshold = 5000;
        private EditorConfig EditorConfig => EditorConfig.Config;

        // Map/Road 级操作
        [AfterAction(nameof(RefreshAllRoads))]
        public void RecreateMapRoadList(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            if (!IsMapValid(targetMap)) return;

            var roadList = targetMap.Roads;
            var roadDataList = targetMap.SceneData.roadDataList;

            // 去重：保留 blocks 最多的
            var duplicateRoads = roadList.GroupBy(r => r.RoadIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.blocks.Count).Skip(1)).ToList();
            RemoveRoads(targetMap, duplicateRoads);

            // 移除超界 road
            var roadToRemove = roadList.Where(r => r.RoadIndex < 0 || r.RoadIndex >= roadDataList.Count).ToList();
            RemoveRoads(targetMap, roadToRemove);

            // 添加缺失 road
            var roadToAdd = roadDataList.Select(d => d.RoadGlobalIndex).Where(i => !roadList.Exists(r => r.RoadIndex == i)).ToList();
            AddRoads(targetMap, roadToAdd);
        }

        public void RefreshAllRoads(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            if (!IsMapValid(map)) return;

            if (map.SceneData == null || map.SceneData.roadDataList == null) return;
            foreach (var roadData in map.SceneData.roadDataList)
            {
                int roadIndex = roadData.RoadGlobalIndex;
                OnRoadBlockCountChanged(EditorActionContext.ForRoad(map, roadIndex));
                OnBlockDisplacementRuleChanged(EditorActionContext.ForRoadAndBlock(map, roadIndex, 0));
            }
        }

        // Road 级
        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void RefreshRoadBlocks(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!CheckRoadList(map)) RecreateMapRoadList(ctx);
            if (!map.SceneData.GetRoadData(roadIndex, out _)) return;

            var sceneData = map.SceneData;
            sceneData.GetRoadData(roadIndex, out var roadData);
            int blockCount = Mathf.Max(0, roadData.BlockCount);
            if (blockCount > MaxBlockCountThreshold) return;

            ctx.BlockLocalIndex = 0;
            OnRoadBlockCountChanged(ctx);
        }

        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void OnRoadBlockCountChanged(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!CheckRoadList(map)) RecreateMapRoadList(ctx);
            if (!map.SceneData.GetRoadData(roadIndex, out _)) return;

            var sceneData = map.SceneData;
            sceneData.GetRoadData(roadIndex, out var roadData);
            int blockCount = Mathf.Max(0, roadData.BlockCount);

            if (!map.TryGetRoad(roadIndex, out var road)) return;

            // 去重：保留 blockLocalIndex 最小的
            var duplicates = road.blocks
                .GroupBy(b => b.blockLocalIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(b => b.blockLocalIndex).Skip(1)).ToList();
            if (duplicates.Count > 0)
            {
                RemoveBlocks(road, duplicates);
            }

            int formerCount = road.blocks.Count;
            if (formerCount < blockCount)
            {
                map.Factory.CreateBlocks(road, formerCount, blockCount - formerCount);
            }
            else if (formerCount > blockCount)
            {
                var toRemove = road.blocks.Where(b => b.blockLocalIndex >= blockCount).ToList();
                RemoveBlocks(road, toRemove);
            }

            // 重新赋值 localIndex 按顺序
            road.blocks = road.blocks.OrderBy(b => b.blockLocalIndex).ToList();
            for (int i = 0; i < road.blocks.Count; i++)
            {
                road.blocks[i].blockLocalIndex = i;
            }

            ctx.BlockLocalIndex = 0;
        }

        // Block 位移与显示
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        public void OnBlockDisplacementRuleChanged(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!map.SceneData.GetRoadData(roadIndex, out _)) return;

            var sceneData = map.SceneData;
            if (!map.TryGetRoad(roadIndex, out var road)) return;
            if (road.blocks == null || road.blocks.Count == 0) return;

            sceneData.GetRoadData(roadIndex, out var roadData);
            int blockCount = Mathf.Max(0, roadData.BlockCount);
            var sortedBlocks = road.blocks.Where(b => b.blockLocalIndex >= 0 && b.blockLocalIndex < blockCount)
                .OrderBy(b => b.blockLocalIndex).ToList();
            if (sortedBlocks.Count == 0) return;

            // 分组：遇到具有位移规则的块时开启新组
            var groups = new List<List<Block>>();
            var currentGroup = new List<Block>();
            groups.Add(currentGroup);

            foreach (var block in sortedBlocks)
            {
                bool isNewGroupStart = currentGroup.Count > 0 && sceneData.HasDisplacementRule(roadIndex, block.blockLocalIndex);
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

            foreach (var group in groups)
            {
                if (group.Count == 0) continue;
                map.DisplacementApplier?.ApplyDisplacement(map, group, group[0].transform.localPosition, group[0].transform.localRotation);
            }
        }

        public void RefreshBlockInfoDisplay(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!map.SceneData.GetRoadData(roadIndex, out _)) return;

            if (!map.TryGetRoad(roadIndex, out var road)) return;
            if (road.blocks == null || road.blocks.Count == 0) return;

            foreach (var block in road.blocks)
            {
                var color = GetBlockColor(map, roadIndex, block.blockLocalIndex);
                block.blockInformationDisplay.RefreshBlockDisplay(color);
            }
        }

        // 辅助
        private bool IsMapValid(Map map)
        {
            if (map == null) return false;
            if (map.Factory == null) return false;
            if (map.SceneData == null) return false;
            return true;
        }

        private bool CheckRoadList(Map map)
        {
            if (map.SceneData == null || map.SceneData.roadDataList == null) return false;
            for (int i = 0; i < map.SceneData.roadDataList.Count; i++)
            {
                if (!map.TryGetRoad(i, out _))
                {
                    Debug.LogError($"Road missing at index {i} in map {map.name}");
                    return false;
                }
            }
            return true;
        }

        private void RemoveRoads(Map map, List<Road> roadsToRemove)
        {
            foreach (var road in roadsToRemove)
            {
                map.Roads.Remove(road);
                if (road != null) DestroyImmediate(road.gameObject);
            }
        }

        private void AddRoads(Map map, List<int> roadIndicesToAdd)
        {
            foreach (var index in roadIndicesToAdd)
            {
                var newRoad = map.Factory.CreateRoad(map, index);
                if (newRoad != null && !map.Roads.Contains(newRoad)) map.Roads.Add(newRoad);
            }
        }

        public void RemoveBlocks(Road road, List<Block> blocksToRemove)
        {
            foreach (var block in blocksToRemove)
            {
                if (block == null) continue;
                road.blocks.Remove(block);
                DestroyImmediate(block.gameObject);
            }
        }

        private Color GetBlockColor(Map map, int roadIndex, int blockLocalIndex)
        {
            var sceneData = map.SceneData;
            if (sceneData == null) return EditorConfig.problemBlockColor;
            sceneData.GetBlockData(roadIndex, blockLocalIndex, out var data);
            bool hasTurn = data.HasTurn;
            bool hasDisplacement = data.HasDisplacement;

            if (sceneData.HasTap(roadIndex, blockLocalIndex))
            {
                return (hasTurn || hasDisplacement)
                    ? EditorConfig.tapBlockWithDisplacementColor
                    : EditorConfig.tapBlockWithoutDisplacementColor;
            }

            return (hasTurn || hasDisplacement)
                ? EditorConfig.normalBlockWithDisplacementColor
                : EditorConfig.normalBlockColor;
        }
    }
}

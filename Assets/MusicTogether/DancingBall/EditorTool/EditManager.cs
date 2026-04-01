using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.Scene;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    public class EditManager : MonoBehaviour, IEditManager
    {
        private const int MaxBlockCountThreshold = 5000;

        [SerializeField] private Factory factory;
        [SerializeField] private EditorActionDispatcher dispatcher;

        public Factory Factory => factory;
        public EditorActionDispatcher Dispatcher => dispatcher;
        private EditorConfig EditorConfig => EditorConfig.Config;

        // Map/Road 级操作
        [AfterAction(nameof(RefreshAllRoads))]
        [Obsolete] public void RecreateMapRoadList(EditorActionContext ctx)
        {
            var targetMap = ctx.TargetMap;
            if (!IsMapValid(targetMap)) return;

            var roadList = targetMap.Roads;
            var roadDataList = targetMap.SceneData.roadDataList;

            // 去重：保留 blocks 最多的
            var duplicateRoads = roadList
                .GroupBy(r => r.RoadData?.roadName)
                .Where(g => g.Key != null && g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.Blocks.Count).Skip(1))
                .ToList();
            RemoveRoads(targetMap, duplicateRoads);

            // 移除无效 road（不在数据列表中）
            var validNames = new HashSet<string>(roadDataList.Select(r => r.roadName));
            var roadToRemove = roadList.Where(r => r.RoadData == null || !validNames.Contains(r.RoadData.roadName)).ToList();
            RemoveRoads(targetMap, roadToRemove);

            // 添加缺失 road
            var roadToAdd = roadDataList
                .Where(d => !roadList.Exists(r => r.RoadData != null && r.RoadData.roadName == d.roadName))
                .ToList();
            AddRoads(targetMap, roadToAdd);
        }

        [Obsolete] public void RefreshAllRoads(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            if (!IsMapValid(map)) return;
            if (map.SceneData == null || map.SceneData.roadDataList == null) return;

            for (int i = 0; i < map.SceneData.roadDataList.Count; i++)
            {
                OnRoadBlockCountChanged(EditorActionContext.ForRoad(map, i));
                OnBlockDisplacementRuleChanged(EditorActionContext.ForRoadAndBlock(map, i, 0));
            }
        }

        // Road 级
        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        [Obsolete] public void RefreshRoadBlocks(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!CheckRoadList(map)) RecreateMapRoadList(ctx);
            if (!TryGetRoadData(map, roadIndex, out _)) return;

            var sceneData = map.SceneData;
            var roadData = sceneData.roadDataList[roadIndex];
            int blockCount = Mathf.Max(0, roadData.BlockCount);
            if (blockCount > MaxBlockCountThreshold) return;

            ctx.BlockLocalIndex = 0;
            OnRoadBlockCountChanged(ctx);
        }

        [AfterAction(nameof(OnBlockDisplacementRuleChanged))]
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        [Obsolete] public void OnRoadBlockCountChanged(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!CheckRoadList(map)) RecreateMapRoadList(ctx);
            if (!TryGetRoadData(map, roadIndex, out var roadData)) return;

            if (!TryGetRoad(map, roadData, out var road)) return;

            int blockCount = Mathf.Max(0, roadData.BlockCount);

            // 去重：保留 blockLocalIndex 最小的
            var duplicates = road.Blocks
                .GroupBy(b => b.BlockLocalIndex)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderBy(b => b.BlockLocalIndex).Skip(1))
                .ToList();
            if (duplicates.Count > 0)
            {
                RemoveBlocks(road, duplicates);
            }

            int formerCount = road.Blocks.Count;
            if (formerCount < blockCount)
            {
                factory?.CreateBlocks(road, formerCount, blockCount - formerCount);
            }
            else if (formerCount > blockCount)
            {
                var toRemove = road.Blocks.Where(b => b.BlockLocalIndex >= blockCount).ToList();
                RemoveBlocks(road, toRemove);
            }

            // 重新赋值 localIndex 按顺序
            var sorted = road.Blocks.OrderBy(b => b.BlockLocalIndex).ToList();
            road.Blocks.Clear();
            road.Blocks.AddRange(sorted);
            for (int i = 0; i < road.Blocks.Count; i++)
            {
                road.Blocks[i].BlockLocalIndex = i;
            }

            ctx.BlockLocalIndex = 0;
        }

        // Block 位移与显示
        [AfterAction(nameof(RefreshBlockInfoDisplay))]
        [Obsolete] public void OnBlockDisplacementRuleChanged(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!TryGetRoadData(map, roadIndex, out var roadData)) return;
            if (!TryGetRoad(map, roadData, out var road)) return;
            if (road.Blocks == null || road.Blocks.Count == 0) return;

            int blockCount = Mathf.Max(0, roadData.BlockCount);
            var sortedBlocks = road.Blocks
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
                bool isNewGroupStart = currentGroup.Count > 0 && HasDisplacementRule(roadData, block.BlockLocalIndex);
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
                if (!roadData.Get_BlockData(rootBlock.BlockLocalIndex, out var blockData) || blockData == null) continue;
                blockData.ApplyDisplacementRule(group);
            }
        }

        [Obsolete] public void RefreshBlockInfoDisplay(EditorActionContext ctx)
        {
            var map = ctx.TargetMap;
            int roadIndex = ctx.RoadIndex;
            if (!IsMapValid(map)) return;
            if (!TryGetRoadData(map, roadIndex, out var roadData)) return;
            if (!TryGetRoad(map, roadData, out var road)) return;
            if (road.Blocks == null || road.Blocks.Count == 0) return;

            foreach (var block in road.Blocks)
            {
                var color = GetBlockColor(roadData, block.BlockLocalIndex);
                block.BlockDisplay?.RefreshBlockDisplay(color);
            }
        }

        // 辅助
        [Obsolete] private bool IsMapValid(IMap map)
        {
            if (map == null) return false;
            if (map.SceneData == null) return false;
            if (factory == null)
            {
                Debug.LogWarning("[EditManager] Factory 未赋值。", this);
                return false;
            }
            return true;
        }

        [Obsolete] private bool CheckRoadList(IMap map)
        {
            if (map.SceneData == null || map.SceneData.roadDataList == null) return false;
            foreach (var roadData in map.SceneData.roadDataList)
            {
                if (!map.Roads.Any(r => r.RoadData != null && r.RoadData.roadName == roadData.roadName))
                {
                    Debug.LogError($"Road missing: {roadData.roadName} in map", this);
                    return false;
                }
            }
            return true;
        }

        [Obsolete] private bool TryGetRoadData(IMap map, int roadIndex, out RoadData roadData)
        {
            roadData = null;
            if (map.SceneData == null || map.SceneData.roadDataList == null) return false;
            if (roadIndex < 0 || roadIndex >= map.SceneData.roadDataList.Count) return false;
            roadData = map.SceneData.roadDataList[roadIndex];
            return roadData != null;
        }

        [Obsolete] private bool TryGetRoad(IMap map, RoadData roadData, out IRoad road)
        {
            road = map.Roads.FirstOrDefault(r => r.RoadData != null && r.RoadData.roadName == roadData.roadName);
            return road != null;
        }

        [Obsolete] private void RemoveRoads(IMap map, List<IRoad> roadsToRemove)
        {
            foreach (var road in roadsToRemove)
            {
                map.Roads.Remove(road);
                if (road is MonoBehaviour roadBehaviour)
                {
                    DestroyImmediate(roadBehaviour.gameObject);
                }
            }
        }

        [Obsolete] private void AddRoads(IMap map, List<RoadData> roadDataToAdd)
        {
            foreach (var roadData in roadDataToAdd)
            {
                var newRoad = factory.CreateRoad(map, roadData);
                if (newRoad != null && !map.Roads.Contains(newRoad)) map.Roads.Add(newRoad);
            }
        }

        [Obsolete] public void RemoveBlocks(IRoad road, List<IBlock> blocksToRemove)
        {
            foreach (var block in blocksToRemove)
            {
                if (block == null) continue;
                road.Blocks.Remove(block);
                if (block is MonoBehaviour blockBehaviour)
                {
                    DestroyImmediate(blockBehaviour.gameObject);
                }
            }
        }

        [Obsolete] private bool HasDisplacementRule(RoadData roadData, int blockLocalIndex)
        {
            if (!roadData.Get_BlockData(blockLocalIndex, out var data) || data == null) return false;
            if (data is ClassicBlockDisplacementData classicData) return classicData.HasDisplacementRule;
            return true;
        }

        [Obsolete] private Color GetBlockColor(RoadData roadData, int blockLocalIndex)
        {
            if (roadData == null) return EditorConfig.problemBlockColor;
            bool hasRule = HasDisplacementRule(roadData, blockLocalIndex);
            return hasRule ? EditorConfig.normalBlockWithDisplacementColor : EditorConfig.normalBlockColor;
        }
    }
}
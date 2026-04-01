using System;
using System.Collections.Generic;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.Scene;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject blockPrefab;

        /// <summary>
        /// 从prefab或空物体创建Road。未预装Road脚本时自动添加ClassicRoad
        /// </summary>
        [Obsolete] public IRoad CreateRoad(IMap map, RoadData roadData)
        {
            //数据校验：空校验与重复校验
            if (map == null || roadData == null) return null;
            if (map.Roads.Exists(r => r.RoadData != null && r.RoadData.roadName == roadData.roadName))
            {
                Debug.LogError($"Road with name {roadData.roadName} already exists in the map.");
                return null;
            }

            var parent = map.Transform;

            GameObject roadObj = roadPrefab != null
                ? Instantiate(roadPrefab, parent, false)
                : new GameObject();
            roadObj.name = string.IsNullOrEmpty(roadData.roadName) ? "Road" : $"Road_{roadData.roadName}";
            if (roadPrefab == null && parent != null)
            {
                roadObj.transform.SetParent(parent);
            }

            if (!roadObj.TryGetComponent<IRoad>(out var road))
            {
                road = roadObj.AddComponent<ClassicRoad>();
            }

            road.Init(map, roadData);
            map.Roads.Add(road);
            return road;
        }

        [Obsolete] public List<IRoad> CreateRoads(IMap map, List<RoadData> roadDataList)
        {
            var list = new List<IRoad>();
            if (roadDataList == null) return list;
            foreach (var roadData in roadDataList)
            {
                var road = CreateRoad(map, roadData);
                if (road != null) list.Add(road);
            }
            return list;
        }

        /// <summary>
        /// 从prefab创建Block。未预装Block脚本时自动添加ClassicBlock
        /// </summary>
        [Obsolete] public IBlock CreateBlock(IRoad road, int blockLocalIndex)
        {
            if (road == null) return null;
            if (road.Blocks.Exists(b => b.BlockLocalIndex == blockLocalIndex))
            {
                Debug.LogError($"Block with local index {blockLocalIndex} already exists in the road.");
                return null;
            }
            
            var parent = road.Transform;

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

            block.Init(road, blockLocalIndex);

            road.Blocks.Add(block);
            return block;
        }

        [Obsolete] public List<IBlock> CreateBlocks(IRoad road, int indexBegin, int count)
        {
            var index = new List<int>();
            for (int i = 0; i < count; i++)
            {
                index.Add(indexBegin + i);
            }
            return CreateBlocks(road, index);
        }

        [Obsolete] public List<IBlock> CreateBlocks(IRoad road, IEnumerable<int> index)
        {
            var list = new List<IBlock>();
            foreach (var i in index)
            {
                if (road.Blocks.Exists(b => b.BlockLocalIndex == i)) continue;
                var block = CreateBlock(road, i);
                if (block != null) list.Add(block);
            }
            return list;
        }
    }
}
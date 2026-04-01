using System.Collections.Generic;
using MusicTogether.Archived_DancingBall.Scene;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.EditorTool
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] public GameObject blockPrefab;

        public Road CreateRoad(Map map, int roadIndex)
        {
            if (map.Roads.Exists(r => r.RoadIndex == roadIndex))
            {
                Debug.LogError($"Road with index {roadIndex} already exists in the map.");
                return null;
            }

            GameObject roadObj = new GameObject($"Road_{roadIndex}");
            roadObj.transform.SetParent(map.transform);

            var road = roadObj.AddComponent<Road>();
            road.map = map;
            road.RoadIndex = roadIndex;
            map.Roads.Add(road);
            return road;
        }

        public List<Road> CreateRoads(Map map, int indexBegin, int count)
        {
            var list = new List<Road>();
            for (int i = indexBegin; i < indexBegin + count; i++)
            {
                if (map.Roads.Exists(r => r.RoadIndex == i)) continue;
                var r = CreateRoad(map, i);
                if (r != null) list.Add(r);
            }
            return list;
        }

        public Block CreateBlock(Road road, int blockLocalIndex)
        {
            if (road.blocks.Exists(b => b.blockLocalIndex == blockLocalIndex))
            {
                Debug.LogError($"Block with local index {blockLocalIndex} already exists in the road.");
                return null;
            }

            GameObject blockObj = Instantiate(blockPrefab, road.transform, false);
            blockObj.transform.localPosition = Vector3.zero;
            blockObj.transform.localRotation = Quaternion.identity;
            blockObj.name = $"Block_{blockLocalIndex}";

            var block = blockObj.AddComponent<Block>();
            block.road = road;
            block.blockLocalIndex = blockLocalIndex;

            var tileHolder = blockObj.GetComponentInChildren<TileHolder>();
            var blockInfoDisplay = blockObj.GetComponentInChildren<BlockInformationDisplay>();
            if (tileHolder == null) tileHolder = blockObj.AddComponent<TileHolder>();
            if (blockInfoDisplay == null) blockInfoDisplay = blockObj.AddComponent<BlockInformationDisplay>();

            block.tileHolder = tileHolder;
            block.blockInformationDisplay = blockInfoDisplay;

            road.blocks.Add(block);
            return block;
        }

        public List<Block> CreateBlocks(Road road, int indexBegin, int count)
        {
            var list = new List<Block>();
            for (int i = indexBegin; i < indexBegin + count; i++)
            {
                if (road.blocks.Exists(b => b.blockLocalIndex == i)) continue;
                var b = CreateBlock(road, i);
                if (b != null) list.Add(b);
            }
            return list;
        }
    }
}

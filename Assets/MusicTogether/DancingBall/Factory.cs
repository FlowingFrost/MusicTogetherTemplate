using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Factory : MonoBehaviour
    {
        public Road CreateRoad(Map map, int roadIndex)
        {
            var index = map.roads.FindIndex(r => roadIndex == r.roadIndex);
            if (index > 0)
            {
                Debug.LogError($"Road with index {roadIndex} already exists in the map.");
                return map.roads[index];
            }
            GameObject roadObj = new GameObject($"Road_{roadIndex}");
            roadObj.transform.SetParent(map.transform);
            Road road = roadObj.AddComponent<Road>();
            road.map = map;
            road.roadIndex = roadIndex;
            return road;
        }
        
        public Block CreateBlock(Road road, int blockIndex)
        {
            var index = road.blocks.FindIndex(b=> blockIndex == b.blockIndex);
            if (index > 0)
            {
                Debug.LogError($"Block with index {blockIndex} already exists in the road.");
                return road.blocks[index];
            }
            GameObject blockObj = new GameObject($"Block_{blockIndex}");
            blockObj.transform.SetParent(road.transform);
            Block block = blockObj.AddComponent<Block>();
            block.road = road;
            block.blockIndex = blockIndex;
            return block;
        }
        
        public List<Block> CreateBlocks(Road road, int indexBegin, int count)
        {
            List<Block> blocks = new List<Block>();
            for (int i = indexBegin; i < indexBegin + count; i++)
            {
                int index = road.blocks.FindIndex(b=> i == b.blockIndex);
                if (i < 0) blocks.Add(CreateBlock(road, i));
            }
            return blocks;
        }
    }
}
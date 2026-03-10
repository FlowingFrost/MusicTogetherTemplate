using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Factory : MonoBehaviour
    {
        [SerializeField] public GameObject blockPrefab;
        public Road CreateRoad(Map map, int roadIndex)
        {
            var index = map.roads.FindIndex(r => roadIndex == r.roadIndex);
            if (index >= 0)
            {
                Debug.LogError($"Road with index {roadIndex} already exists in the map.");
                return null;
            }
            map.mapData.GetRoadData(roadIndex, out var roadData);
            GameObject roadObj = new GameObject($"Road_{roadIndex}");
            roadObj.transform.SetParent(map.transform);
            
            Road road = roadObj.AddComponent<Road>();
            road.map = map;
            road.roadIndex = roadIndex;
            map.roads.Add(road);
            return road;
        }
        
        public List<Road> CreateRoads(Map map, int indexBegin, int count)
        {
            List<Road> roads = new List<Road>();
            for (int i = indexBegin; i < indexBegin + count; i++)
            {
                int index = map.roads.FindIndex(r => i == r.roadIndex);
                if (index < 0) roads.Add(CreateRoad(map, i));
            }
            return roads;
        }
        
        public Block CreateBlock(Road road, int blockIndex)
        {
            var index = road.blocks.FindIndex(b=> blockIndex == b.globalBlockIndex);
            if (index > 0)
            {
                Debug.LogError($"Block with index {blockIndex} already exists in the road.");
                return null;
            }

            GameObject blockObj = Instantiate(blockPrefab, road.transform);
            blockObj.name = $"Block_{blockIndex}";
            Block block = blockObj.AddComponent<Block>();
            
            TileHolder tileHolder = blockObj.GetComponentInChildren<TileHolder>();
            BlockInformationDisplay blockInfoDisplay = blockObj.GetComponentInChildren<BlockInformationDisplay>();
            if (tileHolder == null) tileHolder = blockObj.AddComponent<TileHolder>();
            if (blockInfoDisplay == null) blockInfoDisplay = blockObj.AddComponent<BlockInformationDisplay>();
            
            block.road = road;
            block.tileHolder = tileHolder;
            block.blockInformationDisplay = blockInfoDisplay;
            
            block.globalBlockIndex = blockIndex;
            road.blocks.Add(block);
            return block;
        }
        
        public List<Block> CreateBlocks(Road road, int indexBegin, int count)
        {
            List<Block> blocks = new List<Block>();
            for (int i = indexBegin; i < indexBegin + count; i++)
            {
                int index = road.blocks.FindIndex(b=> i == b.globalBlockIndex);
                if (index < 0) blocks.Add(CreateBlock(road, i));
            }
            return blocks;
        }
    }
}
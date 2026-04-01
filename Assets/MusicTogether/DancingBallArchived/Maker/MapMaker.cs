using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using MusicTogether.DancingBallArchived.Map;

namespace MusicTogether.DancingBallArchived.Maker
{
    public class MapMaker : MonoBehaviour
    {
        [Title("Resources")]
        public MapHolder mapHolder;
        
        public List<RoadMaker> roadMakers;
        public List<RoadHolder> RoadHolders=>mapHolder.roadHolders;
        [Title("Data")] 
        public float mapDefaultBlockSize;

        //Pre-Editing Function
        private void SetGlobalIndex()
        {
            int length = 0;
            foreach (RoadMaker roadMaker in roadMakers)
            {
                foreach (BlockHolder blockHolder in roadMaker.BlockHolders)
                {
                    blockHolder.globalIndex = blockHolder.blockIndex + length;
                }
                int count = roadMaker.BlockHolders.Count;
                mapHolder.roadLength[roadMaker.RoadIndex] = count;
                length+= count;
            }
        }

        public void UpdateBlockManagement()
        {
            
        }
        //Editing Tools
        [Button]
        public RoadMaker CreateRoad()
        {
            int roadIndex = roadMakers.Count;
            return CreateRoad(roadIndex);
        }
        public RoadMaker CreateRoad(int roadIndex)
        {
            var obj = new GameObject($"Road{roadIndex}");
            var maker = obj.AddComponent<RoadMaker>();
            var holder = obj.AddComponent<RoadHolder>();
            
            maker.Init(this,mapHolder,holder,roadIndex);
            
            roadMakers.Add(maker);
            RoadHolders.Add(holder);
            return maker;
        }
        
        //Pre-Playing Function
        private void SortBlocks()
        {
            List<BlockHolder> blocks = new List<BlockHolder>();
            foreach (RoadMaker roadMaker in roadMakers)
            {
                roadMaker.GetTimeInformation();
                blocks.AddRange(roadMaker.BlockHolders);
            }
            float[] animTimes = blocks.Select(b => b.animTime).ToArray();
            float[] tapTimes = blocks.Select(b => b.tapTime).ToArray();
            var sortedAnimIndices = blocks.Select(bh => bh.globalIndex).ToArray(); 
            var sortedTapIndices = blocks.Select(bh => bh.globalIndex).ToArray();
            Array.Sort(animTimes,sortedAnimIndices);
            Array.Sort(tapTimes,sortedTapIndices);
            mapHolder.sortedAnimIndices = sortedAnimIndices;
            mapHolder.sortedTapIndices = sortedTapIndices;
        }
    }
}
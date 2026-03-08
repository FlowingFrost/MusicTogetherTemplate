using System.Collections.Generic;
using MusicTogether.DancingBallOld.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    public class ClassicFactory : MonoBehaviour, IFactory
    {
        public IBlock CreateBlock(IRoad road, int indexInRoad)
        {
            var gameObject = Instantiate(Resources.Load("DancingBall/ClassicBlock"), road.Transform);
            var block = gameObject.GetComponent<ClassicBlock>();
            block.IndexInRoad = indexInRoad;
            block.transform.SetParent(road.Transform);
            return block;
        }

        public IEnumerable<IBlock> CreateBlocks(IRoad road, int blockStartIndex, int blockEndIndex)
        {
            for (int i = blockStartIndex; i <= blockEndIndex; i++)
                yield return CreateBlock(road, i);
        }
        
        public IRoad CreateRoad(IMap map, int indexInMap)
        {
            var gameObject = new GameObject($"Road{indexInMap}", typeof(ClassicRoad));
            gameObject.transform.SetParent(map.Transform);
            var road = gameObject.GetComponent<ClassicRoad>();
            road.RoadIndex = indexInMap;
            return road;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall.Interfaces
{
    public interface IBlock
    {
        public IBlockMaker BlockMaker { get; }

        public int IndexInRoad { get; set; }
        
        //public IBlock PreviousBlock { get; }
        //public IBlock NextBlock { get; }
        public Transform Transform { get; }
        //public int IndexInMap { get; }
        public List<Vector3> GetPositionsInBlock();
    }
}
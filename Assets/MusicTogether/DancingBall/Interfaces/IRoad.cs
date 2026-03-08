using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall.Interfaces
{
    public interface IRoad
    {
        public Transform Transform { get; }
        public IRoadMaker RoadMaker { get; }
        int RoadIndex { get; set; }
        //public int NoteStartIndex { get; set; }
        //public int NoteEndIndex { get; }
        //public int BlockStartIndex { get; set; }
        //public int BlockEndIndex { get; }

        //public void OnDisplacementChanged(IBlockMakerData changedBlock);
        List<IBlock> Blocks { get; }
        IBlock FirstBlock { get; }
        IBlock LastBlock { get; }
        IBlock PreviousBlock(IBlock currentBlock);
        IBlock NextBlock(IBlock currentBlock);
        public IEnumerable<IBlock> BlocksBehind(IBlock currentBlock);
        public IEnumerable<IBlock> BlockBehindTillNextTap(IBlock currentBlock);
        public IEnumerable<IBlock> BlockBehindTillNextCorner(IBlock currentBlock);
        public IEnumerable<IEnumerable<IBlock>> BlockBehindSplitByCorner(IBlock block);
        
        
        
        
        
        double GetTimeAtBlock(IBlock currentBlock);
        List<MovingData> GetMovingDatas(IBlock beginBlock);
    }
}
using System.Collections.Generic;
using MusicTogether.General;
using UnityEngine;

namespace MusicTogether.DancingBall.Interfaces
{
    public interface IMap
    {
        public Transform Transform { get; }
        public InputNoteData NoteData { get; }
        public IReadOnlyList<IRoad> Roads { get; }
        
        public IRoad PreviousRoad(IRoad currentRoad);
        public IRoad NextRoad(IRoad currentRoad);
        //public IBlock FirstBlockInRoad(IRoad road);
        //public IBlock LastBlockInRoad(IRoad road);
        
        public IRoad GetRoadByBlock(IBlock block);
        //public IBlock PreviousBlockInCurrentRoad(IBlock currentBlock);
        //public IBlock NextBlockInCurrentRoad(IBlock currentBlock);
        //public IEnumerable<IBlock> BlockBehindTillNextTap(IBlock currentBlock);
        //public IEnumerable<IBlock> BlockBehindTillNextCorner(IBlock currentBlock);
        //public IEnumerable<IBlock> BlocksBehindInCurrentRoad(IBlock currentBlock);
        
        


        //public List<MovingData> GetNextMovingData(IBlock currentBlock);
    }
}
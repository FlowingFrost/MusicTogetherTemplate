using System.Collections.Generic;

namespace MusicTogether.DancingBallOld.Interfaces
{
    public interface IFactory
    {
        public IBlock CreateBlock(IRoad road, int indexInRoad);
        public IEnumerable<IBlock> CreateBlocks(IRoad road, int blockStartIndex, int blockEndIndex);
        public IRoad CreateRoad(IMap map, int indexInMap);
    }
}
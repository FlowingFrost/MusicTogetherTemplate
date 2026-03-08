using System.Collections.Generic;

namespace MusicTogether.DancingBall.Interfaces
{
    public interface IDisplacementApplyer
    {
        public void ApplyDisplacement(List<IBlock> targetBlocks, IBlock previousBlock);
    }
}
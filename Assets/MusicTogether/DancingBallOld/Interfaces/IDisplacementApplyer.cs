using System.Collections.Generic;

namespace MusicTogether.DancingBallOld.Interfaces
{
    /// <summary>
    /// 位移规则应用器。
    /// 根据段首 Block 的 BlockEntry（从 Road.PlacementData 读取）
    /// 计算并设置本段所有 Block 的 localPosition / localRotation。
    /// </summary>
    public interface IDisplacementApplyer
    {
        /// <summary>
        /// 对一段连续方块应用位移。
        /// </summary>
        /// <param name="targetBlocks">有序（按 IndexInRoad 升序）的方块段。</param>
        /// <param name="previousBlock">段首的前一个 Block（可为 null，表示 Road 起点）。</param>
        /// <param name="road">所属 Road，用于读取 PlacementData。</param>
        void ApplyDisplacement(List<IBlock> targetBlocks, IBlock previousBlock, IRoad road);
    }
}

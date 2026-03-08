namespace MusicTogether.DancingBallOld.Interfaces
{
    public enum TurnType { None, Left, Right }
    public enum DisplacementType { None, Up, Down }

    /// <summary>
    /// Block 的纯放置数据——不持有场景对象引用，由 BlockPlacementData 文件按 index 提供。
    /// </summary>
    public interface IBlockData
    {
        /// <summary>该 Block 在所属 Road 中的索引（与 BlockPlacementData 中的 index 对应）。</summary>
        int Index { get; }
        /// <summary>是否为转弯节点（TurnType != None）。</summary>
        bool HasTap { get; }
        /// <summary>是否携带竖向位移规则（DisplacementType != None）。</summary>
        bool HasRule { get; }
        TurnType TurnType { get; }
        DisplacementType DisplacementType { get; }
    }
}
namespace MusicTogether.DancingBallOld.Interfaces
{
    /// <summary>
    /// Road 制作器接口：描述一条 Road 的参数配置（Block 范围、音符范围等）。
    /// </summary>
    public interface IRoadMaker
    {
        IRoad Road { get; set; }

        /// <summary>本 Road 第一个 Block 在 PlacementData 中的起始索引（0-based）。</summary>
        int BlockStartIndex { get; set; }

        /// <summary>本 Road 最后一个 Block 在 PlacementData 中的结束索引（包含）。</summary>
        int BlockEndIndex { get; set; }

        /// <summary>本 Road 对应音符数据的起始索引。</summary>
        int NoteStartIndex { get; set; }

        /// <summary>本 Road 对应音符数据的结束索引（包含）。</summary>
        int NoteEndIndex { get; set; }

        /// <summary>Block 范围发生变化时的回调（通知 EditManager 增删方块）。</summary>
        System.Action UpdateBlockRange { get; }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Interfaces
{
    /// <summary>
    /// 一段轨道：持有若干 Block 的运行时对象，并关联 BlockPlacementData 文件。
    /// 所有方块的放置数据（转弯/位移）均通过 <see cref="PlacementData"/> 按 index 查询，
    /// 不再序列化在 Block 自身上。
    /// </summary>
    public interface IRoad
    {
        Transform Transform { get; }
        IRoadMaker RoadMaker { get; }
        int RoadIndex { get; set; }

        // ── 数据源 ──────────────────────────────────────────────────
        /// <summary>与本 Road 绑定的 Block 放置数据文件。</summary>
        BlockPlacementData PlacementData { get; }

        /// <summary>获取指定 Block 的放置数据（快捷方式）。</summary>
        BlockEntry GetBlockEntry(IBlock block);

        /// <summary>获取指定 index 的放置数据（快捷方式）。</summary>
        BlockEntry GetBlockEntry(int indexInRoad);

        // ── Block 列表 ───────────────────────────────────────────────
        List<IBlock> Blocks { get; }
        IBlock FirstBlock { get; }
        IBlock LastBlock { get; }

        // ── Block 导航 ───────────────────────────────────────────────
        IBlock PreviousBlock(IBlock currentBlock);
        IBlock NextBlock(IBlock currentBlock);

        /// <summary>currentBlock 之前（不含自身）的所有 Block。</summary>
        IEnumerable<IBlock> BlocksBefore(IBlock currentBlock);

        /// <summary>从 currentBlock 到下一个 Tap（转弯）Block 的连续段（含两端）。</summary>
        IEnumerable<IBlock> BlocksTillNextTap(IBlock currentBlock);

        /// <summary>从 currentBlock 到下一个 Corner Block 的连续段（含两端）。</summary>
        IEnumerable<IBlock> BlocksTillNextCorner(IBlock currentBlock);

        /// <summary>
        /// 将 currentBlock 之后的方块按 Corner 拆成若干段，
        /// 每段首尾均为 Corner，例：[cur,A,B,C][C,D,...,K][K,...,last]。
        /// </summary>
        IEnumerable<IEnumerable<IBlock>> BlocksBehindSplitByCorner(IBlock currentBlock);

        // ── 时间 / 运动数据 ──────────────────────────────────────────
        /// <summary>根据音符数据返回 Ball 到达该 Block 的绝对时间（秒）。</summary>
        double GetTimeAtBlock(IBlock block);

        /// <summary>返回从 beginBlock 到下一个 Tap Block 的运动数据列表。</summary>
        List<MovingData> GetMovingDatas(IBlock beginBlock);
    }
}
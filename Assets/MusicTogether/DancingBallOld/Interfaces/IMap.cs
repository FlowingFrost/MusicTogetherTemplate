using System.Collections.Generic;
using MusicTogether.General;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Interfaces
{
    /// <summary>
    /// 整个关卡地图的容器，管理所有 Road 并提供全局查询入口。
    /// </summary>
    public interface IMap
    {
        Transform Transform { get; }
        InputNoteData NoteData { get; }
        IReadOnlyList<IRoad> Roads { get; }

        // ── Road 导航 ─────────────────────────────────────────────────
        IRoad PreviousRoad(IRoad currentRoad);
        IRoad NextRoad(IRoad currentRoad);

        // ── 跨 Road 查询 ─────────────────────────────────────────────
        /// <summary>根据 Block 实例反查其所属的 Road（O(n) 遍历）。</summary>
        IRoad GetRoadByBlock(IBlock block);

        /// <summary>返回 block 在整个地图中的全局索引（各 Road Block 连续累加）。</summary>
        int GetGlobalBlockIndex(IBlock block);

        /// <summary>返回下一个 Road 的第一个 Block（用于 Road 末尾传送）。</summary>
        IBlock GetFirstBlockOfNextRoad(IRoad currentRoad);
    }
}

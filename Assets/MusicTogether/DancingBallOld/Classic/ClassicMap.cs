using System.Collections.Generic;
using MusicTogether.DancingBallOld.Interfaces;
using MusicTogether.General;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    /// <summary>
    /// 经典模式地图，管理所有 Road 并提供全局 Block/Road 查询。
    /// </summary>
    public class ClassicMap : SerializedBehaviour, IMap
    {
        // ── Inspector 绑定 ───────────────────────────────────────────
        [SerializeField] protected ILevelManager levelManager;
        [SerializeField] protected InputNoteData noteData;
        [SerializeField] protected List<IRoad> roads = new List<IRoad>();

        // ── 运行数据 ─────────────────────────────────────────────────
        protected double levelTime => levelManager != null ? levelManager.LevelTime : 0;

        // ── IMap 实现 ────────────────────────────────────────────────
        public Transform Transform => transform;
        public InputNoteData NoteData => noteData;
        public IReadOnlyList<IRoad> Roads => roads;

        // ── Road 导航 ─────────────────────────────────────────────────
        public IRoad PreviousRoad(IRoad road) =>
            road.RoadIndex > 0 ? roads[road.RoadIndex - 1] : null;

        public IRoad NextRoad(IRoad road) =>
            road.RoadIndex < roads.Count - 1 ? roads[road.RoadIndex + 1] : null;

        // ── 跨 Road 查询 ─────────────────────────────────────────────

        /// <summary>
        /// 根据 Block 实例找到它所属的 Road。
        /// 利用 IndexInRoad + Blocks.Count 做快速范围判断，避免逐一比较引用。
        /// </summary>
        public IRoad GetRoadByBlock(IBlock block)
        {
            foreach (var road in roads)
            {
                var idx = block.IndexInRoad;
                if (idx >= 0 && idx < road.Blocks.Count && road.Blocks[idx] == block)
                    return road;
            }
            // 回退：逐引用比较
            foreach (var road in roads)
                foreach (var b in road.Blocks)
                    if (b == block) return road;
            return null;
        }

        /// <summary>
        /// 返回 block 在整个 Map 中的全局 Block 索引（各 Road 顺序累加）。
        /// </summary>
        public int GetGlobalBlockIndex(IBlock block)
        {
            int offset = 0;
            foreach (var road in roads)
            {
                foreach (var b in road.Blocks)
                {
                    if (b == block) return offset + b.IndexInRoad;
                }
                offset += road.Blocks.Count;
            }
            return -1;
        }

        /// <summary>返回下一条 Road 的第一个 Block（Road 末尾传送用）。</summary>
        public IBlock GetFirstBlockOfNextRoad(IRoad currentRoad)
        {
            var next = NextRoad(currentRoad);
            return next?.FirstBlock;
        }

        // ── 编辑器辅助 ───────────────────────────────────────────────

        /// <summary>重新为所有 Road 分配 RoadIndex（0-based 连续）。</summary>
        [Button("Reindex Roads")]
        public void ReindexRoads()
        {
            for (int i = 0; i < roads.Count; i++)
                roads[i].RoadIndex = i;
        }
    }
}

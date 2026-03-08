using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBallOld.Interfaces;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    /// <summary>
    /// 经典模式轨道。
    /// 通过 <see cref="BlockPlacementData"/> 按 IndexInRoad 读取每个 Block 的放置数据，
    /// Block 自身只保存 index 和 Transform，不再序列化任何放置属性。
    /// </summary>
    public class ClassicRoad : SerializedBehaviour, IRoad
    {
        // ── Inspector 绑定 ───────────────────────────────────────────
        [SerializeField] protected IMap map;
        [SerializeField] protected IRoadMaker roadMaker;

        [Tooltip("与本 Road 关联的放置数据文件（ScriptableObject）。")]
        [SerializeField] protected BlockPlacementData placementData;

        [Tooltip("本 Road 对应音符数据的起始索引（包含）。")]
        [ReadOnly] [SerializeField] protected int noteStartIndex;
        [Tooltip("本 Road 对应音符数据的结束索引（包含）。")]
        [SerializeField] protected int noteEndIndex;

        // ── 运行时数据 ───────────────────────────────────────────────
        protected List<IBlock> blocks = new List<IBlock>();

        // ── 基础属性 ─────────────────────────────────────────────────
        public Transform Transform => transform;
        public IRoadMaker RoadMaker => roadMaker;
        public int RoadIndex { get; set; }
        public int NoteStartIndex => noteStartIndex;
        public int NoteEndIndex => noteEndIndex;

        // ── 数据源 ───────────────────────────────────────────────────
        public BlockPlacementData PlacementData => placementData;

        public BlockEntry GetBlockEntry(IBlock block) =>
            placementData != null ? placementData.GetEntry(block.IndexInRoad) : new BlockEntry(block.IndexInRoad);

        public BlockEntry GetBlockEntry(int indexInRoad) =>
            placementData != null ? placementData.GetEntry(indexInRoad) : new BlockEntry(indexInRoad);

        // ── Block 列表 ───────────────────────────────────────────────
        public List<IBlock> Blocks => blocks;
        public IBlock FirstBlock => blocks.Count > 0 ? blocks[0] : null;
        public IBlock LastBlock => blocks.Count > 0 ? blocks[^1] : null;

        // ── Block 导航 ───────────────────────────────────────────────
        public IBlock PreviousBlock(IBlock block) =>
            block.IndexInRoad <= 0 ? null : blocks[block.IndexInRoad - 1];

        public IBlock NextBlock(IBlock block) =>
            block.IndexInRoad >= blocks.Count - 1 ? null : blocks[block.IndexInRoad + 1];

        public IEnumerable<IBlock> BlocksBefore(IBlock block) =>
            blocks.Take(block.IndexInRoad);

        // ── Corner / Tap 辅助 ────────────────────────────────────────

        private bool IsTapBlock(IBlock block) => GetBlockEntry(block).HasTap;
        private bool IsCornerBlock(IBlock block) => GetBlockEntry(block).HasTap || GetBlockEntry(block).HasRule;

        /// <summary>返回 block 之后第一个 Tap Block 的索引；若不存在则返回末尾索引。</summary>
        private int NextTapBlockIndex(IBlock block)
        {
            for (int i = block.IndexInRoad + 1; i < blocks.Count; i++)
                if (IsTapBlock(blocks[i])) return i;
            return blocks.Count - 1;
        }

        /// <summary>返回 block 之后第一个 Corner Block 的索引；若不存在则返回末尾索引。</summary>
        private int NextCornerBlockIndex(IBlock block)
        {
            for (int i = block.IndexInRoad + 1; i < blocks.Count; i++)
                if (IsCornerBlock(blocks[i])) return i;
            return blocks.Count - 1;
        }

        private IEnumerable<IBlock> GetRange(int startIndex, int endIndex) =>
            blocks.GetRange(startIndex, endIndex - startIndex + 1);

        public IEnumerable<IBlock> BlocksTillNextTap(IBlock block) =>
            GetRange(block.IndexInRoad, NextTapBlockIndex(block));

        public IEnumerable<IBlock> BlocksTillNextCorner(IBlock block) =>
            GetRange(block.IndexInRoad, NextCornerBlockIndex(block));

        /// <summary>
        /// 将 block 之后的方块按 Corner 拆成若干段。
        /// 每段首尾均为 Corner，例：[cur,A,B,C][C,D,...,K][K,...,last]。
        /// </summary>
        public IEnumerable<IEnumerable<IBlock>> BlocksBehindSplitByCorner(IBlock block)
        {
            var result = new List<List<IBlock>>();
            while (NextBlock(block) != null)
            {
                int nextCornerIdx = NextCornerBlockIndex(block);
                result.Add(GetRange(block.IndexInRoad, nextCornerIdx).ToList());
                block = blocks[nextCornerIdx];
            }
            return result;
        }

        // ── 时间 / 运动数据 ──────────────────────────────────────────

        private InputNoteData InputNoteData => map?.NoteData;

        public double GetTimeAtBlock(IBlock block)
        {
            if (InputNoteData == null)
            {
                Debug.LogError("[ClassicRoad] NoteData 未绑定。");
                return -1;
            }
            int noteIndex = NoteStartIndex + block.IndexInRoad;
            if (noteIndex < NoteStartIndex || noteIndex > NoteEndIndex)
            {
                Debug.LogError($"[ClassicRoad] Block index {block.IndexInRoad} 超出音符范围 [{NoteStartIndex},{NoteEndIndex}]。");
                return -1;
            }
            return InputNoteData.noteLists[0].GetNoteTimeAt(noteIndex);
        }

        public List<MovingData> GetMovingDatas(IBlock beginBlock)
        {
            var end = NextTapBlockIndex(beginBlock);
            var result = new List<MovingData>();
            for (int i = beginBlock.IndexInRoad; i <= end; i++)
            {
                var block = blocks[i];
                var time = GetTimeAtBlock(block);
                var pair = new System.ValueTuple<double, Vector3>(time, block.Transform.position);
                result.Add(new MovingData(block, new List<System.ValueTuple<double, Vector3>> { pair }));
            }
            return result;
        }

        // ── 编辑器辅助 ───────────────────────────────────────────────

        /// <summary>
        /// 根据 NoteData 中 [NoteStartIndex, NoteEndIndex] 范围内的 Tap 信息
        /// 刷新 PlacementData 中的 HasTap 字段（仅编辑器调用）。
        /// </summary>
        [Button("Refresh Tap From NoteData")]
        public void RefreshTapFromNoteData()
        {
            if (InputNoteData == null || placementData == null) return;
            var notes = InputNoteData.noteLists[0].GetNotes();
            for (int i = 0; i < blocks.Count; i++)
            {
                int noteIndex = NoteStartIndex + i;
                var entry = placementData.GetEntry(i);
                bool hasTap = notes.Contains(noteIndex);
                if (hasTap && entry.turnType == TurnType.None)
                    entry.turnType = TurnType.Right; // 默认右转，编辑器中再调整
                else if (!hasTap)
                    entry.turnType = TurnType.None;
                placementData.SetEntry(entry);
            }
        }
    }
}

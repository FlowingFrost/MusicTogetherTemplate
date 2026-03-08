using System;
using MusicTogether.DancingBallOld.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    /// <summary>
    /// 经典模式 Road 制作器。
    /// 描述一条 Road 的 Block 范围和音符范围，
    /// 并在范围变化时通知 EditManager 进行同步。
    /// </summary>
    public class ClassicRoadMaker : SerializedBehaviour, IRoadMaker
    {
        // ── Inspector 绑定 ───────────────────────────────────────────
        [SerializeField] protected IRoad road;

        [Tooltip("本 Road 第一个 Block 在 PlacementData 中的索引（通常为 0）。")]
        [ReadOnly] [SerializeField] protected int blockStartIndex;

        [Tooltip("本 Road 最后一个 Block 在 PlacementData 中的索引（包含）。")]
        [OnValueChanged(nameof(OnRangeChanged))]
        [SerializeField] protected int blockEndIndex;

        [Tooltip("对应音符数据的起始索引。")]
        [ReadOnly] [SerializeField] protected int noteStartIndex;

        [Tooltip("对应音符数据的结束索引（包含）。")]
        [OnValueChanged(nameof(OnRangeChanged))]
        [SerializeField] protected int noteEndIndex;

        // ── IRoadMaker 实现 ──────────────────────────────────────────
        public IRoad Road
        {
            get => road;
            set => road = value;
        }

        public int BlockStartIndex
        {
            get => blockStartIndex;
            set => blockStartIndex = value;
        }

        public int BlockEndIndex
        {
            get => blockEndIndex;
            set { blockEndIndex = value; OnRangeChanged(); }
        }

        public int NoteStartIndex
        {
            get => noteStartIndex;
            set => noteStartIndex = value;
        }

        public int NoteEndIndex
        {
            get => noteEndIndex;
            set { noteEndIndex = value; OnRangeChanged(); }
        }

        public Action UpdateBlockRange => OnRangeChanged;

        // ── 内部 ─────────────────────────────────────────────────────
        private void OnRangeChanged()
        {
            if (EditManager.Instance != null)
                EditManager.Instance.OnBlockRangeChanged(this);
        }
    }
}

using System;
using System.Collections.Generic;
using MusicTogether.DancingBallOld.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallOld
{
    /// <summary>
    /// 单个 Block 的放置数据（纯数据，不持有场景对象引用）。
    /// </summary>
    [Serializable]
    public class BlockEntry
    {
        [ReadOnly] public int index;
        public TurnType turnType;
        public DisplacementType displacementType;

        /// <summary>是否为转弯节点（TurnType != None）</summary>
        public bool HasTap => turnType != TurnType.None;
        /// <summary>是否携带位移规则（DisplacementType != None）</summary>
        public bool HasRule => displacementType != DisplacementType.None;

        public BlockEntry(int index)
        {
            this.index = index;
            this.turnType = TurnType.None;
            this.displacementType = DisplacementType.None;
        }

        public BlockEntry(int index, TurnType turn, DisplacementType displacement)
        {
            this.index = index;
            this.turnType = turn;
            this.displacementType = displacement;
        }
    }

    /// <summary>
    /// 保存一条 Road 内所有 Block 放置信息的 ScriptableObject。
    /// Block 通过 IndexInRoad 与本文件中的 BlockEntry 匹配。
    /// </summary>
    [CreateAssetMenu(menuName = "MusicTogether/BlockPlacementData", fileName = "NewBlockPlacementData")]
    public class BlockPlacementData : ScriptableObject
    {
        [SerializeField] [TableList]
        private List<BlockEntry> entries = new List<BlockEntry>();

        // ────────────────────────────────────────────────────────────
        // CRUD
        // ────────────────────────────────────────────────────────────

        /// <summary>根据 IndexInRoad 获取数据；若不存在则返回默认条目（普通方块）。</summary>
        public BlockEntry GetEntry(int index)
        {
            int i = FindIndex(index);
            return i >= 0 ? entries[i] : new BlockEntry(index);
        }

        /// <summary>写入或更新一条数据。</summary>
        public void SetEntry(BlockEntry entry)
        {
            int i = FindIndex(entry.index);
            if (i >= 0)
                entries[i] = entry;
            else
            {
                entries.Add(entry);
                entries.Sort((a, b) => a.index.CompareTo(b.index));
            }
            MarkDirty();
        }

        /// <summary>批量设置整段 Road 的数据（先清空再写入）。</summary>
        public void SetRange(IEnumerable<BlockEntry> newEntries)
        {
            entries.Clear();
            entries.AddRange(newEntries);
            entries.Sort((a, b) => a.index.CompareTo(b.index));
            MarkDirty();
        }

        /// <summary>将指定 index 的数据重置为默认（普通方块）。</summary>
        public void ResetEntry(int index)
        {
            int i = FindIndex(index);
            if (i >= 0) entries[i] = new BlockEntry(index);
            MarkDirty();
        }

        /// <summary>删除指定 index 的条目（索引不存在时什么也不做）。</summary>
        public void RemoveEntry(int index)
        {
            int i = FindIndex(index);
            if (i >= 0) { entries.RemoveAt(i); MarkDirty(); }
        }

        /// <summary>确保 [0, count) 范围内每个 index 都存在条目（不足则补默认）。</summary>
        public void EnsureCapacity(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (FindIndex(i) < 0)
                    entries.Add(new BlockEntry(i));
            }
            entries.Sort((a, b) => a.index.CompareTo(b.index));
            MarkDirty();
        }

        /// <summary>截断：移除所有 index >= count 的条目。</summary>
        public void Truncate(int count)
        {
            entries.RemoveAll(e => e.index >= count);
            MarkDirty();
        }

        public int Count => entries.Count;

        public IReadOnlyList<BlockEntry> Entries => entries;

        // ────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────

        private int FindIndex(int index)
        {
            // entries 保持有序，可用二分查找
            int lo = 0, hi = entries.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                int cmp = entries[mid].index.CompareTo(index);
                if (cmp == 0) return mid;
                if (cmp < 0) lo = mid + 1; else hi = mid - 1;
            }
            return -1;
        }

        private void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}

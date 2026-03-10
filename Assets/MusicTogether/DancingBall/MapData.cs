using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall
{
    public enum TurnType { None, Left, Right, Forward }
    public enum DisplacementType { None, Up, Down, ForwardUp, ForwardDown }

    /// <summary>
    /// Road数据
    /// </summary>
    public class RoadData
    {
        public int index;
        public int beginBlockIndex;
        
        public RoadData(int index)
        {
            this.index = index;
            beginBlockIndex = int.MaxValue;
        }
    }
    
    /// <summary>
    /// 单个 Block 的放置数据（纯数据，不持有场景对象引用）。
    /// Block所有的拐弯模式：(TurnType + DisplacementType) 的组合
    /// 1.需要点击转向：
    /// Left/Right + None：在当前水平面内分别向左/右转90度。
    /// Left/right + Up/Down：左右转90°+上下转90°
    /// Forward + None：跳跃，继续向前但是空出2个位置
    /// 2.不需要点击
    /// None + Up/Down：向上/下转90°
    /// None + ForwardUp/ForwardDown：向上/下转45°(并不真正旋转45°，而是按照切比雪夫距离在斜对角上移动1单位)
    /// 其余组合不合法，视为 None + None（普通方块）
    /// </summary>
    [Serializable]
    public class BlockData
    {
        public int blockGlobalIndex;
        public TurnType turnType;
        public DisplacementType displacementType;
        
        public bool HasTurn => turnType != TurnType.None;
        public bool HasDisplacement => displacementType != DisplacementType.None;
        public BlockData(int blockGlobalIndex)
        {
            this.blockGlobalIndex = blockGlobalIndex;
            this.turnType = TurnType.None;
            this.displacementType = DisplacementType.None;
        }
    }
    
    /// <summary>
    /// 保存 Map 内含有自定义规则的 Block 放置信息的 ScriptableObject。
    /// Block 通过 IndexInRoad 与本文件中的 BlockEntry 匹配。
    /// </summary>
    [CreateAssetMenu(menuName = "MusicTogether/DB_MaoData", fileName = "NewMapPlacementData")]
    public class MapData : SerializedScriptableObject
    {
        public InputNoteData inputNoteData;
        private List<int> Notes => inputNoteData.noteLists[0].notes;
        public int totalBlockCount;
        public int totalRoadCount => roadDataList.Count;
        public int BlockDataCount => blockDataList.Count;
        [ListDrawerSettings(CustomAddFunction = nameof(AddRoadData))]
        public List<RoadData> roadDataList = new List<RoadData>();

        [ListDrawerSettings(CustomAddFunction = nameof(AddBlockData))]
        public List<BlockData> blockDataList = new List<BlockData>();

        private RoadData AddRoadData()
        {
            int nextIndex = roadDataList.Count > 0 ? roadDataList[^1].index + 1 : 0;
            return new RoadData(nextIndex);
        }

        private BlockData AddBlockData()
        {
            int nextIndex = blockDataList.Count > 0 ? blockDataList[^1].blockGlobalIndex + 1 : 0;
            return new BlockData(nextIndex);
        }
        
        // ────────────────────────────────────────────────────────────
        // CRUD
        // ────────────────────────────────────────────────────────────
        public bool IsValidRoadIndex(int index) => index >= 0 && index < totalRoadCount;
        public void GetRoadData(int index, out RoadData data)
        {
            int i = FindRoadIndexInDataList(index);
            if (i < 0)
            {
                data = new RoadData(index);
                SetRoadData(data);
            }
            else data = roadDataList[i];
        }
        
        public void SetRoadData(RoadData entry)
        {
            int i = FindRoadIndexInDataList(entry.index);
            if (i >= 0)
                roadDataList[i] = entry;
            else
            {
                roadDataList.Add(entry);
                roadDataList.Sort((a, b) => a.index.CompareTo(b.index));
            }
            MarkDirty();
        }
        
        public void RemoveRoadData(int index)
        {
            int i = FindRoadIndexInDataList(index);
            if (i >= 0) { roadDataList.RemoveAt(i); MarkDirty(); }
        }
        
        public int GetRoadEndBlockIndex(int roadIndex)
        {
            GetRoadData(roadIndex, out var data);
            return roadDataList.Find(r => r.index == roadIndex+1)?.beginBlockIndex ?? totalBlockCount - 1;
        }

        public List<int> RoadNotes(int index)
        {
            GetRoadData(index, out var data);
            int blockBegin = data.beginBlockIndex;
            int blockEnd = GetRoadEndBlockIndex(index);
            return Notes.Select(n => n >= blockBegin && n <= blockEnd ? n : -1).Where(n => n > 0).ToList();
        }
        
        
        
        
        public bool IsValidBlockIndex(int blockGlobalIndex) => blockGlobalIndex >= 0 && blockGlobalIndex < totalBlockCount;
        /// <summary>根据 IndexInRoad 获取数据；若不存在则返回默认条目（普通方块）。</summary>
        public bool GetBlockData(int blockGlobalIndex, out BlockData data)
        {
            int i = FindBlockIndexInDataList(blockGlobalIndex);
            data = i >= 0 ? blockDataList[i] : new BlockData(blockGlobalIndex);
            return i >= 0;
        }
        
        /// <summary>写入或更新一条数据。</summary>
        public void SetBlockData(BlockData entry)
        {
            int i = FindBlockIndexInDataList(entry.blockGlobalIndex);
            if (i >= 0)
                blockDataList[i] = entry;
            else
            {
                blockDataList.Add(entry);
                blockDataList.Sort((a, b) => a.blockGlobalIndex.CompareTo(b.blockGlobalIndex));
            }
            MarkDirty();
        }
        
        /// <summary>删除指定 index 的条目（索引不存在时什么也不做）。</summary>
        public void RemoveBlockData(int blockGlobalIndex)
        {
            int i = FindBlockIndexInDataList(blockGlobalIndex);
            if (i >= 0) { blockDataList.RemoveAt(i); MarkDirty(); }
        }

        public bool HasBlockData(int blockGlobalIndex)
        {
            if (GetBlockData(blockGlobalIndex, out var data))
            {
                return data.HasTurn || data.HasDisplacement;
            }
            return false;
        }
        public bool BlockHasTap(int blockGlobalIndex) => Notes.Exists(n => n == blockGlobalIndex);

        /// <summary>
        /// 从 blockIndex 向前（不含自身）查找最近一个满足 HasTurn 或 HasDisplacement 的 BlockData。
        /// 找到则返回 true，并通过 data 输出结果；否则返回 false，data 为 null。
        /// </summary>
        public bool FindLastDataedBlockIndex(int blockIndex, out int index)
        {
            for (int i = blockDataList.Count - 1; i >= 0; i--)
            {
                var entry = blockDataList[i];
                if (entry.blockGlobalIndex >= blockIndex) continue;
                if (entry.HasTurn || entry.HasDisplacement)
                {
                    index = entry.blockGlobalIndex;
                    return true;
                }
            }
            index = 0;
            return false;
        }
        public bool FindNextDataedBlockIndex(int blockIndex, out int index)
        {
            index = totalBlockCount;
            for (int i = blockDataList.Count - 1; i >= 0; i--)
            {
                var entry = blockDataList[i];
                if (entry.blockGlobalIndex < blockIndex) return false;
                if (entry.HasTurn || entry.HasDisplacement)
                {
                    index = entry.blockGlobalIndex;
                    return true;
                }
            }
            return false;
        }

        public IReadOnlyList<RoadData> RoadDatas => roadDataList;
        public IReadOnlyList<BlockData> BlockDatas => blockDataList;
        // ────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────

        private int FindRoadIndexInDataList(int index) => roadDataList.FindIndex(entry => entry.index == index);
        private int FindBlockIndexInDataList(int blockGlobalIndex) => blockDataList.FindIndex(entry => entry.blockGlobalIndex == blockGlobalIndex);

        /// <summary>
        /// 双重保险，暂时不必要
        /// </summary>
        private void RefreshRoadData()
        {
            for (int i = 0; i < totalRoadCount; i++)
            {
                if (roadDataList.Find(r => r.index == i) == null)
                {
                    roadDataList.Add(new RoadData(i));
                }
            }
            roadDataList.Sort((a, b) => a.index.CompareTo(b.index));
            MarkDirty();
        }
        private void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
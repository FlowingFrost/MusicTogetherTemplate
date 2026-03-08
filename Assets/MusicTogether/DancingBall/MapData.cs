using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public enum TurnType { None, Left, Right, Up, Down }
    public enum DisplacementType { None, Up, Down, Forward }

    /// <summary>
    /// Road数据
    /// </summary>
    public class RoadData
    {
        [ReadOnly] public int index;
        public int beginBlockIndex;
        
        public RoadData(int index)
        {
            this.index = index;
        }
    }
    
    /// <summary>
    /// 单个 Block 的放置数据（纯数据，不持有场景对象引用）。
    /// </summary>
    [Serializable]
    public class BlockData
    {
        [ReadOnly] public int index;
        public TurnType turnType;
        public DisplacementType displacementType;
        public BlockData(int index)
        {
            this.index = index;
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
        public List<RoadData> roadDataList = new List<RoadData>();
        public List<BlockData> blockDataList = new List<BlockData>();
        
        // ────────────────────────────────────────────────────────────
        // CRUD
        // ────────────────────────────────────────────────────────────
        public void GetRoadData(int index, out RoadData data)
        {
            int i = FindRoadIndex(index);
            if (i < 0)
            {
                data = new RoadData(index);
                SetRoadData(data);
            }
            else data = roadDataList[i];
        }
        
        public void SetRoadData(RoadData entry)
        {
            int i = FindRoadIndex(entry.index);
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
            int i = FindRoadIndex(index);
            if (i >= 0) { roadDataList.RemoveAt(i); MarkDirty(); }
        }
        
        public int GetRoadEndBlockIndex(int roadIndex)
        {
            GetRoadData(roadIndex, out var data);
            return roadDataList.Find(r => r.index == roadIndex+1)?.beginBlockIndex ?? BlockCount - 1;
        }

        public List<int> RoadNotes(int index)
        {
            GetRoadData(index, out var data);
            int blockBegin = data.beginBlockIndex;
            int blockEnd = GetRoadEndBlockIndex(index);
            return Notes.Select(n => n >= blockBegin && n <= blockEnd ? n : -1).Where(n => n > 0).ToList();
        }
        
        /// <summary>根据 IndexInRoad 获取数据；若不存在则返回默认条目（普通方块）。</summary>
        public bool GetBlockData(int index, out BlockData data)
        {
            int i = FindBlockIndex(index);
            data = i >= 0 ? blockDataList[i] : null;
            return i >= 0;
        }
        
        /// <summary>写入或更新一条数据。</summary>
        public void SetBlockData(BlockData entry)
        {
            int i = FindBlockIndex(entry.index);
            if (i >= 0)
                blockDataList[i] = entry;
            else
            {
                blockDataList.Add(entry);
                blockDataList.Sort((a, b) => a.index.CompareTo(b.index));
            }
            MarkDirty();
        }
        
        /// <summary>删除指定 index 的条目（索引不存在时什么也不做）。</summary>
        public void RemoveBlockData(int index)
        {
            int i = FindBlockIndex(index);
            if (i >= 0) { blockDataList.RemoveAt(i); MarkDirty(); }
        }
        
        public int RoadCount => roadDataList.Count;
        public int BlockCount => blockDataList.Count;
        public IReadOnlyList<RoadData> RoadDatas => roadDataList;
        public IReadOnlyList<BlockData> BlockDatas => blockDataList;
        // ────────────────────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────────────────────

        private int FindRoadIndex(int index) => roadDataList.FindIndex(entry => entry.index == index);
        private int FindBlockIndex(int index) => blockDataList.FindIndex(entry => entry.index == index);

        /// <summary>
        /// 双重保险，暂时不必要
        /// </summary>
        private void RefreshRoadData()
        {
            for (int i = 0; i < RoadCount; i++)
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
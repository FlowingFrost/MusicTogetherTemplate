using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall
{
    public enum TurnType { None, Forward, Left, Right, Jump }
    public enum DisplacementType { None, Up, Down, ForwardUp, ForwardDown }

    /// <summary>
    /// Road数据
    /// </summary>
    public class RoadData
    {
        public int roadGlobalIndex;
        public int beginBlockIndex;
        
        public RoadData(int roadGlobalIndex)
        {
            this.roadGlobalIndex = roadGlobalIndex;
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
        // ========== 可序列化字段 ==========
        public InputNoteData inputNoteData;
        public int totalBlockCount;

        [ListDrawerSettings(CustomAddFunction = nameof(AddRoadData))]
        public List<RoadData> roadDataList = new List<RoadData>();

        [ListDrawerSettings(CustomAddFunction = nameof(AddBlockData))]
        public List<BlockData> blockDataList = new List<BlockData>();

        // ========== 私有属性 ==========
        private List<int> Notes => inputNoteData.noteLists[0].notes;

        // ========== 公共只读属性 ==========
        public int totalRoadCount => roadDataList.Count;
        public int BlockDataCount => blockDataList.Count;
        public IReadOnlyList<RoadData> RoadDatas => roadDataList;
        public IReadOnlyList<BlockData> BlockDatas => blockDataList;

        // ========== Road 相关 CRUD ==========
        public bool IsValidRoadIndex(int index) => index >= 0 && index < totalRoadCount;

        public void GetRoadData(int index, out RoadData data)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(index);
            if (i < 0)
            {
                data = new RoadData(index);
                SetRoadData(data);
            }
            else data = roadDataList[i];
        }
        
        public void SetRoadData(RoadData entry)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(entry.roadGlobalIndex);
            if (i >= 0)
                roadDataList[i] = entry;
            else
            {
                roadDataList.Add(entry);
                roadDataList.Sort((a, b) => a.roadGlobalIndex.CompareTo(b.roadGlobalIndex));
            }
            MarkDirty();
        }
        
        public void RemoveRoadData(int index)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(index);
            if (i >= 0) { roadDataList.RemoveAt(i); MarkDirty(); }
        }
        
        /// <summary>
        /// 若不是最后一段路，则返回下一段路的 beginBlockIndex 作为当前段路的 endBlockIndex，要求二者共享这个方块。
        /// </summary>
        public int GetRoadEndBlockIndex(int roadIndex)
        {
            GetRoadData(roadIndex, out var data);
            return roadDataList.Find(r => r.roadGlobalIndex == roadIndex+1)?.beginBlockIndex ?? totalBlockCount - 1;
        }
        
        public List<int> RoadNotes(int index)
        {
            GetRoadData(index, out var data);
            int blockBegin = data.beginBlockIndex;
            int blockEnd = GetRoadEndBlockIndex(index);
            return Notes.Select(n => n >= blockBegin && n <= blockEnd ? n : -1).Where(n => n > 0).ToList();
        }

        // ========== Block 相关 CRUD ==========
        public bool InRange_ByBlockGlobalIndex(int blockGlobalIndex) => blockGlobalIndex >= 0 && blockGlobalIndex < totalBlockCount;

        /// <summary>根据 IndexInRoad 获取数据；若不存在则返回默认条目（普通方块）。</summary>
        public bool GetBlockData_ByBlockGlobalIndex(int blockGlobalIndex, out BlockData data)
        {
            int i = Find_BlockDataListIndex_ByBlockGlobalIndex(blockGlobalIndex);
            data = i >= 0 ? blockDataList[i] : new BlockData(blockGlobalIndex);
            return i >= 0;
        }
        
        /// <summary>写入或更新一条数据。</summary>
        public void SetBlockData(BlockData entry)
        {
            int i = Find_BlockDataListIndex_ByBlockGlobalIndex(entry.blockGlobalIndex);
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
            int i = Find_BlockDataListIndex_ByBlockGlobalIndex(blockGlobalIndex);
            if (i >= 0) { blockDataList.RemoveAt(i); MarkDirty(); }
        }

        public bool HasDisplacementRule_ByBlockGlobalIndex(int blockGlobalIndex)
        {
            if (GetBlockData_ByBlockGlobalIndex(blockGlobalIndex, out var data))
            {
                return data.HasTurn || data.HasDisplacement;
            }
            return false;
        }

        public bool HasTap_ByBlockGlobalIndex(int roadIndex, int blockGlobalIndex) => Notes.Exists(n => n == blockGlobalIndex);
        public bool IsBeginBlock_ByBlockGlobalIndex(int roadGlobalIndex, int blockGlobalIndex) => roadDataList.Find(r => r.roadGlobalIndex == roadGlobalIndex).beginBlockIndex == blockGlobalIndex;

        // ========== 查找方法（特殊方块、音符） ==========
        /// <summary>
        /// 从 blockIndex 向前（不含自身）查找最近一个满足 HasTurn 或 HasDisplacement 的 BlockData。
        /// </summary>
        public bool TryGetPrevious_BlockDataGlobalIndex_WhichHasDisplacementRule(int searchBeginBlockIndex, out int resultBlockGlobalIndex)
        {
            for (int i = blockDataList.Count - 1; i >= 0; i--)
            {
                var entry = blockDataList[i];
                if (entry.blockGlobalIndex >= searchBeginBlockIndex) continue;
                if (entry.HasTurn || entry.HasDisplacement)
                {
                    resultBlockGlobalIndex = entry.blockGlobalIndex;
                    return true;
                }
            }
            resultBlockGlobalIndex = 0;
            return false;
        }

        public bool TryGetNext_BlockDataGlobalIndex_WhichHasDisplacementRule(int searchBeginBlockIndex, out int resultBlockGlobalIndex)
        {
            resultBlockGlobalIndex = totalBlockCount;
            for (int i = blockDataList.Count - 1; i >= 0; i--)
            {
                var entry = blockDataList[i];
                if (entry.blockGlobalIndex < searchBeginBlockIndex) return false;
                if (entry.HasTurn || entry.HasDisplacement)
                {
                    resultBlockGlobalIndex = entry.blockGlobalIndex;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从 blockIndex 向前（不含自身）查找最近一个序号存在于 Notes 的 BlockData。
        /// </summary>
        public bool TryGetPrevious_BlockDataGlobalIndex_WhichNeedTap(int searchBeginBlockIndex, out int resultBlockGlobalIndex)
        {
            for (int i = Notes.Count - 1; i >= 0; i--)
            {
                var note = Notes[i];
                if (note >= searchBeginBlockIndex) continue;
                resultBlockGlobalIndex = note;
                return true;
            }
            resultBlockGlobalIndex = 0;
            return false;
        }
        public bool TryGetNext_BlockDataGlobalIndex_WhichNeedTap(int searchBeginBlockIndex, out int resultBlockGlobalIndex)
        {
            resultBlockGlobalIndex = totalBlockCount;
            for (int i = 0; i < Notes.Count; i++)
            {
                var note = Notes[i];
                if (note < searchBeginBlockIndex) continue;
                resultBlockGlobalIndex = note;
                return true;
            }
            return false;
        }

        // ========== 私有辅助方法 ==========
        private int Find_RoadDataListIndex_ByRoadGlobalIndex(int roadGlobalIndex) => roadDataList.FindIndex(entry => entry.roadGlobalIndex == roadGlobalIndex);
        private int Find_BlockDataListIndex_ByBlockGlobalIndex(int blockGlobalIndex) => blockDataList.FindIndex(entry => entry.blockGlobalIndex == blockGlobalIndex);

        private RoadData AddRoadData()
        {
            int nextIndex = roadDataList.Count > 0 ? roadDataList[^1].roadGlobalIndex + 1 : 0;
            return new RoadData(nextIndex);
        }

        private BlockData AddBlockData()
        {
            int nextIndex = blockDataList.Count > 0 ? blockDataList[^1].blockGlobalIndex + 1 : 0;
            return new BlockData(nextIndex);
        }

        /// <summary>
        /// 双重保险，暂时不必要
        /// </summary>
        private void RefreshRoadData()
        {
            for (int i = 0; i < totalRoadCount; i++)
            {
                if (roadDataList.Find(r => r.roadGlobalIndex == i) == null)
                {
                    roadDataList.Add(new RoadData(i));
                }
            }
            roadDataList.Sort((a, b) => a.roadGlobalIndex.CompareTo(b.roadGlobalIndex));
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
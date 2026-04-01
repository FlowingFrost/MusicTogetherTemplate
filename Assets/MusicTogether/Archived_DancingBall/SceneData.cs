using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.DancingBall
{
    public enum TurnType { None, Forward, Left, Right, Jump }

    public enum DisplacementType { None, Up, Down, ForwardUp, ForwardDown }

    /// <summary>
    /// Road数据
    /// </summary>
    public class RoadData
    {
        public int RoadGlobalIndex;
        public int TargetSegmentIndex;
        public int NoteBeginIndex;
        public int NoteEndIndex;
        public int BlockCount => NoteEndIndex - NoteBeginIndex + 1;
        [ListDrawerSettings(DefaultExpandedState = false)]
        public List<BlockData> blockDataList = new List<BlockData>();

        public RoadData(int roadGlobalIndex, int targetSegmentIndex = 0, int noteBeginIndex = -1, int noteEndIndex = -1)
        {
            this.RoadGlobalIndex = roadGlobalIndex;
            this.TargetSegmentIndex = targetSegmentIndex;
            this.NoteBeginIndex = noteBeginIndex;
            this.NoteEndIndex = noteEndIndex;
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
        public int blockLocalIndex;
        public TurnType turnType;
        public DisplacementType displacementType;

        public bool HasTurn => turnType != TurnType.None;
        public bool HasDisplacement => displacementType != DisplacementType.None;

        public BlockData(int blockLocalIndex)
        {
            this.blockLocalIndex = blockLocalIndex;
            this.turnType = TurnType.None;
            this.displacementType = DisplacementType.None;
        }
    }

    /// <summary>
    /// 保存 Map 内含有自定义规则的 Block 放置信息的 ScriptableObject。
    /// Block 通过 IndexInRoad 与本文件中的 BlockEntry 匹配。
    /// </summary>
    [CreateAssetMenu(menuName = "MusicTogether/DB_SceneData", fileName = "NewSceneData")]
    public class SceneData : SerializedScriptableObject
    {
        // ========== 可序列化字段 ==========
        public InputNoteData inputNoteData;
        public List<Segemnt> InputNotes => inputNoteData.noteLists;

        [ListDrawerSettings(CustomAddFunction = nameof(AddRoadData))]
        public List<RoadData> roadDataList = new List<RoadData>();

        // ========== Note操作 ==========

        private bool GetNoteDataIndex(int targetNoteListIndex, out int InputNoteDataListIndex)
        {
            InputNoteDataListIndex = -1;
            InputNoteDataListIndex = InputNotes.FindIndex(l => l.Index == targetNoteListIndex);
            if (InputNoteDataListIndex > 0)
            {
                return true;
            }
            return false;
        }
        private bool GetNoteList(int targetNoteListIndex, out List<int> notes)
        {
            var targetListIndex = InputNotes.FindIndex(l => l.Index == targetNoteListIndex);
            if (targetListIndex > 0)
            {
                notes = InputNotes[targetListIndex].notes;
                return true;
            }
            notes = null;
            return false;
        }
        public bool HasTap(int roadIndex, int blockLocalIndex)
        {
            if (!Is_BlockIndex_InRange(roadIndex, blockLocalIndex) || !TryGetRoadData(roadIndex, out var roadData))
            {
                return false;
            }
            if (!GetNoteList(roadData.TargetSegmentIndex, out var Notes) || Notes == null || Notes.Count == 0)
            {
                return false;
            }

            int noteGlobalIndex = roadData.NoteBeginIndex + blockLocalIndex;
            return Notes.Exists(n => n == noteGlobalIndex);
        }
        public bool GetNoteTime(int roadIndex, float blockLocalIndex, out double time)
        {
            time = 0f;
            if (!Is_BlockIndex_InRange(roadIndex, blockLocalIndex) || !TryGetRoadData(roadIndex, out var roadData))
            {
                return false;
            }
            if (!GetNoteDataIndex(roadData.TargetSegmentIndex, out var noteDataListIndex))
            {
                return false;
            }

            var noteData = InputNotes[noteDataListIndex];
            float noteGlobalIndex = roadData.NoteBeginIndex + blockLocalIndex;
            time = noteData.GetNoteTimeAt(noteGlobalIndex);
            return true;
        }
        public bool GetNoteLenth(int roadIndex, out double time)
        {
            return GetNoteTime(roadIndex, 1, out time) && GetNoteTime(roadIndex, 0, out var beginTime) && (time - beginTime > 0);
        }

        // ========== 公共只读属性 ==========
        public int TotalRoadCount => roadDataList.Count;
        public int TotalBlockCount => roadDataList.Sum(roadData => Mathf.Max((int)0, (int)roadData.BlockCount));
        public IReadOnlyList<RoadData> RoadDatas => roadDataList;

        // ========== Road 相关 CRUD ==========
        public bool IsValidRoadIndex(int roadGlobalIndex) => Find_RoadDataListIndex_ByRoadGlobalIndex(roadGlobalIndex) >= 0;

        public bool GetRoadData(int roadGlobalIndex, out RoadData data)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(roadGlobalIndex);
            if (i < 0)
            {
                data = new RoadData(roadGlobalIndex);
                SetRoadData(data);
                return false;
            }
            else
            {
                data = roadDataList[i];
                return true;
            }
        }

        public void SetRoadData(RoadData entry)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(entry.RoadGlobalIndex);
            if (i >= 0)
            {
                roadDataList[i] = entry;
            }
            else
            {
                roadDataList.Add(entry);
                roadDataList.Sort((a, b) => a.RoadGlobalIndex.CompareTo(b.RoadGlobalIndex));
            }

            entry.blockDataList ??= new List<BlockData>();
            SortBlockData(entry);
            MarkDirty();
        }

        public void RemoveRoadData(int roadGlobalIndex)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(roadGlobalIndex);
            if (i >= 0)
            {
                roadDataList.RemoveAt(i);
                MarkDirty();
            }
        }

        // ========== Block 相关 CRUD ==========
        public bool Is_BlockIndex_InRange(int roadIndex, float blockLocalIndex)
        {
            if (!TryGetRoadData(roadIndex, out var roadData) || blockLocalIndex < 0)
            {
                return false;
            }

            return blockLocalIndex < Mathf.Max(0, roadData.BlockCount);
        }

        public bool GetBlockData(int roadIndex, int blockLocalIndex, out BlockData data)
        {
            if (!Is_BlockIndex_InRange(roadIndex, blockLocalIndex) || !TryGetRoadData(roadIndex, out var roadData))
            {
                data = new BlockData(0);
                return false;
            }

            int i = Find_BlockDataListIndex_ByBlockLocalIndex(roadData, blockLocalIndex);
            data = i >= 0 ? roadData.blockDataList[i] : new BlockData(blockLocalIndex);
            return i >= 0;
        }

        public void SetBlockData(int roadIndex, BlockData entry)
        {
            if (!TryGetRoadData(roadIndex, out var roadData))
            {
                return;
            }

            roadData.blockDataList ??= new List<BlockData>();
            int i = Find_BlockDataListIndex_ByBlockLocalIndex(roadData, entry.blockLocalIndex);
            if (i >= 0)
            {
                roadData.blockDataList[i] = entry;
            }
            else
            {
                if (!Is_BlockIndex_InRange(roadIndex, entry.blockLocalIndex)) return;
                roadData.blockDataList.Add(entry);
                SortBlockData(roadData);
            }
            MarkDirty();
        }

        public void SetBlockData(int roadIndex, int blockLocalIndex, BlockData entry)
        {
            if (!Is_BlockIndex_InRange(roadIndex, entry.blockLocalIndex)) return;
            entry.blockLocalIndex = blockLocalIndex;
            SetBlockData(roadIndex, entry);
        }

        public void RemoveBlockData(int roadIndex, int blockLocalIndex)
        {
            if (!TryGetRoadData(roadIndex, out var roadData) || roadData.blockDataList == null)
            {
                return;
            }

            int i = Find_BlockDataListIndex_ByBlockLocalIndex(roadData, blockLocalIndex);
            if (i >= 0)
            {
                roadData.blockDataList.RemoveAt(i);
                MarkDirty();
            }
        }

        public void SetBlockData_TurnType(int roadIndex, int localIndex, TurnType type)
        {
            if (!GetRoadData(roadIndex, out var roadData)) return;

            var blockData = roadData.blockDataList.FirstOrDefault(b => b.blockLocalIndex == localIndex);
            if (blockData == null)
            {
                if (type == TurnType.None) return;
                blockData = new BlockData(localIndex);
                roadData.blockDataList.Add(blockData);
            }
            blockData.turnType = type;

            if (!blockData.HasTurn && !blockData.HasDisplacement)
            {
                roadData.blockDataList.Remove(blockData);
            }
            MarkDirty();
        }

        public void SetBlockData_DisplacementType(int roadIndex, int localIndex, DisplacementType type)
        {
            if (!GetRoadData(roadIndex, out var roadData)) return;

            var blockData = roadData.blockDataList.FirstOrDefault(b => b.blockLocalIndex == localIndex);
            if (blockData == null)
            {
                if (type == DisplacementType.None) return;
                blockData = new BlockData(localIndex);
                roadData.blockDataList.Add(blockData);
            }
            blockData.displacementType = type;

            if (!blockData.HasTurn && !blockData.HasDisplacement)
            {
                roadData.blockDataList.Remove(blockData);
            }
            MarkDirty();
        }

        public bool HasDisplacementRule(int roadIndex, int blockLocalIndex)
        {
            if (GetBlockData(roadIndex, blockLocalIndex, out var data))
            {
                return data.HasTurn || data.HasDisplacement;
            }
            return false;
        }
        
        public bool IsBeginBlock(int roadIndex, int blockLocalIndex) => Is_BlockIndex_InRange(roadIndex, blockLocalIndex) && blockLocalIndex == 0;

        // ========== 查找方法（特殊方块、音符） ==========
        public bool TryGetPrevious_Block_InCurrentRoad_WhichHasDisplacementRule(int searchRoadIndex, int searchBlockLocalIndex, out int resultBlockLocalIndex)
        {
            resultBlockLocalIndex = -1;
            if (searchBlockLocalIndex < 0) return false;

            if (!GetRoadData(searchRoadIndex, out var data)) return false;
            if (searchBlockLocalIndex >= data.BlockCount) searchBlockLocalIndex = data.BlockCount;
            
            for (int local = searchBlockLocalIndex - 1; local >= 0; local--)
            {
                if (HasDisplacementRule(searchRoadIndex, local))
                {
                    resultBlockLocalIndex = local;
                    return true;
                }
            }

            resultBlockLocalIndex = 0;
            return false;
        }
        public bool TryGetNext_Block_InCurrentRoad_WhichHasDisplacementRule(int searchRoadIndex, int searchBlockLocalIndex, out int resultBlockLocalIndex)
        {
            resultBlockLocalIndex = -1;
            
            if (!GetRoadData(searchRoadIndex, out var data)) return false;
            if (searchBlockLocalIndex >= data.BlockCount) { return false; }

            if (searchBlockLocalIndex < 0) searchBlockLocalIndex = -1;
            
            for (int local = searchBlockLocalIndex + 1; local < data.BlockCount; local++)
            {
                if (HasDisplacementRule(searchRoadIndex, local))
                {
                    resultBlockLocalIndex = local;
                    return true;
                }
            }
            resultBlockLocalIndex = data.NoteEndIndex;
            return false;
        }
        public bool TryGetPrevious_Block_InCurrentRoad_WhichNeedTap(int searchRoadIndex, int searchBlockLocalIndex, out int resultBlockLocalIndex)
        {
            resultBlockLocalIndex = -1;
            if (searchBlockLocalIndex < 0) return false;

            if (!GetRoadData(searchRoadIndex, out var data)) return false;
            if (searchBlockLocalIndex >= data.BlockCount) searchBlockLocalIndex = data.BlockCount;

            if (!GetNoteList(data.TargetSegmentIndex, out var notes)) return false;
            
            for (int local = searchBlockLocalIndex - 1; local >= 0; local--)
            {
                int noteGlobalIndex = data.NoteBeginIndex + local;
                if (notes.Exists(n => n == noteGlobalIndex))
                {
                    resultBlockLocalIndex = local;
                    return true;
                }
            }
            resultBlockLocalIndex = 0;
            return false;
        }
        public bool TryGetNext_Block_InCurrentRoad_WhichNeedTap(int searchRoadIndex, int searchBlockLocalIndex, out int resultBlockLocalIndex)
        {
            resultBlockLocalIndex = -1;
            
            if (!GetRoadData(searchRoadIndex, out var data)) return false;
            if (searchBlockLocalIndex >= data.BlockCount) { return false; }

            if (searchBlockLocalIndex < 0) searchBlockLocalIndex = -1;

            if (!GetNoteList(data.TargetSegmentIndex, out var notes)) return false;
            
            for (int local = searchBlockLocalIndex + 1; local < data.BlockCount; local++)
            {
                int noteGlobalIndex = data.NoteBeginIndex + local;
                if (notes.Exists(n => n == noteGlobalIndex))
                {
                    resultBlockLocalIndex = local;
                    return true;
                }
            }

            resultBlockLocalIndex = data.NoteEndIndex;
            return false;
        }

        private int Find_RoadDataListIndex_ByRoadGlobalIndex(int roadGlobalIndex)
            => roadDataList.FindIndex(entry => entry.RoadGlobalIndex == roadGlobalIndex);

        private bool TryGetRoadData(int roadGlobalIndex, out RoadData data)
        {
            int i = Find_RoadDataListIndex_ByRoadGlobalIndex(roadGlobalIndex);
            if (i < 0)
            {
                data = null;
                return false;
            }

            data = roadDataList[i];
            data.blockDataList ??= new List<BlockData>();
            return true;
        }

        private int Find_BlockDataListIndex_ByBlockLocalIndex(RoadData roadData, int blockLocalIndex)
            => roadData.blockDataList.FindIndex(entry => entry.blockLocalIndex == blockLocalIndex);

        private void SortBlockData(RoadData roadData)
            => roadData.blockDataList.Sort((a, b) => a.blockLocalIndex.CompareTo(b.blockLocalIndex));

        private RoadData AddRoadData()
        {
            int nextIndex = roadDataList.Count > 0 ? roadDataList[^1].RoadGlobalIndex + 1 : 0;
            return new RoadData(nextIndex);
        }

        private void MarkDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
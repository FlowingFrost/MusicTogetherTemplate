using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
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

        [ListDrawerSettings(CustomAddFunction = nameof(AddRoadData))]
        public List<RoadData> roadDataList = new List<RoadData>();

        // ========== 私有属性 ==========
        private List<int> Notes => inputNoteData?.noteLists != null && inputNoteData.noteLists.Count > 0
            ? inputNoteData.noteLists[0].notes
            : null;

        // ========== 公共只读属性 ==========
        public int TotalRoadCount => roadDataList.Count;
        public int TotalBlockCount => roadDataList.Sum(roadData => Mathf.Max(0, roadData.BlockCount));
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

        public List<int> RoadNotes(int roadGlobalIndex)
        {
            if (!TryGetRoadData(roadGlobalIndex, out var roadData))
            {
                return new List<int>();
            }

            if (Notes == null || Notes.Count == 0)
            {
                return new List<int>();
            }

            int globalBegin = roadData.NoteBeginIndex;
            int globalEnd = roadData.NoteEndIndex;

            if (globalBegin < 0 || globalEnd < globalBegin)
            {
                return new List<int>();
            }

            return Notes.Where(n => n >= globalBegin && n <= globalEnd).ToList();
        }

        // ========== Block 相关 CRUD ==========
        public bool InRange(int roadIndex, int blockLocalIndex)
        {
            if (!TryGetRoadData(roadIndex, out var roadData) || blockLocalIndex < 0)
            {
                return false;
            }

            return blockLocalIndex < Mathf.Max(0, roadData.BlockCount);
        }

        public bool GetBlockData(int roadIndex, int blockLocalIndex, out BlockData data)
        {
            if (!InRange(roadIndex, blockLocalIndex) || !TryGetRoadData(roadIndex, out var roadData))
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
                if (!InRange(roadIndex, entry.blockLocalIndex)) return;
                roadData.blockDataList.Add(entry);
                SortBlockData(roadData);
            }
            MarkDirty();
        }

        public void SetBlockData(int roadIndex, int blockLocalIndex, BlockData entry)
        {
            if (!InRange(roadIndex, entry.blockLocalIndex)) return;
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

        public bool HasDisplacementRule(int roadIndex, int blockLocalIndex)
        {
            if (GetBlockData(roadIndex, blockLocalIndex, out var data))
            {
                return data.HasTurn || data.HasDisplacement;
            }
            return false;
        }

        public bool HasTap(int roadIndex, int blockLocalIndex)
        {
            if (!InRange(roadIndex, blockLocalIndex) || !TryGetRoadData(roadIndex, out var roadData))
            {
                return false;
            }

            if (Notes == null)
            {
                return false;
            }

            int noteGlobalIndex = roadData.NoteBeginIndex + blockLocalIndex;
            return Notes.Exists(n => n == noteGlobalIndex);
        }

        public bool IsBeginBlock(int roadIndex, int blockLocalIndex) => InRange(roadIndex, blockLocalIndex) && blockLocalIndex == 0;

        // ========== 查找方法（特殊方块、音符） ==========
        public bool TryGetPrevious_Block_WhichHasDisplacementRule(int searchRoadIndex, int searchBlockLocalIndex, out int resultRoadIndex, out int resultBlockLocalIndex)
        {
            resultRoadIndex = -1;
            resultBlockLocalIndex = -1;

            int roadListIndex = Find_RoadDataListIndex_ByRoadGlobalIndex(searchRoadIndex);
            if (roadListIndex < 0)
            {
                return false;
            }

            for (int i = roadListIndex; i >= 0; i--)
            {
                var road = roadDataList[i];
                int startLocal = i == roadListIndex ? searchBlockLocalIndex - 1 : Mathf.Max(0, road.BlockCount - 1);
                for (int local = startLocal; local >= 0; local--)
                {
                    if (HasDisplacementRule(road.RoadGlobalIndex, local))
                    {
                        resultRoadIndex = road.RoadGlobalIndex;
                        resultBlockLocalIndex = local;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetNext_Block_WhichHasDisplacementRule(int searchRoadIndex, int searchBlockLocalIndex, out int resultRoadIndex, out int resultBlockLocalIndex)
        {
            resultRoadIndex = -1;
            resultBlockLocalIndex = -1;

            int roadListIndex = Find_RoadDataListIndex_ByRoadGlobalIndex(searchRoadIndex);
            if (roadListIndex < 0)
            {
                return false;
            }

            for (int i = roadListIndex; i < roadDataList.Count; i++)
            {
                var road = roadDataList[i];
                int startLocal = i == roadListIndex ? Mathf.Max(0, searchBlockLocalIndex) : 0;
                int endLocal = Mathf.Max(0, road.BlockCount);
                for (int local = startLocal; local < endLocal; local++)
                {
                    if (HasDisplacementRule(road.RoadGlobalIndex, local))
                    {
                        resultRoadIndex = road.RoadGlobalIndex;
                        resultBlockLocalIndex = local;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetPrevious_Block_WhichNeedTap(int searchRoadIndex, int searchBlockLocalIndex, out int resultRoadIndex, out int resultBlockLocalIndex)
        {
            resultRoadIndex = -1;
            resultBlockLocalIndex = -1;

            int roadListIndex = Find_RoadDataListIndex_ByRoadGlobalIndex(searchRoadIndex);
            if (roadListIndex < 0)
            {
                return false;
            }

            for (int i = roadListIndex; i >= 0; i--)
            {
                var road = roadDataList[i];
                int startLocal = i == roadListIndex ? searchBlockLocalIndex - 1 : Mathf.Max(0, road.BlockCount - 1);
                for (int local = startLocal; local >= 0; local--)
                {
                    if (HasTap(road.RoadGlobalIndex, local))
                    {
                        resultRoadIndex = road.RoadGlobalIndex;
                        resultBlockLocalIndex = local;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetNext_Block_WhichNeedTap(int searchRoadIndex, int searchBlockLocalIndex, out int resultRoadIndex, out int resultBlockLocalIndex)
        {
            resultRoadIndex = -1;
            resultBlockLocalIndex = -1;

            int roadListIndex = Find_RoadDataListIndex_ByRoadGlobalIndex(searchRoadIndex);
            if (roadListIndex < 0)
            {
                return false;
            }

            for (int i = roadListIndex; i < roadDataList.Count; i++)
            {
                var road = roadDataList[i];
                int startLocal = i == roadListIndex ? Mathf.Max(0, searchBlockLocalIndex) : 0;
                int endLocal = Mathf.Max(0, road.BlockCount);
                for (int local = startLocal; local < endLocal; local++)
                {
                    if (HasTap(road.RoadGlobalIndex, local))
                    {
                        resultRoadIndex = road.RoadGlobalIndex;
                        resultBlockLocalIndex = local;
                        return true;
                    }
                }
            }
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
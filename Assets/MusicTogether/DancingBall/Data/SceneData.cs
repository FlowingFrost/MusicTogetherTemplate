using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall.Data
{ 
    [Serializable]
    public class RoadData
    {
        [Obsolete("应直接按照列表顺序访问，Index不再需要(Road按照时间排序而不是序号排序)")][HideInInspector] public int roadIndex_Global;
        public string roadName;
        public int targetSegmentIndex;
        public int noteBeginIndex;
        public int noteEndIndex;
        public int BlockCount => noteEndIndex - noteBeginIndex + 1;
        [Obsolete("不建议使用全局序号访问Block，这可能会造成问题")][HideInInspector] public int blockIndex_Global_Begin;
        [ListDrawerSettings(DefaultExpandedState = false)]
        public List<IBlockDisplacementData> blockDisplacementDataList = new List<IBlockDisplacementData>();

        public RoadData(int roadIndexGlobal, int targetSegmentIndex = 0, int noteBeginIndex = -1, int noteEndIndex = -1)
        {
            //this.roadIndex_Global = roadIndexGlobal;
            this.targetSegmentIndex = targetSegmentIndex;
            this.noteBeginIndex = noteBeginIndex;
            this.noteEndIndex = noteEndIndex;
        }
        
       
        private void Refresh_BlockDisplacementDataList() => blockDisplacementDataList.Sort((a,b)=> a.BlockIndex_Local.CompareTo(b.BlockIndex_Local));
        
        /// <summary>
        /// 获取目标Block的Displacement数据在列表中的位置。
        /// </summary>
        public int Get_BlockDisplacementDataListIndex_ByBlockIndexLocal(int blockIndexLocal) => blockDisplacementDataList.FindIndex(data => data.BlockIndex_Local == blockIndexLocal);
        
        public bool Get_BlockData(int blockLocalIndex, out IBlockDisplacementData data)
        {
            data = null;
            int indexInList = Get_BlockDisplacementDataListIndex_ByBlockIndexLocal(blockLocalIndex);
            if (indexInList >= 0)
            {
                data = blockDisplacementDataList[indexInList];
                return true;
            }
            return false;
        }
        
        public void Set_BlockData(IBlockDisplacementData data)
        {
            int indexInList = Get_BlockDisplacementDataListIndex_ByBlockIndexLocal(data.BlockIndex_Local);
            if (indexInList >= 0) blockDisplacementDataList[indexInList] = data;
            else blockDisplacementDataList.Insert(~indexInList,data);
        }

        public void Remove_BlockData(int blockIndexLocal)
        {
            int indexInList = Get_BlockDisplacementDataListIndex_ByBlockIndexLocal(blockIndexLocal);
            if (indexInList >= 0) blockDisplacementDataList.RemoveAt(indexInList);
        }
    }

    [CreateAssetMenu(menuName = "MusicTogether/DancingBall_SceneData", fileName = "NewSceneData")]
    public class SceneData : SerializedScriptableObject
    {
        public InputNoteData inputNoteData;
        public List<Segemnt> SegmentList => inputNoteData.noteLists;

        //[ListDrawerSettings(CustomAddFunction = nameof(AddRoadData))]
        [NonSerialized][OdinSerialize]public List<RoadData> roadDataList = new List<RoadData>();

        // Note数据操作=================================================================================
        private bool GetSegmentIndex(int targetSegmentIndex, out int segmentListIndex)
        {
            segmentListIndex = -1;
            if (targetSegmentIndex < 0) return false;
            if (targetSegmentIndex < SegmentList.Count)
            {
                if (SegmentList[targetSegmentIndex].Index == targetSegmentIndex)
                {
                    segmentListIndex = targetSegmentIndex;
                    return true;
                }
            }

            segmentListIndex = SegmentList.FindIndex(segment => segment.Index == targetSegmentIndex);
            if (segmentListIndex == -1)
            {
                Debug.LogError($"Segment with index {targetSegmentIndex} not found in SegmentList.");
                return false;
            }

            return true;
        }

        public bool GetSegment(int targetSegmentIndex, out Segemnt segment)
        {
            segment = new Segemnt();
            if (!GetSegmentIndex(targetSegmentIndex, out int segmentListIndex)) return false;
            segment = SegmentList[segmentListIndex];
            return true;
        }

        //移除HasTap，road自己根据index进行遍历操作。
        [Obsolete]
        public bool GetNoteTime(int targetSegmentIndex, int noteIndex, out double noteTime)
        {
            noteTime = 0f;
            if (!GetSegment(targetSegmentIndex, out Segemnt segment)) return false;
            if (noteIndex < 0 || noteIndex >= segment.notes.Count)
            {
                Debug.LogError($"Note index {noteIndex} is out of range for segment {targetSegmentIndex}.");
                return false;
            }

            noteTime = segment.GetNoteTimeAt(noteIndex);
            return true;
        }

        public bool Exists_NoteIndex(int targetSegmentIndex, int noteIndex)
        {
            if (!GetSegment(targetSegmentIndex, out Segemnt segment)) return false;
            return segment.notes.Contains(noteIndex);
        }

        //Road数据操作==================================================================================
        [Obsolete] private bool Is_RoadIndexInRange(int roadIndexGlobal) => roadIndexGlobal >= 0 && roadIndexGlobal < roadDataList.Count;
        
        public bool Exists_RoadData_ByRoadName(string roadName) => roadDataList.Any(rd => rd.roadName == roadName);
        
        /// <summary>
        /// 验证RoadData的roadIndex_Global是否有效且与列表中的位置一致。如果不一致，尝试排序列表以修正索引。
        /// </summary>
        [Obsolete] private bool ValidateRoadIndexGlobal(int roadIndexGlobal)
        {
            if (roadIndexGlobal < 0 || roadIndexGlobal >= roadDataList.Count)
            {
                Debug.LogError($"Road index {roadIndexGlobal} is out of range.");
                return false;
            }

            if (roadDataList[roadIndexGlobal].roadIndex_Global != roadIndexGlobal) Refresh_RoadDataList();
            return true;
        }
        
        /// <summary>
        /// 获取RoadData在列表中的索引。现在的列表强制要求序号统一，不再需要这个函数
        /// </summary>
        [Obsolete] private int Find_RoadDataListIndex_ByRoadIndexGlobal(int roadIndexGlobal)
        {
            if (roadIndexGlobal < 0) return -1;
            if (roadDataList.Count > roadIndexGlobal)
            {
                if (roadDataList[roadIndexGlobal].roadIndex_Global == roadIndexGlobal)
                {
                    return roadIndexGlobal;
                }
            }
            return roadDataList.FindIndex(roadData => roadData.roadIndex_Global == roadIndexGlobal);
        }
        
        /// <summary>
        /// 根据Road起始时间排序，按照开始时间排序并生成blockIndex的Global起始。
        /// </summary>
        private void Refresh_RoadDataList()
        {
            var sorted = roadDataList
                .Select(rd => new
                {
                    Data = rd,
                    Time = GetSegment(rd.targetSegmentIndex, out var segment) 
                        ? segment.GetNoteTimeAt(rd.noteBeginIndex) 
                        : 0
                })
                .OrderBy(x => x.Time)
                /*.Select((x, i) =>
                {
                    x.Data.roadIndex_Global = i;
                    return x.Data;
                })*/
                .Select((x)=> x.Data)
                .ToList();

            roadDataList.Clear();
            roadDataList.AddRange(sorted);
            //为了适配旧API设计的功能
#pragma warning disable CS0618 // 类型或成员已过时
            roadDataList[0].blockIndex_Global_Begin = 0;
            for (int i = 1; i < roadDataList.Count; i++)
            {
                roadDataList[i].blockIndex_Global_Begin = roadDataList[i - 1].blockIndex_Global_Begin + roadDataList[i - 1].BlockCount;
            }
#pragma warning restore CS0618 // 类型或成员已过时
        }
        
        [Obsolete] public bool Get_RoadData_ByRoadIndexGlobal(int roadIndexGlobal, out RoadData roadData)
        {
            roadData = null;
            if (!ValidateRoadIndexGlobal(roadIndexGlobal)) return false;
            roadData = roadDataList[roadIndexGlobal];
            return roadData != null;
        }
        
        public int Get_RoadDataIndex_ByRoadName(string roadName) => roadDataList.FindIndex(rd => rd.roadName == roadName);
        
        public bool Get_RoadData_ByRoadName(string roadName, out RoadData roadData)
        {
            roadData = null;
            int index = Get_RoadDataIndex_ByRoadName(roadName);
            if (index >= 0)
            {
                roadData = roadDataList[index];
                return true;
            }
            return false;
        }

        /// <summary>
        /// 替换某一项或在末尾插入新的项(同时修改index)
        /// </summary>
        /// <param name="roadData"></param>
        public void Set_RoadData(RoadData roadData)
        {
            int targetRoadIndexGlobal = Get_RoadDataIndex_ByRoadName(roadData.roadName);
            if (targetRoadIndexGlobal > 0) roadDataList[targetRoadIndexGlobal] = roadData;
            else
            {
                roadDataList.Add(roadData);
                Refresh_RoadDataList();
            }
        }
        
        //==============Road操作中与Block相关的部分
        //过时的设计：不建议直接由SceneData访问Block数据
        //Block数据操作==================================================================================
        //过时的设计：不建议直接由SceneData访问Block数据
    }
}
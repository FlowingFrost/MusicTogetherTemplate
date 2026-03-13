using System.Collections.Generic;
using MusicTogether.DancingBall.Archived_EditorTool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall.Archived_SceneMap
{
    public class Road : MonoBehaviour
    {
        public Map map;
        public EditorTool EditorTool => map.editorTool;
        public EditorActionDispatcher Dispatcher => map.dispatcher;
        [FormerlySerializedAs("roadIndex")] public int roadGlobalIndex = -1;
        public List<Block> blocks;

#if UNITY_EDITOR
        [Title("Road Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel]
        [ReadOnly]
        //[OnValueChanged(nameof(OnRoadDataChanged))]
        private RoadData PreviewRoadData
        {
            get
            {
                if (roadGlobalIndex < 0) return null;
                if (map == null || map.mapData == null) return null;
                map.mapData.GetRoadData(roadGlobalIndex, out var data);
                return data;
            }
            /*set
            {
                if (value == null || map == null || map.mapData == null) return;
                map.mapData.SetRoadData(value);
            }*/
        }

        //暂时隐藏，方法过于危险且不够正式。
        /*private void OnRoadDataChanged(RoadData data)
        {
            if (data == null || map == null || map.mapData == null) return;
            
            _lastRoadData = new RoadData(data.index)
            {
                beginBlockIndex = data.beginBlockIndex
            };

            map.mapData.SetRoadData(data);
            
            if (_lastRoadData != null)
            {
                EditorTool.OnRoadBlockBeginIndexChanged(this, _lastRoadData.beginBlockIndex, data.beginBlockIndex);
            }
            
            UnityEditor.EditorUtility.SetDirty(map.mapData);
        }*/
#endif
        public bool HasValidReference => map != null;
        public bool HasPreviewRoadData => PreviewRoadData != null;
        [ShowIf("@HasPreviewRoadData == false")][Button("创建Road Data")]
        public void CreateRoadData()
        {
            if (map == null || map.mapData == null) return;
            if (roadGlobalIndex < 0) return;
            map.mapData.GetRoadData(roadGlobalIndex, out _);
        }
        
        [ShowIf("HasPreviewRoadData")][Button("修改Block起始Index")]
        public void ModifyBlockBeginIndex(int newBeginIndex)
        {
            int totalBlockCount = map.mapData.totalBlockCount;
            if (newBeginIndex >= totalBlockCount)
            {
                Debug.LogError($"[Road_{roadGlobalIndex}] beginBlockIndex ({newBeginIndex}) 不能大于或等于 MapData 的 totalBlockCount ({totalBlockCount})，操作已取消。");
                return;
            }
            map.mapData.GetRoadData(roadGlobalIndex, out var roadData);
            roadData.beginBlockIndex = newBeginIndex;
            map.mapData.SetRoadData(roadData);
            Dispatcher.Dispatch(nameof(EditorTool.OnRoadBlockBeginIndexChanged),
                EditorActionContext.ForRoad(map, roadGlobalIndex));
        }

        [ShowIf("HasPreviewRoadData")][Button("修改Road Index")]
        public void SetRoadIndex(int newRoadIndex)
        {
            roadGlobalIndex = newRoadIndex;
            RefreshBlocks();
        }
        
        [ShowIf("HasValidReference")] [Button("重建Blocks")]
        public void RebuildBlocks()
        {
            if (map == null || map.mapData == null) return;
            if (roadGlobalIndex < 0) return;
            if (PreviewRoadData == null) return;

            // 销毁所有子物体（含不在 blocks 列表中的游离物体）
            var children = new List<Transform>();
            foreach (Transform child in transform) children.Add(child);
            foreach (var child in children) DestroyImmediate(child.gameObject);

            blocks.Clear();
            RefreshBlocks();
        }
        
        [ShowIf("HasPreviewRoadData")][Button("刷新Blocks")]
        public void RefreshBlocks()
        {
            Dispatcher.Dispatch(nameof(EditorTool.RefreshRoadBlocks),
                EditorActionContext.ForRoad(map, roadGlobalIndex));
        }

        [ShowIf("HasValidReference")]
        [Button("更新Block方位")]
        public void UpdateBlockTransforms()
        {
            Dispatcher.Dispatch(nameof(EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(map, roadGlobalIndex, PreviewRoadData.beginBlockIndex));
        }
        /*public ValueTuple<Vector3, Quaternion> PreviousBlockTransform(int blockIndex)
        {
            if (blocks == null || blocks.Count == 0) return (Vector3.zero, Quaternion.identity);
            if (blockIndex >= PreviewRoadData.beginBlockIndex && blockIndex <= map.mapData.GetRoadEndBlockIndex(roadIndex))
            {
                int localIndex = blocks.FindIndex(b => b.globalBlockIndex == blockIndex);
                if (localIndex > 0 && localIndex < blocks.Count)
                {
                    return (blocks[localIndex - 1].transform.localPosition, blocks[localIndex - 1].transform.localRotation);
                }
            }
            return (Vector3.zero, Quaternion.identity);
        }*/
        
        [ShowIf("HasValidReference")]
        [Button("更新BlockInfo显示")]
        public void RefreshBlockInfoDisplay()
        {
            Dispatcher.Dispatch(nameof(EditorTool.RefreshBlockInfoDisplay),
                EditorActionContext.ForRoad(map, roadGlobalIndex));
        }
    }
}
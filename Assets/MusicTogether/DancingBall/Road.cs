using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Road : MonoBehaviour
    {
        public Map map;
        public EditorTool EditorTool => map.editorTool;
        public int roadIndex = -1;
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
                if (roadIndex < 0) return null;
                if (map == null || map.mapData == null) return null;
                map.mapData.GetRoadData(roadIndex, out var data);
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
            if (roadIndex < 0) return;
            map.mapData.GetRoadData(roadIndex, out _);
        }
        
        [ShowIf("HasPreviewRoadData")][Button("修改Block起始Index")]
        public void ModifyBlockBeginIndex(int newBeginIndex)
        {
            var formerIndex = PreviewRoadData.beginBlockIndex;
            EditorTool.OnRoadBlockBeginIndexChanged(map, roadIndex, formerIndex, newBeginIndex);
        }

        [ShowIf("HasPreviewRoadData")][Button("修改Road Index")]
        public void SetRoadIndex(int newRoadIndex)
        {
            roadIndex = newRoadIndex;
            RefreshBlocks();
        }
        
        [ShowIf("HasValidReference")] [Button("重建Blocks")]
        public void RebuildBlocks()
        {
            if (map == null || map.mapData == null) return;
            if (roadIndex < 0) return;
            if (PreviewRoadData == null) return;
            
            var blocksCopy = new List<Block>(blocks);
            EditorTool.RemoveBlocks(this, blocksCopy);
            RefreshBlocks();
        }
        
        [ShowIf("HasPreviewRoadData")][Button("刷新Blocks")]
        public void RefreshBlocks()
        {
            EditorTool.RefreshRoadBlocks(map, roadIndex);
        }

        [ShowIf("HasValidReference")]
        [Button("更新Block方位")]
        public void UpdateBlockTransforms()
        {
            EditorTool.OnBlockDisplacementRuleChanged(map, PreviewRoadData.beginBlockIndex);
        }
        public ValueTuple<Vector3, Quaternion> PreviousBlockTransform(int blockIndex)
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
        }
        
        [ShowIf("HasValidReference")]
        [Button("更新BlockInfo显示")]
        public void RefreshBlockInfoDisplay()
        {
            EditorTool.RefreshBlockInfoDisplay(map, roadIndex);
        }
    }
}
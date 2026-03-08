using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Road : MonoBehaviour
    {
        public Map map;
        public EditorTool EditorTool => map.editorTool;
        public int roadIndex;
        public List<Block> blocks;

#if UNITY_EDITOR
        private RoadData _lastRoadData;

        [Title("Road Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel]
        [OnValueChanged(nameof(OnRoadDataChanged))]
        [PropertyOrder(10)]
        private RoadData PreviewRoadData
        {
            get
            {
                if (map == null || map.mapData == null) return null;
                map.mapData.GetRoadData(roadIndex, out var data);
                return data;
            }
            set
            {
                if (value == null || map == null || map.mapData == null) return;
                map.mapData.SetRoadData(value);
            }
        }

        private void OnRoadDataChanged(RoadData data)
        {
            if (data == null || map == null || map.mapData == null) return;

            if (_lastRoadData != null)
            {
                if (_lastRoadData.beginBlockIndex != data.beginBlockIndex)
                    Debug.Log($"Road[{roadIndex}] beginBlockIndex: {_lastRoadData.beginBlockIndex} → {data.beginBlockIndex}");
            }

            _lastRoadData = new RoadData(data.index)
            {
                beginBlockIndex = data.beginBlockIndex
            };

            map.mapData.SetRoadData(data);
            UnityEditor.EditorUtility.SetDirty(map.mapData);
        }
#endif
        
        [Button("Refresh Blocks")]
        public void RefreshBlocks()
        {
            EditorTool.RefreshRoadBlocks(this);
        }
    }
}
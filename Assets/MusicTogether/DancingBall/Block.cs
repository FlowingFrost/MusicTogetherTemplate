using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Block : MonoBehaviour
    {
        public Map Map => road.map;
        public Road road;
        public MapData MapData => Map.mapData;
        public int blockIndex;

#if UNITY_EDITOR
        private BlockData _lastBlockData;

        [Title("Block Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel]
        [OnValueChanged(nameof(OnBlockDataChanged))]
        [PropertyOrder(10)]
        private BlockData PreviewBlockData
        {
            get
            {
                if (road == null) return null;
                if (MapData == null) return null;

                MapData.GetBlockData(blockIndex, out var data);
                return data;
            }
            set
            {
                if (value == null || MapData == null) return;
                MapData.SetBlockData(value);
            }
        }

        private void OnBlockDataChanged(BlockData data)
        {
            if (data == null || MapData == null) return;

            if (_lastBlockData != null)
            {
                if (_lastBlockData.turnType != data.turnType)
                    Debug.Log($"Block[{blockIndex}] turnType: {_lastBlockData.turnType} → {data.turnType}");
                if (_lastBlockData.displacementType != data.displacementType)
                    Debug.Log($"Block[{blockIndex}] displacementType: {_lastBlockData.displacementType} → {data.displacementType}");
            }

            _lastBlockData = new BlockData(data.index)
            {
                turnType = data.turnType,
                displacementType = data.displacementType
            };

            MapData.SetBlockData(data);
            UnityEditor.EditorUtility.SetDirty(MapData);
        }
#endif
    }
}
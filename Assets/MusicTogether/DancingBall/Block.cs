using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall
{
    public class Block : MonoBehaviour
    {
        public Map Map => HasValidReference ? road.map : null;
        public MapData MapData => HasValidReference ? Map.mapData : null;
        public EditorTool EditorTool => HasValidReference ?  Map.editorTool : null;
        public TileHolder tileHolder;
        public BlockInformationDisplay blockInformationDisplay;
        public Road road;
        
        public int globalBlockIndex = -1;

#if UNITY_EDITOR
        //private BlockData _lastBlockData;
        [Title("Block Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel] [Sirenix.OdinInspector.ReadOnly]

        //[OnValueChanged(nameof(OnBlockDataChanged))]
        private BlockData PreviewBlockData
        {
            get
            {
                if (globalBlockIndex < 0) return null;
                if (road == null) return null;
                if (MapData == null) return null;

                MapData.GetBlockData(globalBlockIndex, out var data);
                return data;
            }
            /*set
            {
                if (value == null || MapData == null) return;
                MapData.SetBlockData(value);
            }*/
        }

        /*private void OnBlockDataChanged(BlockData data)
        {
            if (data == null || MapData == null) return;

            if (_lastBlockData != null)
            {
                if (_lastBlockData.turnType != data.turnType)
                    Debug.Log($"Block[{blockGlobalIndex}] turnType: {_lastBlockData.turnType} → {data.turnType}");
                if (_lastBlockData.displacementType != data.displacementType)
                    Debug.Log($"Block[{blockGlobalIndex}] displacementType: {_lastBlockData.displacementType} → {data.displacementType}");
            }

            _lastBlockData = new BlockData(data.index)
            {
                turnType = data.turnType,
                displacementType = data.displacementType
            };

            MapData.SetBlockData(data);
            UnityEditor.EditorUtility.SetDirty(MapData);
        }*/
#endif
        public bool HasValidReference => road != null;
        public bool HasValidIndex => MapData.IsValidBlockIndex(globalBlockIndex);

        [ShowIf("@!HasValidReference || !HasValidIndex")]
        [InfoBox("引用信息无效，无法获取数据。")]
        [Button("删除方块")]
        private void DeleteInvalidBlock()
        {
            if (Application.isPlaying) return;
            if (HasValidReference) return;
            DestroyImmediate(gameObject);
        }
        
        [Title("", horizontalLine: true)]
        
        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("TurnType")][EnumToggleButtons]
        public TurnType newTurnType;

        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("TurnType")][Button("修改TurnType")]
        public void ModifyTurnType()
        {
            MapData.GetBlockData(globalBlockIndex, out var data);
            data.turnType = newTurnType;
            MapData.SetBlockData(data);
            EditorTool.OnBlockDisplacementRuleChanged(Map, globalBlockIndex);
        }

        [Title("", horizontalLine: true)]
        
        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("DisplacementType")][EnumToggleButtons]
        public DisplacementType newDisplacementType;

        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("DisplacementType")][Button("修改DisplacementType")]
        public void ModifyDisplacementType()
        {
            MapData.GetBlockData(globalBlockIndex, out var data);
            data.displacementType = newDisplacementType;
            MapData.SetBlockData(data);
            EditorTool.OnBlockDisplacementRuleChanged(Map, globalBlockIndex);
        }
    }
}
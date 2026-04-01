using System.Collections.Generic;
using MusicTogether.Archived_DancingBall.DancingBall;
using MusicTogether.Archived_DancingBall.EditorTool;
using MusicTogether.Archived_DancingBall.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Scene
{
    public class Block : MonoBehaviour
    {
        public Map Map => road != null ? road.map : null;
        public SceneData SceneData => Map != null ? Map.SceneData : null;
        public EditorTool.EditorTool EditorTool => Map != null ? Map.EditorTool : null;
        public EditorActionDispatcher Dispatcher => Map != null ? Map.Dispatcher : null;

        public TileHolder tileHolder;
        public BlockInformationDisplay blockInformationDisplay;
        public Road road;

        public int blockLocalIndex = -1;

#if UNITY_EDITOR
        [Title("Block Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel] [ReadOnly]
        private BlockData PreviewBlockData
        {
            get
            {
                if (blockLocalIndex < 0) return null;
                if (road == null || SceneData == null) return null;
                SceneData.GetBlockData(road.RoadIndex, blockLocalIndex, out var data);
                return data;
            }
        }
#endif

        public bool HasValidReference => road != null && Map != null;
        public bool HasValidIndex => SceneData != null && SceneData.Is_BlockIndex_InRange(road.RoadIndex, blockLocalIndex);

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
            SceneData.GetBlockData(road.RoadIndex, blockLocalIndex, out var data);
            data.turnType = newTurnType;
            SceneData.SetBlockData(road.RoadIndex, data);
            
            // 使用 Dispatcher 触发更新
            Dispatcher?.Dispatch(nameof(Archived_DancingBall.EditorTool.EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(Map, road.RoadIndex, blockLocalIndex));
        }

        [Title("", horizontalLine: true)]

        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("DisplacementType")][EnumToggleButtons]
        public DisplacementType newDisplacementType;

        [ShowIf("@HasValidIndex && HasValidReference")]
        [VerticalGroup("DisplacementType")][Button("修改DisplacementType")]
        public void ModifyDisplacementType()
        {
            SceneData.GetBlockData(road.RoadIndex, blockLocalIndex, out var data);
            data.displacementType = newDisplacementType;
            SceneData.SetBlockData(road.RoadIndex, data);
            
            // 使用 Dispatcher 触发更新
            Dispatcher?.Dispatch(nameof(Archived_DancingBall.EditorTool.EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(Map, road.RoadIndex, blockLocalIndex));
        }
        
        public List<MovementData> GetBlockMovementData()
        {
            SceneData.GetNoteTime(road.RoadIndex, blockLocalIndex, out var time);
            var tiles = tileHolder.GetTileTransforms();
            switch (tiles.Count)
            {
                case 0:
                    return new List<MovementData>();
                case 1:
                    return new List<MovementData>
                    {new MovementData(time, tiles[0].position + tiles[0].up * (tileHolder.TileThickness/2 + Map.ballRadius), tiles[0].rotation)
                    };
                case 2:
                    SceneData.GetNoteLenth(road.RoadIndex, out var blockTime);
                    return new List<MovementData>
                    {
                        new MovementData(time + blockTime/7, tiles[0].position + tiles[0].up * (tileHolder.TileThickness/2 + Map.ballRadius), tiles[0].rotation),
                        new MovementData(time - blockTime/7, tiles[1].position + tiles[1].up * (tileHolder.TileThickness/2 + Map.ballRadius), tiles[1].rotation)
                    };
                default:
                    Debug.LogWarning("Unexpected number of tiles in TileHolder.");
                    return new List<MovementData>();
            }
        }
    }
}

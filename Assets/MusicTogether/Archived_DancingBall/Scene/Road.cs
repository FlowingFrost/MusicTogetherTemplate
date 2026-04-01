using System.Collections.Generic;
using System.Linq;
using MusicTogether.Archived_DancingBall.DancingBall;
using MusicTogether.Archived_DancingBall.EditorTool;
using MusicTogether.Archived_DancingBall.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Scene
{
    public class Road : MonoBehaviour
    {
        public Map map;
        [ShowInInspector] public int RoadIndex = -1;

        [ListDrawerSettings(DefaultExpandedState = false)]
        public List<Block> blocks = new List<Block>();

#if UNITY_EDITOR
        [Title("Road Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel]
        [ReadOnly]
        private RoadData PreviewRoadData
        {
            get
            {
                if (RoadIndex < 0) return null;
                if (map == null || map.SceneData == null) return null;
                map.SceneData.GetRoadData(RoadIndex, out var data);
                return data;
            }
        }
#endif

        public bool HasValidReference => map != null;
        public bool HasPreviewRoadData => PreviewRoadData != null;

        [ShowIf("HasPreviewRoadData"), Button("刷新Blocks")]
        public void RefreshBlocks()
        {
            map?.Dispatcher?.Dispatch(nameof(EditorTool.EditorTool.RefreshRoadBlocks),
                EditorActionContext.ForRoad(map, RoadIndex));
        }

        [ShowIf("HasValidReference")]
        [Button("更新Block方位")]
        public void UpdateBlockTransforms()
        {
            map?.Dispatcher?.Dispatch(
                nameof(EditorTool.EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(map, RoadIndex, 0));
        }

        [ShowIf("HasValidReference")]
        [Button("更新BlockInfo显示")]
        public void RefreshBlockInfoDisplay()
        {
            map?.Dispatcher?.Dispatch(nameof(EditorTool.EditorTool.RefreshBlockInfoDisplay),
                EditorActionContext.ForRoad(map, RoadIndex));
        }

        [ShowIf("HasValidReference")]
        [Button("修改Note起始Index")]
        public void ModifyBlockBeginIndex(int newBeginIndex)
        {
            if (map == null || map.SceneData == null) return;
            map.SceneData.GetRoadData(RoadIndex, out var roadData);
            roadData.NoteBeginIndex = newBeginIndex;
            map.SceneData.SetRoadData(roadData);
            map.Dispatcher?.Dispatch(nameof(EditorTool.EditorTool.OnRoadBlockCountChanged),
                EditorActionContext.ForRoad(map, RoadIndex));
        }

        [ShowIf("HasValidReference")]
        [Button("修改Note结束Index")]
        public void ModifyBlockEndIndex(int newEndIndex)
        {
            if (map == null || map.SceneData == null) return;
            map.SceneData.GetRoadData(RoadIndex, out var roadData);
            roadData.NoteEndIndex = newEndIndex;
            map.SceneData.SetRoadData(roadData);
            map.Dispatcher?.Dispatch(nameof(EditorTool.EditorTool.OnRoadBlockCountChanged),
                EditorActionContext.ForRoad(map, RoadIndex));
        }

        [ShowIf("HasValidReference")]
        [Button("修改目标片段")]
        public void ModifySegmentIndex(int newSegmentIndex)
        {
            if (map == null || map.SceneData == null) return;
            map.SceneData.GetRoadData(RoadIndex, out var roadData);
            roadData.TargetSegmentIndex = newSegmentIndex;
            map.SceneData.SetRoadData(roadData);
            map.Dispatcher?.Dispatch(nameof(EditorTool.EditorTool.OnRoadBlockCountChanged),
                EditorActionContext.ForRoad(map, RoadIndex));
        }
        public List<MovementData> GetBlockMovementDatas(int blockBeginIndex, int blockEndIndex)
        {
            List<MovementData> movementDatas = new List<MovementData>();
            if (map == null || map.SceneData == null) return movementDatas;

            var _blocks = this.blocks.Where(b =>
                b.blockLocalIndex >= blockBeginIndex && b.blockLocalIndex <= blockEndIndex);
            foreach (var block in _blocks)
            {
                movementDatas.AddRange(block.GetBlockMovementData());
            }
            movementDatas.Sort((a,b)=> a.Time.CompareTo(b.Time));
            return movementDatas;
        }
    }
}

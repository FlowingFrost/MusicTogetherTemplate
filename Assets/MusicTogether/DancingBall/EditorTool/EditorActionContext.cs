using MusicTogether.DancingBall.Scene;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// 编辑操作的上下文：双键（roadIndex + blockLocalIndex）。
    /// </summary>
    public class EditorActionContext
    {
        public Map TargetMap;
        public int RoadIndex = -1;
        public int BlockLocalIndex = -1;

        public static EditorActionContext ForMap(Map map)
            => new EditorActionContext { TargetMap = map };

        public static EditorActionContext ForRoad(Map map, int roadIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex };

        public static EditorActionContext ForRoadAndBlock(Map map, int roadIndex, int blockLocalIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex, BlockLocalIndex = blockLocalIndex };
    }
}

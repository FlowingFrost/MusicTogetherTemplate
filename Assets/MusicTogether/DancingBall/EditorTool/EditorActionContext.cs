using MusicTogether.DancingBall.Scene;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// 编辑操作的上下文：双键（roadIndex + blockLocalIndex）。
    /// </summary>
    public class EditorActionContext
    {
        public IMap TargetMap;
        public int RoadIndex = -1;
        public int BlockLocalIndex = -1;

        public static EditorActionContext ForMap(IMap map)
            => new EditorActionContext { TargetMap = map };

        public static EditorActionContext ForRoad(IMap map, int roadIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex };

        public static EditorActionContext ForRoadAndBlock(IMap map, int roadIndex, int blockLocalIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex, BlockLocalIndex = blockLocalIndex };
    }

    public class EditorActionContextSerialized
    {
        public IMap TargetMap;
        public IRoad TargetRoad;
        public IBlock TargetBlock;

        public static EditorActionContextSerialized ForMap(IMap map)
            => new EditorActionContextSerialized { TargetMap = map };

        public static EditorActionContextSerialized ForRoad(IMap map, IRoad road)
            => new EditorActionContextSerialized { TargetMap = map, TargetRoad = road };

        public static EditorActionContextSerialized ForRoadAndBlock(IMap map, IRoad road, IBlock block)
            => new EditorActionContextSerialized { TargetMap = map, TargetRoad = road, TargetBlock = block };
    }
}

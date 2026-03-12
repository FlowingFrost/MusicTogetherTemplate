using MusicTogether.DancingBall.SceneMap;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// 编辑操作的统一上下文，所有 EditorTool 方法均通过此对象传参。
    /// 字段默认值 -1 表示"本次操作不涉及该字段"。
    /// </summary>
    public class EditorActionContext
    {
        /// <summary>操作目标地图</summary>
        public Map TargetMap;

        /// <summary>目标 Road 索引，-1 表示不涉及</summary>
        public int RoadIndex = -1;

        /// <summary>目标 Block 全局索引，-1 表示不涉及</summary>
        public int BlockIndex = -1;

        // ── 快捷构造方法 ───────────────────────────────────────────────────

        public static EditorActionContext ForMap(Map map)
            => new EditorActionContext { TargetMap = map };

        public static EditorActionContext ForRoad(Map map, int roadIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex };

        /// <summary>
        /// 通过 blockIndex 构造上下文时必须同时提供 roadIndex。
        /// roadIndex 应与该 block 实例所归属的 Road 一致（即该 Block 组件上 road 字段所指向的 Road 的 roadIndex）。
        /// </summary>
        public static EditorActionContext ForRoadAndBlock(Map map, int roadIndex, int blockIndex)
            => new EditorActionContext { TargetMap = map, RoadIndex = roadIndex, BlockIndex = blockIndex };
    }
}

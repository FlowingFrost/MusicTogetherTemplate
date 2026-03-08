using UnityEngine;

namespace MusicTogether.DancingBallOld.Interfaces
{
    /// <summary>
    /// 代表地图上一个方块的运行时对象（场景中的 GameObject）。
    /// 放置数据（转弯、位移等）统一通过所属 Road 的 BlockPlacementData 按 IndexInRoad 查询，
    /// 不再序列化在 Block 自身上。
    /// </summary>
    public interface IBlock
    {
        /// <summary>该 Block 在所属 Road 中的索引（0-based）。</summary>
        int IndexInRoad { get; set; }

        /// <summary>Block 的 Transform，用于读写位置/旋转（接口无法直接访问 MonoBehaviour.transform）。</summary>
        Transform Transform { get; }
    }
}
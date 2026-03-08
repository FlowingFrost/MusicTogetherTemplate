using MusicTogether.DancingBallOld.Interfaces;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    /// <summary>
    /// 经典模式 Block 的运行时场景对象。
    /// 自身不再保存任何放置数据；所有数据统一由所属 Road 的
    /// <see cref="BlockPlacementData"/> 按 <see cref="IndexInRoad"/> 提供。
    /// </summary>
    public class ClassicBlock : MonoBehaviour, IBlock
    {
        [SerializeField] private int indexInRoad;

        public int IndexInRoad
        {
            get => indexInRoad;
            set => indexInRoad = value;
        }

        public Transform Transform => transform;
    }
}

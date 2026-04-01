using System.Collections.Generic;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Scene
{
    public class ClassicBlock : SerializedMonoBehaviour, IBlock
    {
        //外部引用
        public IRoad Road { get; private set; }

        //本体绑定信息
        public Transform Transform => transform;
        public ITileHolder TileHolder { get; private set; }
        public IBlockDebug BlockDisplay { get; private set; }
        //参数
        public int BlockLocalIndex { get; set; }
        //表达式
        private bool IsDataValid => Road != null && Road.Map != null && Road.RoadData != null && BlockLocalIndex >= 0;
    
#if UNITY_EDITOR
        [Title("Block Data (Preview)")]
        [ShowInInspector, InlineProperty, HideLabel] [ReadOnly]
        private IBlockDisplacementData PreviewBlockDisplacementData
        {
            get
            {
                if (!IsDataValid) return null;
                if (!Road.RoadData.Get_BlockData(BlockLocalIndex, out IBlockDisplacementData blockDisplacementData)) return null;
                return blockDisplacementData;
            }
        }
#endif
        
        //函数
        public void Init(IRoad targetRoad, int blockLocalIndex)
        {
            Road = targetRoad;
            BlockLocalIndex = blockLocalIndex;
            TileHolder = GetComponentInChildren<ClassicTileHolder>() ?? gameObject.AddComponent<ClassicTileHolder>();
            BlockDisplay = GetComponentInChildren<ClassicBlockDebug>() ?? gameObject.AddComponent<ClassicBlockDebug>();
        }
    }
}
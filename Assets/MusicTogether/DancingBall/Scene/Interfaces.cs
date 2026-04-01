using System;
using System.Collections.Generic;
using MusicTogether.DancingBall.Data;
using UnityEngine;
using MusicTogether.DancingBall.Player;
using MusicTogether.DancingBall.EditorTool;

namespace MusicTogether.DancingBall.Scene
{
    /// <summary>
    /// 内置在Prefab里面。控制Tile的布局。拓展内容：动画。
    /// </summary>
    public interface ITileHolder
    {
        public void SetTileActive(bool forward, bool backward, bool bottom = true);
        /// <summary>
        /// 返回所有已启用的地板Transform和他们的厚度。
        /// </summary>
        public List<(Transform, float)> GetTilePoses();//由于Tile可能存在移动，这里使用Transform确保实时跟踪最新位置。
    }

    /// <summary>
    /// 自动添加。显示调试信息。为了不让Block过于臃肿，BlockDisplay被设计成一个独立的组件。拓展内容：显示的信息。
    /// </summary>
    public interface IBlockDebug
    {
        public void RefreshBlockDisplay(Color color);
        public void DisplayBlockInformation(string info);
    }
    
    /// <summary>
    /// 自动添加。管理Block的位置，负责完成组件与Road通信。拓展内容：无
    /// </summary>
    public interface IBlock
    {
        //外部引用
        public IRoad Road { get; }//自动创建双向引用。
        //本体绑定信息
        public Transform Transform { get; }
        public ITileHolder TileHolder { get; }
        public IBlockDebug BlockDisplay { get; }
        //参数
        public int BlockLocalIndex { get; set; }
        //函数
        public void Init(IRoad targetRoad, int blockLocalIndex);
        //public List<MovementData> GetBlockMovementData();时间计算由Player完成。
    }

    /// <summary>
    /// 自动添加。管理一段的Block。负责完成组件与Map通信。拓展内容：无
    /// </summary>
    public interface IRoad
    {
        //外部引用
        public IMap Map { get; }
        public RoadData RoadData { get; }
        //本体绑定信息
        public Transform Transform { get; }
        public List<IBlock> Blocks { get; }
        //参数
        //public string TargetRoadData { get; } 实现接口的类自己声明。用于RoadData丢失时访问。
        
        //函数
        public void Init(IMap map, RoadData roadData);
        
        #region Road_Operations //操作功能
            //物体操作================================================================================================
            public void RefreshRoadBlocks();
            public void OnBlockDisplacementRuleChanged();
            public void RefreshBlockInfoDisplay();
            
            //数据操作================================================================================================
            //Road级别
            public void ModifyBlockBeginIndex(int newBeginIndex);
            public void ModifyBlockEndIndex(int newEndIndex);
            public void ModifyTargetRoadDataName(string newName);
            
            //Block级别
            public void ModifyDisplacementData(int blockLocalIndex, IBlockDisplacementData newDisplacementData);
            
            //数据获取
            public List<MovementData> GetBlockMovementDatum(int blockBeginIndex);
            
        #endregion
        
        #region Road_MapFunctions //地图操作
            public IBlock CreateBlock(int blockLocalIndex);
            public List<IBlock> CreateBlocks(IEnumerable<int> index);
            public List<IBlock> CreateBlocks(int indexBegin, int count);
        #endregion
    }

    public interface IMap
    {
        public const int MaxBlockCountThreshold = 5000;
        //外部绑定
        //自身绑定
        public Transform Transform { get; }
        public EditManager EditManager { get; }
        public SceneData SceneData { get; }
        public List<IRoad> Roads { get; }

        //操作功能
        public void RebuildRoads();
        
        //地图操作
        public void AddRoads(List<RoadData> roadDataToAdd);
        public void RemoveRoads(List<IRoad> roadsToRemove);
    }
}
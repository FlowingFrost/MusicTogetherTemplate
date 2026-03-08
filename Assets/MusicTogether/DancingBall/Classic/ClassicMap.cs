using System.Collections.Generic;
using MusicTogether.DancingBall.Interfaces;
using MusicTogether.General;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicMap : SerializedBehaviour, IMap
    {
        //引用资源
        [SerializeField] protected ILevelManager levelManager;
        [SerializeField] protected InputNoteData noteData;
        //其它引用信息
        protected List<IRoad> roads;
        //运行数据
        protected double levelTime => levelManager.LevelTime;
        //API
        public InputNoteData NoteData => noteData;
        public IReadOnlyList<IRoad> Roads => roads;
        //数据库API
        public IRoad PreviousRoad(IRoad currentRoad) => currentRoad.RoadIndex <= 0 ? null : roads[currentRoad.RoadIndex - 1];
        public IRoad NextRoad(IRoad currentRoad) => currentRoad.RoadIndex >= roads.Count - 1 ? null : roads[currentRoad.RoadIndex + 1];
        //public IBlock FirstBlockInRoad(IRoad road) => road.FirstBlock;
        //public IBlock LastBlockInRoad(IRoad road) => road.LastBlock;
        //复杂查询
        public IRoad GetRoadByBlock(IBlock block)
        {
            foreach (var road in roads)
                if (road.BlocksBehind(block) != null) return road;
            return null;
        }
        //public IBlock PreviousBlockInCurrentRoad(IBlock currentBlock) => GetRoadByBlock(currentBlock).PreviousBlock(currentBlock);
        //public IBlock NextBlockInCurrentRoad(IBlock currentBlock) => GetRoadByBlock(currentBlock).NextBlock(currentBlock);
        //public IEnumerable<IBlock> BlockBehindTillNextTap(IBlock currentBlock) => GetRoadByBlock(currentBlock).BlockBehindTillNextTap(currentBlock);
        //public IEnumerable<IBlock> BlockBehindTillNextCorner(IBlock currentBlock) => GetRoadByBlock(currentBlock).BlockBehindTillNextCorner(currentBlock);
        //public IEnumerable<IBlock> BlocksBehindInCurrentRoad(IBlock currentBlock) => GetRoadByBlock(currentBlock).BlocksBehind(currentBlock);


    }
}
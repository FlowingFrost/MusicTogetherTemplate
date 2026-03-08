using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Interfaces;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicRoad : SerializedBehaviour, IRoad
    {
        //绑定信息
        protected IMap map;
        protected List<IBlock> blocks;
        //参数
        [ReadOnly] [SerializeField] protected int noteStartIndex;
        [SerializeField] protected int noteEndIndex;
        [ReadOnly] [SerializeField] protected int blockStartIndex;
        [SerializeField] protected int blockEndIndex;
        //API
        //数据API
        protected InputNoteData inputNoteData => map.NoteData;
        protected List<int> tempNoteList => map.NoteData.noteLists[0].notes;
        
        public int RoadIndex { get; set; }
        public int NoteStartIndex { get; set; }
        public int NoteEndIndex { get; }
        public int BlockStartIndex { get; set; }
        public int BlockEndIndex { get; }
        //块API
        public IBlock FirstBlock => blocks.FirstOrDefault();
        public IBlock LastBlock => blocks.LastOrDefault();
        public IBlock PreviousBlock(IBlock block) => block.IndexInRoad <= 0 ? null : blocks[block.IndexInRoad - 1];
        public IBlock NextBlock(IBlock block) => block.IndexInRoad >= blocks.Count - 1 ? null : blocks[block.IndexInRoad + 1];
        
        public bool IsTapBlock(IBlock block) => block.BlockMaker.HasTap;
        public bool IsCornerBlock(IBlock block) => block.BlockMaker.HasTap || block.BlockMaker.HasRule;
        
        public int NextTapBlockID(IBlock block)
        {
            for (int i = block.IndexInRoad + 1; i < blocks.Count; i++)
            {
                if (IsTapBlock(blocks[i])) return i;
            }
            return blocks.Count - 1; //如果没有转弯块，返回最后一个块的索引
        }
        public int NextCornerBlockID(IBlock block)
        {
            for (int i = block.IndexInRoad + 1; i < blocks.Count; i++)
            {
                if (IsCornerBlock(blocks[i])) return i;
            }
            return blocks.Count - 1; //如果没有转弯块，返回最后一个块的索引
        }
        public IEnumerable<IBlock> GetBlocksInRange(int startIndex, int endIndex)
        {
            return blocks.GetRange(startIndex, endIndex - startIndex + 1);
        }
        
        public IEnumerable<IBlock> BlocksBehind(IBlock block) => blocks.Take(block.IndexInRoad);
        public IEnumerable<IBlock> BlockBehindTillNextTap(IBlock block) => GetBlocksInRange(block.IndexInRoad, NextTapBlockID(block));
        public IEnumerable<IBlock> BlockBehindTillNextCorner(IBlock block) => GetBlocksInRange(block.IndexInRoad, NextCornerBlockID(block));
        /// <summary>
        /// 将后续方块拆分为根据corner排列的节，每一节的开头和结尾都是Corner。例：[CurrentBlock,A,B,C,D][D,E,F,G,...,K][K,L,M,LastBlock]
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public IEnumerable<IEnumerable<IBlock>> BlockBehindSplitByCorner(IBlock block)
        {
            IEnumerable<IEnumerable<IBlock>> result = new List<List<IBlock>>();
            while(NextBlock(block) != null)
            {
                var nextCornerID = NextCornerBlockID(block);
                var blockSegment = GetBlocksInRange(block.IndexInRoad, nextCornerID);
                result.Append(blockSegment);
                block = blocks[nextCornerID];
            }
            return result;
        }
        
        
        public double GetTimeAtBlock(IBlock block)
        {
            var noteIndex = NoteStartIndex + block.IndexInRoad;
            if (noteIndex < NoteStartIndex || noteIndex > NoteEndIndex)
            {
                Debug.LogError("Block index out of range of notes.");
                return -1;
            }
            return inputNoteData.noteLists[0].GetNoteTimeAt(noteIndex);
        }

        public List<MovingData> GetMovingDatas(IBlock beginBlock)
        {
            var begin = beginBlock.IndexInRoad;
            var end = NextTapBlockID(beginBlock);
            var movingDataList = new List<MovingData>();
            //movingDataList = GetBlocksInRange(begin, end).Select( block => block.GetMovingData()).ToList();
            for (int i = beginBlock.IndexInRoad; i <= end; i++)// 二者等价，但上面更简洁。
            {
                var block = blocks[i];
                //movingDataList.Add(block.);
            }

            return movingDataList;
        }
        
        
        public void RefreshTurnData()
        {
            var noteList = map.NoteData.noteLists[0].notes;
            var notesInRange = noteList.Where(note => note >= NoteStartIndex && note <= NoteEndIndex);
            //简单实现，但是存在性能问题。
            foreach (var block in blocks)
            {
                block.BlockMaker.HasTap = notesInRange.Any(note => note == BlockStartIndex + block.IndexInRoad);
            }
        }
        
        
    }
}
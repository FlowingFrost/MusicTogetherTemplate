using System;
using MusicTogether.DancingBall.Interfaces;
using Unity.Collections;
using UnityEngine;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicRoadMaker : IRoadMaker
    {
        public IRoad Road { get; set; }
        [SerializeField] [ReadOnly] protected int blockStartIndex;
        [SerializeField] protected int blockEndIndex;
        [SerializeField] protected bool acceptFormerBlocks;
        
        public int NoteStartIndex { get; set; }
        public int NoteEndIndex { get; }
        public int BlockStartIndex { get; set; }
        public int BlockEndIndex { get; }
        public bool AcceptFormerBlocks => acceptFormerBlocks;

        public Action UpdateBlockRange => () => EditManager.Instance.OnBlockRangeChanged(this);
    }
}
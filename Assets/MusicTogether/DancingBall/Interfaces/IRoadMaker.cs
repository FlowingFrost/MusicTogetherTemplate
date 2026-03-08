namespace MusicTogether.DancingBall.Interfaces
{
    public interface IRoadMaker
    {
        IRoad Road { get; set; }
        public int BlockStartIndex { get; set; }
        public int BlockEndIndex { get; }
        public bool AcceptFormerBlocks { get; }
    }
}
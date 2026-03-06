namespace MusicTogether.DancingBall.Interfaces
{
    //public enum DisplacementType {None, }
    public interface IDisplacementRule
    {
        bool HasRule { get; }
        void SetPosition(IBlock rootBlock, IBlock currentBlock);
    }
}
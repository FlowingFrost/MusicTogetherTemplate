namespace MusicTogether.DancingBall.Interfaces
{
    public interface IBlock
    {
        public IDisplacementRule DisplacementRule { get; }
        public int IndexInRoad { get; set; }
        //public int IndexInMap { get; }
        
    }
}
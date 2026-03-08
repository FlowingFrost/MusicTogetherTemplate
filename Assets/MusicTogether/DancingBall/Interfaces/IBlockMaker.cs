using System.Collections.Generic;

namespace MusicTogether.DancingBall.Interfaces
{
    public enum TurnType { None, Left, Right }
    public enum DisplacementType { None, Up, Down }
    public interface IBlockMaker
    {
        IBlock Block { get; }
        bool HasTap { get; set; }
        bool HasRule { get; set; }
        TurnType TurnType { get; }
        DisplacementType DisplacementType { get; }
    }
}
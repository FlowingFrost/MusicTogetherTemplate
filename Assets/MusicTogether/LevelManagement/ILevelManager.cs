using System;

namespace MusicTogether.LevelManagement
{
    public enum LevelState {Playing, Paused, Previewing}
    public interface ILevelManager
    {
        public LevelState CurrentLevelState { get; }
        public double LevelTime { get; }
        public double LevelProgress { get; set; }
        public void SetLevelState(LevelState state);
        public event Action<LevelState> OnLevelStateChanged;
    }
}
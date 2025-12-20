using System;
using System.Collections.Generic;

namespace MusicTogether.LevelManagement
{
    public enum LevelState
    {
        Playing,
        Paused,
        Previewing
    }

    public interface ILevelManager
    {
        //绑定资源
        public event Action<LevelState> OnLevelStateChanged;
        public event Action OnEditorModeEnter;
        //参数
        public bool IsEditorPreviewing { get; }
        public LevelState CurrentLevelState { get; }
        public double LevelTime { get; }
        public double LevelProgress { get; set; }
        //方法
        public void SetLevelState(LevelState state);
        //功能
        public void RefreshEditorMode();
    }
    
    public interface ILevelUnion
    {
        // ILevelManager LevelManager { get; }
        public void AwakeUnion();
        public void StartUnion();
        public void UpdateUnion();
    }
    
    [Serializable]
    public enum UnionUpdateMode
    {
        PlayMode,
        EditorMode,
        Both
    }
}
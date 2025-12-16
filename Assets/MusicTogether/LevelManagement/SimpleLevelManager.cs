using System;
using LiteGameFrame.CoreInfrastructure;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace MusicTogether.LevelManagement
{
    public class SimpleLevelManager : Singleton<SimpleLevelManager>, ILevelManager//, IStateHandler
    {
        //绑定
        public PlayableDirector levelDirector;

        public float fps;
        //参数
        private double previousTime;
        private LevelState currentLevelState = LevelState.Paused;

        public LevelState CurrentLevelState
        {
            get => currentLevelState;
            private set
            {
                currentLevelState = value;
                OnLevelStateChanged?.Invoke(value);
            }
        }
        public double LevelTime => levelDirector.time;
        public double LevelProgress { get; set; } = 0;
        public event Action<LevelState> OnLevelStateChanged;


        public void SetLevelState(LevelState state)
        {
            switch (state)
            {
                case LevelState.Playing:
                    levelDirector.Play();
                    previousTime = LevelTime;
                    LevelProgress = LevelTime;
                    CurrentLevelState = state;
                    break;
                case LevelState.Paused:
                    levelDirector.Pause();
                    CurrentLevelState = state;
                    break;
                default:
                    CurrentLevelState = state;
                    break;
            }
        }
        
        private void Update()
        {
            switch (CurrentLevelState)
            {
                case LevelState.Playing:
                    if (levelDirector.state == PlayState.Paused)
                    {
                        CurrentLevelState = LevelState.Paused;
                        break;
                    }
                    
                    if (LevelTime < LevelProgress || LevelTime <= previousTime)
                    {
                        CurrentLevelState = LevelState.Previewing;
                        break;
                    }
                    
                    if (Mathf.Abs((float)(LevelTime - previousTime)) > 1/fps)
                    {
                        Debug.Log($"Level time changed abnormally from {previousTime} to {LevelTime}, tolerance {1/fps}");
                    }
                    LevelProgress = LevelTime;
                    break;
                case LevelState.Paused:
                    if (levelDirector.state == PlayState.Playing)
                    {
                        if (LevelTime < LevelProgress)
                            CurrentLevelState = LevelState.Previewing;
                        else
                            CurrentLevelState = LevelState.Playing;
                    }
                    break;
                case LevelState.Previewing:
                    if (levelDirector.state == PlayState.Paused)
                    {
                        CurrentLevelState = LevelState.Paused;
                    }
                    if (LevelTime >= LevelProgress)
                    {
                        CurrentLevelState = LevelState.Playing;
                    }
                    break;
            }
            previousTime = LevelTime;
        }
    }
}
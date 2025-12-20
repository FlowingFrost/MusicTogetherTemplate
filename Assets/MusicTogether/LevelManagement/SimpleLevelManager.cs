using System;
using System.Collections;
using System.Collections.Generic;
using LiteGameFrame.CoreInfrastructure;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace MusicTogether.LevelManagement
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class SimpleLevelManager : SerializedMonoBehaviour, ILevelManager//, IStateHandler,Singleton<SimpleLevelManager>
    {
        //绑定
        public PlayableDirector levelDirector;
        public float fps;
        //参数
        private double previousTime;
        private LevelState currentLevelState = LevelState.Paused;
        //API
        public ILevelManager LevelManager => this;
        public event Action<LevelState> OnLevelStateChanged;
        public event Action OnEditorModeEnter;
        public bool IsEditorPreviewing => Application.isEditor && !Application.isPlaying;
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
        
        public void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Debug.Log("PlayMode State Changed: " + state);
#if UNITY_EDITOR
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                RefreshEditorMode();
                //StartCoroutine(WaitForEditModeEnter());
            }
#endif
        }
        
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

        [Button("重新刷新")]
        public void RefreshEditorMode()
        {
            OnEditorModeEnter?.Invoke();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
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

        // ILevelUnion 实现 - LevelManager 本身不需要主动更新子 Union
        // 子 Union 由 Timeline 上的 LevelUnionTrack Clip 独立驱动
    }
}
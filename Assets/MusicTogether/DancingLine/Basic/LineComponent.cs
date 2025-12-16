using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Classic;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.Basic
{
    public class LineComponent : ClassicLineComponent
    {
        public TimeLinePlayerBehaviour timeLine;
        public CanvasCounter canvasCounter;
        public TextMeshProUGUI debugText;
        public Material lineMaterial;
        public Color playingColor = Color.white;
        public Color previewingColor = Color.gray;
        public Color pausedColor = Color.yellow;
        private string stateText = "游玩中";
        private double PauseTime;

        public override void OnLevelStateChanged(LevelState newState)
        {
            stateText = "游玩中";
            Color targetColor = playingColor;
            switch (newState)
            {
                case LevelState.Previewing:
                    stateText = "预览中";
                    targetColor = previewingColor;
                    break;
                case LevelState.Paused:
                    stateText = "已暂停";
                    targetColor = pausedColor;
                    break;
            }
            lineMaterial.color = targetColor;
            debugText.color = targetColor;
            debugText.text = $"当前关卡状态 : {stateText}\n当前线运动状态 : {CurrentMotionType}";
        }

        public override void OnGroundedChanged(IDirection direction, bool grounded, float groundHeight)
        {
            base.OnGroundedChanged(direction, grounded, groundHeight);
            debugText.text = $"当前关卡状态 : {stateText}\n当前线运动状态 : {CurrentMotionType}";
        }
        
        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) //写在这里是不合适的，只作为临时方案
            {
                if (timeLine.NowPlayState == PlayState.Playing)
                {
                    timeLine.Pause();
                    PauseTime = timeLine.NowTime;
                    
                    timeLine.InitialTime = PauseTime - 2f;
                    timeLine.NowTime = PauseTime - 2f;
                    LevelManager.LevelProgress = PauseTime;
                    canvasCounter.beginTime = PauseTime;
                    
                }
                else
                {
                    int currentDirectionID = pool.LineNodes[pool.CurrentIndex].Direction.ID;
                    controller.SetCurrentDirection(currentDirectionID);
                    ClearNodesAfterTime(PauseTime);
                    timeLine.Play();
                }
            }
            base.Update();
            //debugText.text = ((LinePool)pool).DebugInformation();
            //debugText.text = $"当前关卡状态：{LevelState}";
            switch (LevelState)
            {
                case LevelState.Previewing:
                    canvasCounter.UpdateProgress(Time);
                    break;
                default:
                    canvasCounter.UpdateProgress(PauseTime+1);
                    break;
            }
            
        }
    }
}
using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.Basic
{
    public class LineComponent : BaseLineComponent
    {
        public TimeLinePlayerBehaviour timeLine;
        public CanvasCounter canvasCounter;
        public TextMeshProUGUI debugText;
        private double Time => LevelManager.LevelProgress;
        private double PauseTime;
        public override void Move()
        {
            transform.position = pool.GetPosition(Time);
        }

        public override void Turn()
        {
            pool.AddNode(Time);
        }

        public override void Turn(IDirection direction)
        {
            pool.AddNode(Time,direction);
        }

        private void Awake()
        {
            if (pool.LineNodes.Count == 0)
                pool.AddNode(0,controller.CurrentDirection());
            controller.Register(Turn, Turn);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) //写在这里是不合适的，只作为临时方案
            {
                if (timeLine.NowPlayState == PlayState.Playing)
                {
                    timeLine.Pause();
                    PauseTime = timeLine.NowTime;
                    
                    timeLine.InitialTime = PauseTime - 2f;
                    timeLine.NowTime = PauseTime - 2f;
                    
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
            controller.DetectInput();
            Move();
            debugText.text = ((LinePool)pool).DebugInformation();
            canvasCounter.UpdateProgress(Time);
        }
        
        [Button("清除当前时间点之后的节点")]
        public void ClearNodesAfterNow()
        {
            ClearNodesAfterTime(Time);
        }
        public void ClearNodesAfterTime(double time)
        {
            if (pool.GetType().IsAssignableFrom(typeof(LinePool)))
            { 
                ((LinePool)pool).ClearLaterNodes(time);
            }
        }
    }
}
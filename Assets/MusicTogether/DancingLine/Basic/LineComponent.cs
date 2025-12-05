using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.Basic
{
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    #endif
    public class LineComponent : BaseLineComponent
    {
        public bool RefreshOnEditorChange = true;
        public PlayableDirector timeLine;
        public CanvasCounter canvasCounter;
        private double Time => LevelManager.LevelProgress;
        private double PauseTime;
        public override void Move()
        {
            transform.position = pool.GetPosition(Time);
        }
        public override void Turn(IDirection direction)
        {
            pool.AddNode(Time,direction);
        }

        private void Awake()
        {
            if (pool.LineNodes.Count == 0)
                pool.AddNode(0,controller.CurrentDirection());
            controller.RegisterTurn(Turn);
        }
        void Update()
        {
            if (!RefreshOnEditorChange && !Application.isPlaying) return;
            if (Input.GetKeyDown(KeyCode.Escape)) //写在这里是不合适的，只作为临时方案
            {
                if (timeLine.state == PlayState.Playing)
                {
                    timeLine.Pause();
                    PauseTime = timeLine.time;
                    
                    timeLine.initialTime = PauseTime - 2f;
                    timeLine.time = PauseTime - 2f;
                    
                    canvasCounter.beginTime = PauseTime;
                    
                }
                else
                {
                    int currentDirectionID = pool.LineNodes[pool.CurrentIndex].Direction.ID;
                    controller.SetCurrentDirection(currentDirectionID);
                    ClearNodesAfterTime(PauseTime - 1f);
                    timeLine.Play();
                }
            }
            controller.DetectInput();
            Move();
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
using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    public class LineComponent : BaseLineComponent
        <LineController,Direction,LinePool,LineNode,LineTail>
    {
        private double Time => LevelManager.LevelProgress;
        
        public override void Move()
        {
            transform.position = pool.GetPosition(Time);
        }
        public override void Turn(Direction direction)
        {
            pool.AddNode(Time,direction.DirectionVector);
        }

        private void Awake()
        {
            pool.AddNode(0,controller.CurrentDirection().DirectionVector);
            controller.Register(Turn);
        }
        void Update()
        {
            controller.DetectInput();
            Move();
        }
    }
}
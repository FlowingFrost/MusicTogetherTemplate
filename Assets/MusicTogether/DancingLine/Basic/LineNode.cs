using System;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    public class LineNode : BaseLineNode<LineTail>
    {
        public LineNode(double beginTime, Vector3 directionVector, Transform lineContainer) : base(beginTime, directionVector)
        {
            Tail = new LineTail(directionVector, lineContainer);
        }

        public override void SetActive(bool active)
        {
            Tail.SetActive(active);
        }
        public override Vector3 GetNodePosition(double time)
        {
            float deltaTime = (float)(time - BeginTime);
            return BeginPosition + DirectionVector*deltaTime;
        }
        public override Vector3 UpdateNode(double time)
        {
            float deltaTime = (float)(time - BeginTime);
            if (deltaTime >= 0)
            {
                Tail.SetActive(true);
                Tail.UpdateTail(deltaTime);
                return GetNodePosition(time);
            }
            else
            {
                Tail.SetActive(false);
                return BeginPosition;
            }
        }

        public override void DeleteNode()
        {
            Tail.DeleteTail();
            Tail = null;
        }
    }
}
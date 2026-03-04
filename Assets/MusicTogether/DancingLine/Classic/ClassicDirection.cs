using System;
using MusicTogether.DancingLine.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Classic
{
    [Serializable]
    public class ClassicDirection : IDirection
    {
        [SerializeField] private int id;
        [SerializeField] private int nextDirectionID;
        [SerializeField] private Vector3 directionVector;
        
        public int ID { get => id; set => id = value; }
        public int NextDirectionID { get => nextDirectionID; set => nextDirectionID = value; }

        public Quaternion Rotation => Quaternion.LookRotation(directionVector);
        public MotionState GetLineHeadMotionState(Vector3 startPoint, double time)
        {
            return new MotionState(){
                ParentSpacePosition = startPoint + directionVector * (float)time,
                ParentSpaceRotation = Quaternion.LookRotation(directionVector)
            };
        }

        public MotionState UpdatePosition(Vector3 startPoint, double time, Transform lineTailTransform)
        {
            var motionState = GetLineHeadMotionState(startPoint, time);
            lineTailTransform.localPosition = (startPoint + motionState.ParentSpacePosition - directionVector.normalized) / 2;// - motionState.ParentSpacePosition.normalized
            lineTailTransform.localRotation = motionState.ParentSpaceRotation;
            lineTailTransform.localScale = new Vector3(1, 1, (motionState.ParentSpacePosition - startPoint).magnitude);
            return motionState;
        }
    }
}
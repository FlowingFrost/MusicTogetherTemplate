using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall.Interfaces
{
    public class MovingData
    {
        public IBlock targetBlock { get; }
        public List<ValueTuple<double, Vector3>> TimePositionPairs { get; }
        
        public event Action<Vector3> OnTargetChanged;
        private double beginTime => TimePositionPairs[0].Item1;
        private double endTime => TimePositionPairs[^1].Item1;
        private Vector3 beginPosition => TimePositionPairs[0].Item2;
        private Vector3 endPosition => TimePositionPairs[^1].Item2;
        private int currentIndex = 0;
        
        public MovingData (IBlock targetBlock, List<ValueTuple<double, Vector3>> timePositionPairs)
        {
            this.targetBlock = targetBlock;
            TimePositionPairs = timePositionPairs;
        }
        
        public void SetBegin(ValueTuple<double, Vector3> begin)
        {
            TimePositionPairs.Insert(0, begin);
            currentIndex = 0;
        }
        
        public Vector3 CurrentPosition(float currentTime)
        {
            if (currentTime <= beginTime) return beginPosition;
            if (currentTime >= endTime) return endPosition;
            
        }
    }
    public interface IBallPlayer
    {
        
    }
}
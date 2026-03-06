using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.General
{
    [Serializable]
    public struct TimeRange
    {
        [HorizontalGroup("time")] public float startTime;
        [HorizontalGroup("time")] public float endTime;

        public TimeRange(float begin, float end)
        {
            startTime = begin;
            endTime = end;
        }

        public TimeRange(float timeStamp, TimeRange deltaTime)
        {
            startTime = timeStamp + deltaTime.startTime;
            endTime = timeStamp + deltaTime.endTime;
        }

        public double GetProgress(double currentTime)
        {
            return (currentTime - startTime) / (endTime - startTime);
        }

        public bool InRange(double timeStamp)
        {
            return timeStamp >= startTime && timeStamp <= endTime;
        }
    }
}
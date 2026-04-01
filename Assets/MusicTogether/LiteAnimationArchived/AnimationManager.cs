using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.LiteAnimation
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeReference]
        public List<IBaseFixAnimation> animations = new List<IBaseFixAnimation>();
        
        private bool isPlaying = false;
        
        private void Initialize()
        { foreach (var anim in animations) anim.Init(); }

        public void PlayAnimation(double time)
        {
            if (!isPlaying) return;
            foreach (var anim in animations)
            { 
                if (anim.enabled && anim.timeRange.InRange(time))
                { anim.PlayAnimation(time); } 
            }
        }

        public TimeRange GetMaxTimeRange()
        {
            float begin = animations.Select(a => a.timeRange.startTime).OrderBy(a=>a).First();
            float end = animations.Select(a => a.timeRange.endTime).OrderBy(a=>a).Last();
            return new TimeRange(begin, end);
        }
        
        public void AddAnimation<T>() where T : IBaseFixAnimation, new()
        {
            animations.Add(new T());
        }
        public void AddAnimation<T>(T animation) where T : class, IBaseFixAnimation
        {
            animations.Add(animation);
        }
        public void AddMoveAnimation() => AddAnimation<TransformAnimation>();
        public void AddColorAnimation() => AddAnimation<ColorAnimation>();
    }
}
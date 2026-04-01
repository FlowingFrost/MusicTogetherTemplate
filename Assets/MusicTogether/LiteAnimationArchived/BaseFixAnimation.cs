using System;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.LiteAnimation
{
    public interface IBaseFixAnimation
    {
        bool enabled{get;set;}
        TimeRange timeRange{get;set;}
        AnimationCurve curve{get;set;}
        void Init();
        void PlayAnimation(double time);
    }
    public enum AnimationType { Color, Move, Rotate, Scale }
    [Serializable]
    public abstract class BaseFixAnimation<T> : IBaseFixAnimation where T : class
    {
        [field: SerializeField]public bool enabled{get;set;} = true;
        [field: SerializeField]public TimeRange timeRange{get;set;}
        [field: SerializeField]public AnimationCurve curve{get;set;} = AnimationCurve.Linear(0,0,1,1);
        [SerializeField] protected T target;
        public virtual void Init()
        {
            if (target == null)
            {
                throw new ArgumentException("动画目标无效");
            }
        }
        public abstract void PlayAnimation(double time);

        protected float Evaluate(double time)
        {
            float normalizedTime = (float)timeRange.GetProgress(time);
            return curve.Evaluate(normalizedTime);
        }
    }
}

using System;
using UnityEngine;

namespace MusicTogether.LiteAnimation
{
    public enum TransformAnimationType{Position,Rotation,Scale}
    [Serializable]
    public class TransformAnimation : BaseFixAnimation<Transform>
    {
        public TransformAnimationType animationType = TransformAnimationType.Position;
        public Vector3 begin;
        public Vector3 end;
        public bool local;
        
        public override void PlayAnimation(double time)
        {
            if (!enabled) return;
            float t = Evaluate(time);
            Vector3 value = Vector3.Lerp(begin, end, t);
            switch (animationType)
            {
                case TransformAnimationType.Position:
                    if (local) target.localPosition = value;
                    else target.position = value;
                    break;
                case TransformAnimationType.Rotation:
                    if (local) target.localEulerAngles = value;
                    else target.eulerAngles = value;
                    break;
                case TransformAnimationType.Scale:
                    target.localScale = value;
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseLineController : ILineController
    {
        event Action<IDirection> OnDirectionChanged;
        private List<IDirection> Directions = new List<IDirection>();
        
        protected int CurrentDirectionIndex = 0;
        
        public virtual void Register(Action<IDirection> callback)
        {
            OnDirectionChanged += callback;
        }
        public virtual IDirection CurrentDirection()
        {
            return Directions[CurrentDirectionIndex];
        }
        public abstract void DetectInput();
    }
}
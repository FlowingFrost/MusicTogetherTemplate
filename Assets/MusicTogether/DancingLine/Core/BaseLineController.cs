using System;
using System.Collections.Generic;
using System.Threading;
using Vector3 = System.Numerics.Vector3;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseLineController<T> where T : BaseDirection
    {
        public Action<T> OnDirectionChanged;
        public List<T> Directions = new List<T>();
        
        protected int CurrentDirectionIndex = 0;

        /*protected BaseLineController(Action<T> callback)
        {
            OnDirectionChanged = callback;
        }*/

        public virtual void Register(Action<T> callback)
        {
            OnDirectionChanged += callback;
        }
        public virtual T CurrentDirection()
        {
            return Directions[CurrentDirectionIndex];
        }
        public abstract void DetectInput();
    }
}
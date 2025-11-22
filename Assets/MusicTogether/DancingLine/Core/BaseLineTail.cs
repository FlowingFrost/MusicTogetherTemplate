using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 基本线尾抽象类
    /// </summary>
    public abstract class BaseLineTail
    {
        public Vector3 BeginPosition;
        public Vector3 DirectionVector;
        protected BaseLineTail(Vector3 directionVector)
        {
            DirectionVector = directionVector;
        }
        
        public virtual void SetBeginPosition(Vector3 position)
        {
            BeginPosition = position;
        }
        public abstract void UpdateTail(float deltaTime);
        public abstract bool DeleteTail();
    }
}

using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 基本线尾抽象类
    /// </summary>
    public abstract class BaseLineTail : ILineTail
    {
        public Vector3 BeginPosition;
        public Vector3 DirectionVector;
        protected GameObject _lineTailObject;
        /*protected BaseLineTail(Vector3 directionVector)
        {
            DirectionVector = directionVector;
        }*/


        public void Init(Vector3 direction)
        {
            DirectionVector = direction;
        }

        public virtual void SetBeginPosition(Vector3 position)
        {
            BeginPosition = position;
        }
        public abstract void SetActive(bool active);
        public abstract void UpdateTail(float deltaTime);
        public abstract void DeleteTail();
    }
}

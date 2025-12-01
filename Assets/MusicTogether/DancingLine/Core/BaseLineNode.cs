using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 基本输入节点信息抽象类
    /// 注：节点内声明目标线尾
    /// </summary>
    /// <typeparam name="T">基本线尾类型</typeparam>
    [Serializable]
    public abstract class BaseLineNode : ILineNode
    {
        Type TailType;
        
        public double BeginTime { get; set; }                    
        public Vector3 BeginPosition;
        public Vector3 DirectionVector;
        public ILineTail Tail;

        /*protected BaseLineNode(double beginTime, Vector3 directionVector)
        {
            this.BeginTime = beginTime;
            this.DirectionVector = directionVector;
        }*/
        protected virtual bool Validate()
        {
            if (Tail == null)
            {
                Tail = (ILineTail)Activator.CreateInstance(TailType);
            }
            if (Tail == null)
            {
                Debug.LogError($"无法创建类型为{TailType}的线身");
                return false;
            }
            return true;
        }
        
        public virtual bool AssignTailType(Type tailType)
        {
            TailType = tailType;
            if (TailType != null && typeof(ILineTail).IsAssignableFrom(TailType))
            {
                Tail = (ILineTail) Activator.CreateInstance(TailType);
                return Validate();
            }
            Debug.LogError($"分配的线尾类型{tailType}无效：未实现ILineTail接口");
            return false;
        }
        
        public virtual void Init(double time, Vector3 direction)
        {
            BeginTime = time;
            DirectionVector = direction;
        }
        public virtual void AdjustNode(Vector3 position)
        {
            BeginPosition = position;
            Tail.SetBeginPosition(position);
        }
        public virtual void AdjustNode(double time, Vector3 position)
        {
            BeginTime = time;
            AdjustNode(position);
        }

        public virtual void SetActive(bool active)
        {
            Tail.SetActive(active);
        }
        public abstract Vector3 UpdatePosition(double time);
        public abstract void DeleteNode();
    }
}

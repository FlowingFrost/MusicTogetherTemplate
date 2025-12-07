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
    public class BaseLineNode : SerializedMonoBehaviour, ILineNode
    {
        //Type TailType;
        
        public double BeginTime { get; set; }
        public IDirection Direction { get; set; }
        protected Vector3 BeginPosition;
        protected Vector3 DirectionVector;
        public ILineTail Tail;

        /*protected BaseLineNode(double beginTime, Vector3 directionVector)
        {
            this.BeginTime = beginTime;
            this.DirectionVector = directionVector;
        }*/
        protected virtual bool Validate()
        {
            /*if (Tail == null)
            {
                Tail = (ILineTail)Activator.CreateInstance(TailType);
            }*/
            if (Tail == null)
            {
                Debug.LogError($"线身丢失");
                return false;
            }
            return true;
        }
        
        /*public virtual bool AssignTailType(Type tailType)
        {
            TailType = tailType;
            if (TailType != null && typeof(ILineTail).IsAssignableFrom(TailType))
            {
                Tail = (ILineTail) Activator.CreateInstance(TailType);
                return Validate();
            }
            Debug.LogError($"分配的线尾类型{tailType}无效：未实现ILineTail接口");
            return false;
        }*/
        
        public virtual void Init(double time, IDirection direction)
        {
            BeginTime = time;
            Direction = direction;
            DirectionVector = direction.DirectionVector;
            Tail.Init(DirectionVector);
        }
        public virtual void AdjustNode(Vector3 beginPosition)
        {
            BeginPosition = beginPosition;
            Tail.SetBeginPosition(beginPosition);
        }
        public virtual void AdjustNode(double time, Vector3 beginPosition)
        {
            BeginTime = time;
            AdjustNode(beginPosition);
        }

        public virtual void SetActive(bool active)
        {
            Tail.SetActive(active);
        }
        
        public virtual Vector3 GetNodePosition(double time)
        {
            float deltaTime = (float)(time - BeginTime);
            return BeginPosition + DirectionVector*deltaTime;
        }

        public virtual Vector3 UpdatePosition(double time)
        {
            float deltaTime = (float)(time - BeginTime);
            /*超前节点隐藏判断由pool完成
            if (deltaTime >= 0)
            {
                Tail.SetActive(true);
                Tail.UpdateTail(deltaTime);
                return GetNodePosition(time);
            }
            else 
            {
                Tail.SetActive(false);
                return BeginPosition;
            }*/
            Tail.UpdateTail(deltaTime);
            return GetNodePosition(time);
        }

        public virtual void DeleteNode()
        {
            Tail.DeleteTail();
            Tail = null;
        }
    }
}

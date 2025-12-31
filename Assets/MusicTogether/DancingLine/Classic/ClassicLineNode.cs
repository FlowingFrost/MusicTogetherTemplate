using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Classic
{
    /// <summary>
    /// 基本输入节点信息抽象类
    /// 注：节点内声明目标线尾
    /// </summary>
    /// <typeparam name="T">基本线尾类型</typeparam>
    [Serializable]
    public class ClassicLineNode : SerializedMonoBehaviour, ILineNode
    {
        public double beginTime;
        public IDirection direction;
        public MotionType nodeType;
        public Vector3 beginPosition;
        public Vector3 beginVelocity;
        public Vector3 acceleration;
        public ILineTail Tail;

        //内部工具
        /*internal virtual Vector3 GetNodePosition(double time)
        {
            float deltaTime = (float)(time - BeginTime);
            return BeginPosition + DirectionVector*deltaTime;
        }*/
        //外部工具
        public double BeginTime => beginTime;
        public IDirection Direction => direction;
        public MotionType NodeType => nodeType;

        public virtual void Init(double time, IDirection direction)
        {
            beginTime = time;
            this.direction = direction;
            nodeType = MotionType.Grounded;
            Tail.Init(this.direction);
            if (direction == null) 
                throw new ArgumentNullException(nameof(direction));
            if (Tail == null)
                throw new InvalidOperationException("Tail未初始化");
        }
        public void InitMotion(Vector3 beginVelocity, Vector3 acceleration, MotionType nodeType = MotionType.Falling)
        {
            this.nodeType = nodeType;
            this.beginVelocity = beginVelocity;
            this.acceleration = acceleration;
        }
        
        public virtual void SetDirection(IDirection newDirection)
        {
            direction = newDirection;
            Tail.Init(direction);
        }

        public virtual void SetBeginTime(double newBeginTime)
        {
            beginTime = newBeginTime;
        }
        public virtual void SetBeginPosition(Vector3 newBeginPosition)
        {
            beginPosition = newBeginPosition;
            Tail.SetBeginPosition(beginPosition);
        }

        /*public virtual void SetActive(bool active)
        {
            Tail.SetActive(active);
        }*/
        
        public virtual void UpdatePosition(double endTime, out Vector3 position, double? currentTime = null)
        {
            float deltaTime = (float)(endTime - beginTime);
            switch (nodeType)
            {
                case MotionType.Grounded:
                    if (currentTime.HasValue)
                    {
                        if (currentTime.Value >= beginTime)
                        {
                            Tail.SetActive(true);
                        }
                        else 
                        {
                            Tail.SetActive(false);
                        }
                    }
                    position = Tail.UpdateTail(deltaTime);
                    break;
                default:
                    Tail.SetActive(false);
                    position = Tail.UpdateTail(deltaTime);
                    position += beginVelocity * deltaTime + acceleration * (0.5f * deltaTime * deltaTime);
                    break;
            }
        }

        public virtual void UpdatePosition(double endTime, out Vector3 position, out Vector3 velocity, double? currentTime = null)
        {
            UpdatePosition(endTime, out position, currentTime);
            velocity = beginVelocity + acceleration * (float)(endTime - beginTime);
        }
        public double GetEndTime(Vector3 endPoint)
        {
            if (nodeType == MotionType.Grounded)
            {
                Debug.LogError("节点类型为Grounded，无法计算结束时间");
                //为了避免得到NaN此处直接返回开始时间，但这是一个潜在的问题
                return beginTime;
            }
            
            // 使用投影到重力方向的一维运动学公式：s = v0*t + 0.5*a*t^2
            // 重新排列：0.5*a*t^2 + v0*t - s = 0
            
            // 计算重力方向（归一化）
            Vector3 gravityDir = acceleration.normalized;
            
            // 计算位移向量并投影到重力方向
            Vector3 displacement = endPoint - beginPosition;
            float s_proj = Vector3.Dot(displacement, gravityDir);  // 沿重力方向的位移
            
            // 投影初速度和加速度到重力方向
            float v0_proj = Vector3.Dot(beginVelocity, gravityDir);
            float a_proj = acceleration.magnitude;  // Acceleration本身就是重力，取模长（总是正值）
            
            // 一元二次方程系数：0.5*a*t^2 + v0*t - s = 0
            float a = a_proj / 2f;
            float b = v0_proj;
            float c = -s_proj;  // 注意负号
            
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                Debug.LogWarning($"无法计算结束时间，起点：{beginPosition} 到 终点：{endPoint}，沿重力方向位移：{s_proj}，判别式：{discriminant}");
                return beginTime;
            }
            
            // 解一元二次方程，选择正的时间根
            float sqrtD = Mathf.Sqrt(discriminant);
            double t1 = (-b + sqrtD) / (2 * a);
            double t2 = (-b - sqrtD) / (2 * a);
            
            // 选择较小的正根（先到达的时间）
            double solution;
            if (t1 > 0 && t2 > 0)
                solution = Math.Min(t1, t2);
            else if (t1 > 0)
                solution = t1;
            else if (t2 > 0)
                solution = t2;
            else
            {
                Debug.LogWarning($"无正时间解：t1={t1}, t2={t2}");
                return beginTime;
            }
            
            if (double.IsNaN(solution) || solution < 0)
                return beginTime;
                
            return solution + beginTime;
        }
        
        public void GetEndMotion(double endTime, out Vector3 endVelocity, out Vector3 acceleration)
        {
            float deltaTime = (float)(endTime - beginTime);
            endVelocity = beginVelocity + this.acceleration * deltaTime;
            acceleration = this.acceleration;
        }

        public virtual void DeleteNode()
        {
            Tail.DeleteTail();
            Tail = null;
        }
    }
}

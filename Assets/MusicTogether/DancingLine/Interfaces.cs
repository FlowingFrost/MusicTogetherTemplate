using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MusicTogether.LevelManagement;
using UnityEngine;

namespace MusicTogether.DancingLine
{
    /// <summary>
    /// 方向数据接口
    /// 定义了线条移动的方向信息
    /// </summary>
    public interface IDirection
    {
        int ID { get; set; }
        int NextDirectionID { get; set; }
        Vector3 DirectionVector { get; }
    }
    /// <summary>
    /// 线条控制器接口
    /// 负责检测输入并触发方向改变
    /// </summary>
    public interface ILineController
    {
        //IDirection CurrentDirection { get; }

        public event Action<int?> OnDirectionChanged;
        //public event Action<IDirection, bool, Vector3> OnGroundedChanged;
        //void Register(Action<IDirection> changeDirCallback, Action<bool, float> onGroundedChanged);
        
        /// <summary>检测输入（每帧调用）</summary>
        void DetectInput(MotionType currentMotionType);
        
    }
    
    public enum MotionType
    {
        Grounded,
        Falling,
        FallingToGrounded
    }

    /// <summary>
    /// 物理检测接口
    /// 负责检测线条与地面的碰撞
    /// </summary>
    public interface IPhysicsDetector
    {
        public event Action<bool, Vector3> OnGroundedChanged;
        public event Action<Transform> OnWallHit;
        void DetectMotionType(MotionType currentMotionType, Vector3 currentVelocity, Vector3 acceleration);
    }
    
    /// <summary>
    /// 线尾渲染接口
    /// 负责单段线的可视化
    /// </summary>
    public interface ILineTail
    {
        void Init(IDirection direction);
        void SetBeginPosition(Vector3 beginPosition);
        void SetActive(bool active);
        Vector3 UpdateTail(float deltaTime);
        void DeleteTail();
    }
    /// <summary>
    /// 线条节点接口
    /// 表示一个时间点的输入和对应的线段
    /// </summary>
    public interface ILineNode
    {
        public double BeginTime { get; }
        public IDirection Direction { get; }
        public MotionType NodeType { get;}
        void Init(double time, IDirection direction);
        void InitMotion(Vector3 beginVelocity, Vector3 acceleration, MotionType nodeType = MotionType.Falling);
        void SetDirection(IDirection newDirection);
        void SetBeginTime(double newBeginTime);
        void SetBeginPosition(Vector3 newBeginPosition);
        void UpdatePosition(double endTime, out Vector3 position, double? currentTime = null);
        void UpdatePosition(double endTime, out Vector3 position,out Vector3 velocity, double? currentTime = null);
        double GetEndTime(Vector3 endPoint);
        void GetEndMotion(double endTime, out Vector3 endVelocity, out Vector3 acceleration);
        void DeleteNode();
    }
    
    /// <summary>
    /// 线条池接口
    /// 管理所有节点并计算当前位置
    /// </summary>
    public interface ILinePool
    {
        int CurrentIndex { get; }
        bool IsEmpty { get; }
        IReadOnlyList<ILineNode> LineNodes { get; }
        //void AddNode(double time, MotionType nodeType);
        void Init(double time, IDirection direction, MotionType nodeType);
        void AddNodeByBeginPoint(Vector3 beginPoint, IDirection direction, MotionType nodeType);
        void AddNodeByBeginTime(double time, IDirection direction, MotionType nodeType, Vector3? beginVelocity = null, Vector3? acceleration = null);//极易混淆，注意区分
        void AddNode(double time, IDirection direction, Vector3 beginVelocity, Vector3 acceleration, MotionType nodeType);
        void ClearLaterNodes(double? time);
        void GetPosition(double time, out Vector3 position, out Vector3 velocity);
    }
    
    public interface ILineComponent : ILevelUnion
    {
        ILinePool Pool { get; }
        ILineController Controller { get; }
        IPhysicsDetector PhysicsDetector { get; }
        LevelState LevelState { get; }
        IDirection CurrentDirection { get; }
        Vector3 Gravity { get; }
        
        bool GetDirectionByID(int targetID, out IDirection direction);
        bool SetCurrentDirection(int targetID);
        void Move();
        void Turn(int? newDirectionID);//([CanBeNull] IDirection direction);
        //void OnGroundedChanged(bool grounded, Vector3 groundPoint);
        void SetCurrentMotionType(MotionType motionType);
        void OnGravityChanged(Vector3 newGravity);
        void ClearNodesAfterNow();
    }
}
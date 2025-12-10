using System;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 方向数据接口
    /// 定义了线条移动的方向信息
    /// </summary>
    public interface IDirection
    {
        int ID { get; set; }
        int NextDirectionID { get; set; }
        Vector3 DirectionVector { get; set; }
    }
    /// <summary>
    /// 线条控制器接口
    /// 负责检测输入并触发方向改变
    /// </summary>
    public interface ILineController
    {
        
        /// <summary>注册方向改变回调</summary>
        void Register(Action turnCallback,Action<IDirection> changeDirCallback);
        IDirection CurrentDirection();
        bool GetDirectionByID(int targetID, out IDirection direction);
        void SetCurrentDirection(int targetID);
        /// <summary>检测输入（每帧调用）</summary>
        void DetectInput();
    }
    
    /// <summary>
    /// 线尾渲染接口
    /// 负责单段线的可视化
    /// </summary>
    public interface ILineTail
    {
        void Init(Vector3 directionVector);
        void SetBeginPosition(Vector3 position);
        void SetActive(bool active);
        void UpdateTail(float deltaTime);
        void DeleteTail();
    }
    /// <summary>
    /// 线条工厂
    /// 根据用户所选创建不同类型的线条
    /// </summary>
    public interface ILineFactory
    {
        bool NewNode(out ILineNode node);
    }
    /// <summary>
    /// 线条节点接口
    /// 表示一个时间点的输入和对应的线段
    /// </summary>
    public interface ILineNode
    {
        public double BeginTime { get; set; }
        public IDirection Direction { get; set; }
        void Init(double time, IDirection direction);
        //bool AssignTailType(Type tailType);
        void AdjustNode(Vector3 beginPosition);
        void AdjustNode(double time, Vector3 position);
        
        void SetActive(bool active);
        Vector3 UpdatePosition(double time);
        void DeleteNode();
    }
    
    /// <summary>
    /// 线条池接口
    /// 管理所有节点并计算当前位置
    /// </summary>
    public interface ILinePool
    {
        int CurrentIndex { get; }
        List<ILineNode> LineNodes { get; }
        void AddNode(double time);
        void AddNode(double time, IDirection direction);
        void ClearLaterNodes(double time);
        Vector3 GetPosition(double time);
    }
    
    public interface ILineComponent
    {
        ILinePool Pool { get; }
        ILineController Controller { get; }
        void Move();
        void Turn();
        void Turn(IDirection direction);
    }
}
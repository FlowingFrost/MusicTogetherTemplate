using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Basic;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Classic
{
    /// <summary>
    /// DancingLine 组件的抽象基类
    /// 整合线条的池管理、控制器和关卡管理器
    /// </summary>
    public class ClassicLineComponent : SerializedMonoBehaviour, ILineComponent
    {
        //预定参数
        private double beginTime = 0d;
        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        [OdinSerialize]protected  List<IDirection> directions = new List<IDirection>();
        //引用
        [SerializeField]protected ILevelManager levelManager;
        [SerializeField]protected ILinePool pool;
        [SerializeField]protected ILineController controller;
        [SerializeField]protected IPhysicsDetector physicsDetector;
        protected double Time => LevelManager.LevelTime;
        //运行参数
        protected int currentDirectionID = 0;
        public virtual Vector3 Gravity => gravity;

        protected Vector3 Direction
        {
            get
            {
                return CurrentDirection.DirectionVector;
                throw new Exception($"Direction ID {currentDirectionID} not found.");
            }
        }
        public LevelState LevelState
        {
            get
            {
                return LevelManager.CurrentLevelState;
            }
        }
        public IDirection CurrentDirection
        {
            get
            {
                if (GetDirectionByID(currentDirectionID, out var direction))
                {
                    return direction;
                }
                throw new Exception($"Direction ID {currentDirectionID} not found.");
            }
        }
        protected MotionType CurrentMotionType = MotionType.Grounded;

        protected Vector3 currentPosition, currentVelocity;
        //数据
        
        //接口
        public ILevelManager LevelManager => levelManager;// ??= SimpleLevelManager.Instance;
        public ILinePool Pool => pool;
        public ILineController Controller => controller;
        public IPhysicsDetector PhysicsDetector => physicsDetector;

        //公共方法
        public virtual bool GetDirectionByID(int id, out IDirection direction)
        {
            direction = directions.Find(dir => dir.ID == id);
            return direction != null;
        }
        public virtual bool SetCurrentDirection(int targetID)
        {
            if (GetDirectionByID(targetID, out _))
            {
                currentDirectionID = targetID;
                return true;
            }
            return false;
        }

        public virtual bool NextDirection()
        {
            if (GetDirectionByID(CurrentDirection.NextDirectionID, out _))
            {
                currentDirectionID = CurrentDirection.NextDirectionID;
                return true;
            }

            return false;
        }
        public virtual void Move()
        {
            pool.GetPosition(Time , out currentPosition , out currentVelocity);
            transform.localPosition = currentPosition;
        }
        
        public virtual void Turn(int? newDirectionID)
        {
            if (newDirectionID.HasValue)
            {
                SetCurrentDirection(newDirectionID.Value);
            }
            else
            {
                NextDirection();
            }
            Quaternion rotation = Quaternion.LookRotation(Direction, -gravity);
            transform.rotation = rotation;
            switch (LevelState)
            {
                case LevelState.Playing:
                    pool.AddNodeByBeginTime(Time, CurrentDirection, CurrentMotionType);
                    break;
                case LevelState.Previewing:
                    LevelManager.SetLevelState(LevelState.Playing);
                    ClearNodesAfterNow();
                    if (CurrentMotionType == MotionType.FallingToGrounded)//不规范的、临时的
                        CurrentMotionType = MotionType.Falling;
                    pool.AddNodeByBeginTime(Time, CurrentDirection, CurrentMotionType);
                    break;
            }
        }

        public virtual void OnGroundedChanged(bool grounded, Vector3 groundPoint)//这一大坨之后再改
        {
            switch (CurrentMotionType)
            {
                case MotionType.Grounded:
                    if (grounded) return;
                    pool.AddNode(Time, CurrentDirection, Vector3.zero, Gravity, MotionType.Falling);
                    CurrentMotionType = MotionType.Falling;
                    Debug.Log($"检测到离地，开始自由落体, h = {transform.position.y}");
                    break;
                case MotionType.Falling:
                    if (!grounded) return;
                    pool.AddNodeByBeginPoint(groundPoint, CurrentDirection, MotionType.Grounded);
                    CurrentMotionType = MotionType.FallingToGrounded;
                    Debug.Log($"已查找到落点，下落即将结束, h = {transform.position.y}");
                    break;
                case MotionType.FallingToGrounded:
                    if (!grounded) return;
                    CurrentMotionType = MotionType.Grounded;
                    Debug.Log($"已落地, h = {transform.position.y}");
                    break;
            }
        }

        public void SetCurrentMotionType(MotionType motionType)
        {
            CurrentMotionType = motionType;
        }
        
        public virtual void OnLevelStateChanged(LevelState newState)
        {

        }
        
        public virtual void OnGravityChanged(Vector3 newGravity)
        {
            if (LevelManager.IsEditorPreviewing) return;
            Quaternion rotation = Quaternion.LookRotation(Direction, -newGravity);
            transform.rotation = rotation;
            pool.AddNodeByBeginTime(Time,CurrentDirection,CurrentMotionType,acceleration:gravity);
        }
        
        public void ClearNodesAfterTime(double? time)
        {
            pool.ClearLaterNodes(time);
        }
        
        [Button("清除当前时间点之后的节点")]
        public void ClearNodesAfterNow()=>ClearNodesAfterTime(Time);
        
        //生命周期
        public virtual void AwakeUnion()
        {
            controller.OnDirectionChanged += Turn;
            physicsDetector.OnGroundedChanged += OnGroundedChanged;
            LevelManager.OnLevelStateChanged += OnLevelStateChanged;
        }

        public void StartUnion(double startTime = 0d)
        {
            beginTime = startTime;
            pool.Init(startTime,CurrentDirection,MotionType.Grounded);
        }

        public virtual void UpdateUnion()
        {
            if (pool.IsEmpty)
                pool.Init(beginTime,CurrentDirection,MotionType.Grounded);
            switch (LevelState)
            {
                case LevelState.Playing:
                    controller.DetectInput(CurrentMotionType);
                    physicsDetector.DetectMotionType(CurrentMotionType, currentVelocity, Gravity);
                    Move();
                    break;
                case LevelState.Previewing:
                    controller.DetectInput(CurrentMotionType);
                    Move();
                    break;
                case LevelState.Paused:
                    Move();
                    break;
            }
        }
    }
}
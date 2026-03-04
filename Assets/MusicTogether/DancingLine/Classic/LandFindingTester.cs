using System;
using JetBrains.Annotations;
using LightGameFrame.Services;
using MusicTogether.DancingLine.Interfaces;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    public class LandFindingTester : SerializedMonoBehaviour
    {
        [SerializeField] internal PhysicsDetector physicsDetector;
        [SerializeField] internal Vector3 parentSpaceBeginVelocity;
        [SerializeField] internal Vector3 parentSpaceGravity;
        
        [OdinSerialize] internal IDirection direction;

        [SerializeField] internal float currentMoveTime;
        //[SerializeField] internal bool verifyResult = true;
        [SerializeField] internal bool solveFallingTime, verifyFallingPoint, solveDisplacement;
        
        
        private DebugDrawService _debugDraw;
        private MotionState initialMS;
        public string debugInfo;
        
        private void Awake()
        {
            initialMS = new MotionState { SelfTransform = transform, ParentSpacePosition = transform.localPosition, ParentSpaceRotation = transform.localRotation };
            
            _debugDraw = DebugDrawService.Instance;
        }

        public void GetLandingPoint()
        {
            Transform selfTransform = initialMS.SelfTransform;
            MotionState cachedMS = initialMS;
            
            Vector3 parentSpaceBeginPosition = initialMS.ParentSpacePosition;
            Vector3 parentSpaceBeginVelocity = this.parentSpaceBeginVelocity;
            Vector3 parentSpaceGravity = this.parentSpaceGravity;
            Vector3 objectSpaceUp = Vector3.up; // (0,1,0)
            
            void UpdateMotion(float t)
            {
                cachedMS = direction.GetLineHeadMotionState(parentSpaceBeginPosition, t);
                cachedMS.ParentSpacePosition += ParentSpaceMovementDisplacement(t);//应该加入重力
                cachedMS.SelfTransform = selfTransform;
                _debugDraw.DrawBox(cachedMS.WorldSpacePosition, Vector3.one, cachedMS.WorldSpaceRotation, Color.yellow);
            }

            Vector3 ParentSpaceMovementDisplacement(float t) => PhysicsHelper.CalculateDisplacement(parentSpaceBeginVelocity, parentSpaceGravity, t);
            Vector3 ParentSpaceVelocity(float t) => parentSpaceBeginVelocity + parentSpaceGravity * t;
            Vector3 ObjectSpaceVelocity(float t) => cachedMS.ParentVecToObject(ParentSpaceVelocity(t));
            Vector3 objectSpaceGravity = cachedMS.ParentVecToObject(parentSpaceGravity); // 转到物体空间
            
            float ObjectSpaceUpProjection(Vector3 dir) => Vector3.Dot(objectSpaceUp, dir);
            
            UpdateMotion(currentMoveTime);
            var currentGlobalPosition = cachedMS.WorldSpacePosition;
            
            if (!solveFallingTime) return;

            if (physicsDetector.BoxColliderRaycast(cachedMS, physicsDetector.groundFindDistance, out var landingPoint, true))
            {
                _debugDraw.DrawBox(landingPoint, Vector3.one, cachedMS.WorldSpaceRotation, Color.cyan);
                var objectSpaceFallingDisplacement = cachedMS.WorldPosToObject(landingPoint);
                float h = ObjectSpaceUpProjection(objectSpaceFallingDisplacement);
                float v0 = ObjectSpaceUpProjection(ObjectSpaceVelocity(currentMoveTime));
                float g = ObjectSpaceUpProjection(objectSpaceGravity);
                
                if (!solveFallingTime) return;
                if (physicsDetector.SolveMotionTime(v0, g, currentMoveTime, h, out var result))
                {
                    var totalMoveTime = currentMoveTime + result.FinalT;
                    
                    if (!verifyFallingPoint) return;
                    if (totalMoveTime > 0)
                    {
                        //找到了落地位置(正常检测/特殊情况1)，可行。接下来完成验证
                        UpdateMotion(totalMoveTime);
                        var landingGlobalPosition = cachedMS.WorldSpacePosition;
                        var landingGlobalUpDirection = cachedMS.ObjectVecToWorld(objectSpaceUp);

                        if (physicsDetector.BoxColliderRaycast(cachedMS, physicsDetector.groundCheckDistance, out _))
                            //if (BoxColliderRaycast(landingGlobalPosition, -landingGlobalUpDirection, groundCheckDistance, out _))
                        {
                            //正常检测1/特殊情况1 验证通过，落地点存在

                            // 绘制：落地点和轨
                            _debugDraw.DrawBox(landingGlobalPosition, Vector3.one, cachedMS.WorldSpaceRotation, Color.green);
                            _debugDraw.DrawLine(currentGlobalPosition, landingGlobalPosition, Color.cyan);
                            _debugDraw.DrawSphere(landingGlobalPosition, 0.1f, Color.green);
                        }
                        else
                        {
                            //正常检测2 未检出重点地面，说明此时地面早已到达边际，或者其它情况。
                             _debugDraw.DrawBox(landingGlobalPosition, Vector3.one, cachedMS.WorldSpaceRotation, Color.red);
                            UpdateMotion(currentMoveTime);//退回到发起检测时的状态
                            result = null;
                        }
                    }
                }
                else
                {
                    //无法计算时间/验证未通过，特殊情况 234
                    //在发起检测的原始位置计算上移向量。由于物体可能存在旋转，直接使用全局向量可能会导致不准确，因此先将全局向量转换到物体空间进行投影，再转换回局部。
                    var projectedDisplacement =Vector3.Project(cachedMS.ObjectVecToParent(objectSpaceFallingDisplacement), cachedMS.ParentSpaceUpDirection);
                    result.Displacement = projectedDisplacement;
                
                    var worldDisplacement = cachedMS.ParentVecToWorld(projectedDisplacement);
                    _debugDraw.DrawArrow(currentGlobalPosition, currentGlobalPosition + worldDisplacement, Color.magenta, DrawMode.Once, 0.2f, 20f);
                    _debugDraw.DrawBox(currentGlobalPosition + worldDisplacement, Vector3.one, cachedMS.WorldSpaceRotation, Color.magenta);

                }
            }
        }

        private void Update()
        {
            initialMS.ParentSpacePosition = transform.localPosition;
            initialMS.ParentSpaceRotation = transform.localRotation;
            if (solveFallingTime) GetLandingPoint();
        }
    }
}
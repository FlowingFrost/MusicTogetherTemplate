using System;
using LightGameFrame.Services;
using MusicTogether.DancingLine.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    public class BoxColliderRaycastTester : MonoBehaviour
    {
        [SerializeField] internal BoxCollider lineHeadCollider;

        [SerializeField] internal float groundBottomCheckBottomShrinkDistance = 0.05f;
        [SerializeField] internal float groundTopCheckTopShrinkDistance = 0.05f;
        [SerializeField] internal float groundTopCheckBottomShrinkDistance = 0.00f;
        [SerializeField] internal float groundCheckDistance = 0.2f;
        [SerializeField] internal float groundFindDistance = 2f;
        [SerializeField] internal LayerMask groundLayer;
        
        private DebugDrawService _debugDraw;
        private MotionState ms;

        public string debugInfo;
        public bool result;
        public Vector3 fallPoint;
        public Vector3 displacement;
        
        internal Vector3[] DownRayOriginDisplacement
        {
            get
            {

                return downRayOriginDisplacementCache;
            }
        }
        internal Vector3[] TopRayOriginDisplacement
        {
            get
            {

                return topRayOriginDisplacementCache;
            }
        }
        [SerializeField] internal BoxCollider lineHeadColliderCache;
        internal Vector3[] downRayOriginDisplacementCache;
        internal Vector3[] topRayOriginDisplacementCache;
        
        
        private void Awake()
        {
            ms = new MotionState { SelfTransform = transform, ParentSpacePosition = transform.localPosition, ParentSpaceRotation = transform.localRotation };
            
            _debugDraw = DebugDrawService.Instance;
            
            lineHeadColliderCache = lineHeadCollider;
            Vector3 colliderSize = lineHeadCollider.size;
            topRayOriginDisplacementCache = new Vector3[4];
            for(int i = -1; i <= 1; i+=2)
            {
                for(int j = -1; j <= 1; j+=2)
                {
                    topRayOriginDisplacementCache[(i+1)/2 + j+1] = new Vector3(i * colliderSize.x / 2, colliderSize.y/2, j * colliderSize.z/2);
                }
            }
            
            lineHeadColliderCache = lineHeadCollider;
            //Vector3 colliderSize = lineHeadCollider.size;
            downRayOriginDisplacementCache = new Vector3[4];
            for(int i = -1; i <= 1; i+=2)
            {
                for(int j = -1; j <= 1; j+=2)
                {
                    downRayOriginDisplacementCache[(i+1)/2 + j+1] = new Vector3(i * colliderSize.x / 2, -colliderSize.y/2, j * colliderSize.z/2);
                }
            }
        }
        
        internal void BoxColliderRaycast(MotionState ms, float distance, out Vector3 fallPoint,
            bool detectFromTop = false)
        {
            var direction = -ms.WorldSpaceUpDirection;
            Vector3[] displacements = detectFromTop ? TopRayOriginDisplacement : DownRayOriginDisplacement;
            Vector3 shrink = detectFromTop ? Vector3.down * groundTopCheckTopShrinkDistance : Vector3.up * groundBottomCheckBottomShrinkDistance;//调整起点位置，避免与地面贴合
            
            debugInfo = $"origin: {ms.ObjectPosToWorld(shrink)}, direction: {direction}, distance: {distance}, layerMask: {groundLayer}";
            
            // 防穿模机制：检测所有点位，取最近的碰撞点
            bool hasHit = false;
            float minDistance = float.MaxValue;
            Vector3 closestFallPoint = Vector3.zero;
            int closestIndex = -1;
            
            for (int i = 0; i < displacements.Length; i++)
            {
                var origin = ms.ObjectPosToWorld(displacements[i] + shrink);
                Ray ray = new Ray(origin, direction);
                if (Physics.Raycast(ray, out var hitInfo, distance, groundLayer))
                {
                    if (hitInfo.distance < minDistance)
                    {
                        minDistance = hitInfo.distance;
                        closestFallPoint = hitInfo.point - ms.ObjectVecToWorld(DownRayOriginDisplacement[i]); //调整为物体中心位置，等效为 hitInfo.point - (origin - objPosition)
                        closestIndex = i;
                        hasHit = true;
                    }
                    Debug.DrawLine(origin, origin + direction * distance, Color.yellow);
                }
                else
                {
                    Debug.DrawLine(origin, origin + direction * distance, Color.red);
                    
                }
            }
            
            if (hasHit)
            {
                fallPoint = closestFallPoint;
                debugInfo += $"fallPoint: {fallPoint} (closest from ray {closestIndex}, distance: {minDistance})";
                _debugDraw.DrawBox(fallPoint, Vector3.one, ms.WorldSpaceRotation, Color.green, DrawMode.Once);
                result = true;
                this.fallPoint = fallPoint;
            }
            else
            {
                fallPoint = Vector3.zero;
                this.fallPoint = fallPoint;
                result = false;
            }
        }

        [Button]
        public void TestRaycast()
        {
            ms.ParentSpacePosition = transform.localPosition;
            ms.ParentSpaceRotation = transform.localRotation;
            BoxColliderRaycast(ms, groundCheckDistance, out var fallPoint);
        }

        private void Update()
        {
            TestRaycast();
        }
    }
}
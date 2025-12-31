using System;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    public class ClassicPhysicsDetector : MonoBehaviour , IPhysicsDetector
    {
        //参数
        public BoxCollider lineCollider;
        public float groundTestDistance = 2f;
        public LayerMask groundLayer;
        [SerializeField]private GameObject debugFallPoint;
        //
        [SerializeField]private bool debug = false;
        private Vector3 _fallPoint = Vector3.zero;
        //接口
        public event Action<bool, Vector3> OnGroundedChanged;
        public event Action<Transform> OnWallHit;
        
        protected virtual void InvokeGroundedChanged(bool grounded, Vector3 fallPoint)
        {
            OnGroundedChanged?.Invoke(grounded, fallPoint);
        }
        protected virtual bool GroundTest(Vector3[] rayOriginDisplacements, float distance, out Vector3 fallPoint,
            Vector3? velocity = null, Vector3? gravity = null)
        {
            bool Raycast(Vector3 begin, Vector3 direction, float distance, out Vector3 fallPoint, bool debug)
            {
                foreach (var diaplacement in rayOriginDisplacements)
                {
                    var origin = diaplacement + begin;
                    Ray ray = new Ray(origin, direction);
                    if (Physics.Raycast(ray, out var hitInfo, distance, groundLayer))
                    {
                        fallPoint = hitInfo.point - diaplacement; //调整为线条中心位置: 落点位置 + (线条中心 - 发射点位置)，即补充位移
                        if (debug)
                        {
                            Debug.DrawLine(origin, origin + direction * distance, Color.green);
                            //Debug.Log($"击中物体：{hitInfo.transform.name},{hitInfo.transform.position}");
                        }
                        return true;
                    }
                    if (debug)
                    {
                        Debug.DrawLine(origin, origin + direction * distance, Color.red);
                    }
                }

                fallPoint = Vector3.zero;
                return false;
            }
            //Debug.Log($"GroundTest called with distance: {distance}, velocity: {(velocity.HasValue? velocity.Value:0)}, acceleration: {(acceleration.HasValue?acceleration.Value:0)}");
            Vector3 colliderCenter = lineCollider.bounds.center;
            Vector3 colliderSize = lineCollider.bounds.size;
            Vector3 rayDirection = -transform.up;
            
            if (velocity == null || gravity == null)
            {
                // 简单模式：直接使用物体下方向进行射线检测
                return Raycast(colliderCenter + transform.up*0.05f, rayDirection, distance, out fallPoint, debug);
            }

            
            // 轨迹预测模式：使用半隐式欧拉法沿运动轨迹采样检测
            Vector3 v = velocity.Value;
            Vector3 a = gravity.Value;
            
            // 如果加速度为零或速度与加速度方向相反且速度很小，退化为简单检测
            if (a.sqrMagnitude < 0.001f)
            {
                rayDirection = v.sqrMagnitude > 0.001f ? v.normalized : -transform.up;
                return Raycast(colliderCenter, rayDirection, distance, out fallPoint, debug);
            }
            
            // 使用半隐式欧拉法进行轨迹预测
            float deltaTime = Time.fixedDeltaTime; // 使用固定时间步长
            // 半隐式欧拉: v(t+dt) = v(t) + a*dt, p(t+dt) = p(t) + v(t+dt)*dt
            int maxSteps = Mathf.CeilToInt(distance / (v.magnitude * deltaTime + 0.5f * a.magnitude * deltaTime * deltaTime)) + 1;
            maxSteps = Mathf.Max(maxSteps, 5); // 至少采样5个点
            maxSteps = Mathf.Min(maxSteps, 100); // 最多采样50个点，避免性能问题
            
            Vector3 currentPosition = colliderCenter;
            Vector3 currentV = v;
            Vector3 nextV = v;
            float totalDistance = 0f;
            
            for (int step = 0; step < maxSteps; step++)
            {
                // 半隐式欧拉：每次位移使用末速度。
                // 位置检测：当前位置 + 末速度 * 时间步长
                nextV = currentV + a * deltaTime;
                var displacementThisStep = nextV.magnitude * deltaTime;
                totalDistance += displacementThisStep;
                // 如果预测距离超出检测范围，停止
                if (totalDistance > distance) break;
                
                if (Raycast(currentPosition, nextV, displacementThisStep, out fallPoint, debug))
                {
                    //EditorApplication.isPaused = true;
                    return true;
                }
                currentPosition += nextV * deltaTime;
                currentV = nextV;
                // 更新速度供下一次迭代使用
                
            }

            fallPoint = Vector3.zero;
            return false;
        }
        
        public void DetectMotionType(MotionType currentMotionType, Vector3 currentVelocity, Vector3 acceleration)
        {
             Vector3 colliderCenter = lineCollider.bounds.center;
            Vector3 colliderSize = lineCollider.bounds.size;
            Vector3[] rayOriginDisplacement = new Vector3[4];
            for(int i = -1; i <= 1; i+=2)
            {
                for(int j = -1; j <= 1; j+=2)
                {
                    rayOriginDisplacement[(i+1)/2 + j+1] = new Vector3(i * colliderSize.x / 2, -colliderSize.y/2, j * colliderSize.z/2);
                }
            }
            bool grounded = false;
            string debugInfo = "";
            switch (currentMotionType)//当落地情况改变时，需要生成新的节点，但是方向不变
            {
                case MotionType.Grounded:
                    grounded = GroundTest(rayOriginDisplacement, 0.1f, out _);
                    //只检测是否会开始下落
                    if (!grounded)
                    {
                        InvokeGroundedChanged(false, colliderCenter);
                        Debug.Log($"未检测到地面接触点，{debugInfo}");
                    }
                    break;
                case MotionType.Falling:
                    grounded = GroundTest(rayOriginDisplacement, groundTestDistance, out _fallPoint, currentVelocity, acceleration);
                    
                    //提前寻找落点，在一定的高度范围内(groundTestDistance)寻找地面
                    if (grounded)
                    {
                        debugFallPoint.transform.position = _fallPoint;
                        InvokeGroundedChanged(true, _fallPoint);
                    }
                    break;
                case MotionType.FallingToGrounded:
                    grounded = GroundTest(rayOriginDisplacement, 0.1f, out _);
                    //检测什么时候到达地面
                    if (grounded)
                    {
                        InvokeGroundedChanged(true, colliderCenter);
                        Debug.Log($"成功检测到地面接触点，{debugInfo}");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentMotionType), currentMotionType, null);
            }
        }
    }
}
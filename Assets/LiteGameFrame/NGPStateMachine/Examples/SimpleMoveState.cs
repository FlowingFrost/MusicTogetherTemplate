using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Examples
{
    /// <summary>
    /// 示例：简单的移动状态脚本
    /// 
    /// 演示功能：
    /// - 最简化的 IStateHandler 实现
    /// - 条件判断后主动完成
    /// - 演示 RequestStop 的使用（条件不满足时）
    /// </summary>
    public class SimpleMoveState : MonoBehaviour, IStateHandler
    {
        [SerializeField] private Vector3 targetPosition = new Vector3(5, 0, 0);
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float arrivalThreshold = 0.1f;
        
        private bool _isMoving;
        
        public void OnStateEnter(StateContext context)
        {
            Debug.Log("[SimpleMove] Started moving");
            
            // 检查是否已经在目标位置
            if (Vector3.Distance(transform.position, targetPosition) < arrivalThreshold)
            {
                Debug.Log("[SimpleMove] Already at target, stopping");
                context.RequestStop(); // 不触发输出，直接停止
                return;
            }
            
            _isMoving = true;
        }
        
        public void OnStateUpdate(StateContext context)
        {
            if (!_isMoving) return;
            
            // 移动
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );
            
            // 检查是否到达
            if (Vector3.Distance(transform.position, targetPosition) < arrivalThreshold)
            {
                Debug.Log("[SimpleMove] Arrived at target");
                context.RequestComplete(); // 触发输出信号
                _isMoving = false;
            }
        }
        
        public void OnStateExit(StateContext context)
        {
            Debug.Log("[SimpleMove] Exited");
            _isMoving = false;
        }
    }
}

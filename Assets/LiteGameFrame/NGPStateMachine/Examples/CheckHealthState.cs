using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Examples
{
    /// <summary>
    /// 示例：黑板数据交互脚本
    /// 
    /// 演示功能：
    /// - 通过 context.StateMachine 访问状态机
    /// - 使用黑板数据在状态间传递信息
    /// - 根据黑板数据做条件判断
    /// </summary>
    public class CheckHealthState : MonoBehaviour, IStateHandler
    {
        [SerializeField] private float lowHealthThreshold = 30f;
        
        public void OnStateEnter(StateContext context)
        {
            Debug.Log("[CheckHealth] Checking player health");
            
            // 从黑板获取数据
            if (context.StateMachine.Get<float>("playerHealth", out var health))
            {
                Debug.Log($"[CheckHealth] Current health: {health}");
                
                if (health < lowHealthThreshold)
                {
                    Debug.Log("[CheckHealth] Low health detected!");
                    
                    // 设置黑板标记
                    context.StateMachine.Set("isLowHealth", true);
                }
                else
                {
                    context.StateMachine.Set("isLowHealth", false);
                }
            }
            else
            {
                Debug.LogWarning("[CheckHealth] playerHealth not found in blackboard");
            }
            
            // 立即完成检查
            context.RequestComplete();
        }
        
        public void OnStateUpdate(StateContext context)
        {
            // 检查状态不需要每帧更新
        }
        
        public void OnStateExit(StateContext context)
        {
            Debug.Log("[CheckHealth] Check completed");
        }
    }
}

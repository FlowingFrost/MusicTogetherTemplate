using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Examples
{
    /// <summary>
    /// 示例：玩家攻击状态脚本
    /// 
    /// 演示功能：
    /// 1. 实现 IStateHandler 接口
    /// 2. 使用 context.GetBinding() 获取多个绑定对象
    /// 3. 使用 context.RequestComplete() 主动完成状态
    /// 4. 展示状态生命周期的完整流程
    /// 
    /// 使用方式：
    /// 1. 将此脚本挂载到场景中的 GameObject（如 Player）
    /// 2. 在 State Graph 中创建 ScriptStateNode
    /// 3. 绑定此脚本到节点的 "targetScript" 字段
    /// 4. 添加额外绑定：animator, weapon, audioSource
    /// 5. 配置 additionalBindingFields = ["animator", "weapon", "audioSource"]
    /// </summary>
    public class PlayerAttackState : MonoBehaviour, IStateHandler
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackDuration = 1.0f;
        [SerializeField] private string attackTriggerName = "Attack";
        [SerializeField] private AudioClip attackSound;
        
        // 运行时状态
        private float _elapsedTime;
        private bool _isAttacking;
        
        // ============================================
        // IStateHandler 接口实现
        // ============================================
        
        /// <summary>
        /// 状态进入时触发
        /// </summary>
        public void OnStateEnter(StateContext context)
        {
            Debug.Log($"[PlayerAttack] State entered from node: {context.SourceId}");
            
            _isAttacking = true;
            _elapsedTime = 0f;
            
            // 从绑定中获取多个对象（使用现有的 ComponentBinding 机制）
            var animator = context.GetBinding<Animator>("animator");
            var weapon = context.GetBinding<GameObject>("weapon");
            var audioSource = context.GetBinding<AudioSource>("audioSource");
            
            // 执行攻击逻辑
            if (animator != null)
            {
                animator.SetTrigger(attackTriggerName);
                Debug.Log("[PlayerAttack] Triggered attack animation");
            }
            
            if (weapon != null)
            {
                weapon.SetActive(true);
                Debug.Log("[PlayerAttack] Weapon activated");
            }
            
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
                Debug.Log("[PlayerAttack] Playing attack sound");
            }
            
            // 也可以使用 TryGetBinding 带错误提示
            // if (context.TryGetBinding<Animator>("animator", out var anim))
            // {
            //     anim.SetTrigger(attackTriggerName);
            // }
        }
        
        /// <summary>
        /// 每帧更新（仅当状态活跃时）
        /// </summary>
        public void OnStateUpdate(StateContext context)
        {
            if (!_isAttacking) return;
            
            _elapsedTime += Time.deltaTime;
            
            // 检查是否完成攻击
            if (_elapsedTime >= attackDuration)
            {
                Debug.Log("[PlayerAttack] Attack completed, requesting next state");
                
                // 脚本主动请求完成状态 → 触发节点的输出信号 → 转到下一状态
                context.RequestComplete();
                
                _isAttacking = false;
            }
        }
        
        /// <summary>
        /// 状态退出时触发（被强制停止）
        /// </summary>
        public void OnStateExit(StateContext context)
        {
            Debug.Log($"[PlayerAttack] State exited (forced by node: {context.SourceId})");
            
            _isAttacking = false;
            
            // 清理工作
            var weapon = context.GetBinding<GameObject>("weapon");
            if (weapon != null)
            {
                weapon.SetActive(false);
                Debug.Log("[PlayerAttack] Weapon deactivated");
            }
        }
        
        // ============================================
        // 可选：Unity 生命周期方法（用于调试）
        // ============================================
        
        private void OnDrawGizmos()
        {
            if (_isAttacking)
            {
                // 在场景中显示攻击状态
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 1.5f);
            }
        }
    }
}

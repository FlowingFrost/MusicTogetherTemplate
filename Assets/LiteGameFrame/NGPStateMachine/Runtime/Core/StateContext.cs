using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态上下文
    /// 传递给 IStateHandler 的执行环境，提供与状态机交互的能力
    /// 
    /// 主要功能：
    /// 1. 访问状态机引用（黑板数据、状态查询等）
    /// 2. 获取多个绑定对象（复用 NGPStateMachine.GetComponentBinding）
    /// 3. 脚本主动控制流程（RequestComplete/RequestStop）
    /// </summary>
    public class StateContext
    {
        /// <summary>
        /// 状态机引用
        /// </summary>
        public NGPStateMachine StateMachine { get; internal set; }
        
        /// <summary>
        /// 触发此状态的节点 ID（来自 Graph 的节点 GUID）
        /// 用于获取节点的绑定对象
        /// </summary>
        public string NodeId { get; internal set; }
        
        /// <summary>
        /// 源节点 ID（触发信号的上游节点）
        /// 可用于判断是从哪个状态转换过来的
        /// </summary>
        public string SourceId { get; internal set; }
        
        // ============================================
        // 绑定对象访问（委托给 StateMachine.GetComponentBinding）
        // ============================================
        
        /// <summary>
        /// 获取当前节点的绑定对象
        /// 利用现有的 ComponentBinding 机制，支持多字段绑定
        /// </summary>
        /// <typeparam name="T">对象类型（GameObject 或 Component）</typeparam>
        /// <param name="fieldName">绑定字段名（在节点中声明）</param>
        /// <returns>绑定的对象，未找到返回 null</returns>
        public T GetBinding<T>(string fieldName) where T : UnityEngine.Object
        {
            if (StateMachine == null)
            {
                Debug.LogError($"[StateContext] StateMachine is null, cannot get binding '{fieldName}'");
                return null;
            }
            
            return StateMachine.GetComponentBinding<T>(NodeId, fieldName);
        }
        
        /// <summary>
        /// 尝试获取绑定对象（带错误提示）
        /// 如果绑定不存在，会打印错误日志
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="fieldName">绑定字段名</param>
        /// <param name="binding">输出的绑定对象</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetBinding<T>(string fieldName, out T binding) where T : UnityEngine.Object
        {
            binding = GetBinding<T>(fieldName);
            
            if (binding == null)
            {
                Debug.LogError($"[StateContext] Binding not found: nodeId={NodeId}, field={fieldName}, type={typeof(T).Name}");
                return false;
            }
            
            return true;
        }
        
        // ============================================
        // 脚本主动控制状态流程
        // ============================================
        
        /// <summary>
        /// 脚本主动请求完成状态
        /// 调用后将触发节点的输出信号，转到下一状态
        /// 
        /// 典型场景：
        /// - 动画播放完成
        /// - 移动到目标位置
        /// - 计时器结束
        /// </summary>
        public void RequestComplete()
        {
            if (StateMachine == null)
            {
                Debug.LogError("[StateContext] Cannot request complete: StateMachine is null");
                return;
            }
            
            StateMachine.NotifyScriptCompleted(NodeId);
        }
        
        /// <summary>
        /// 脚本主动请求停止（不触发输出信号）
        /// 仅从活跃列表移除，不会触发状态转换
        /// 
        /// 典型场景：
        /// - 条件不满足，提前退出
        /// - 异常情况，需要中止
        /// </summary>
        public void RequestStop()
        {
            if (StateMachine == null)
            {
                Debug.LogError("[StateContext] Cannot request stop: StateMachine is null");
                return;
            }
            
            StateMachine.NotifyScriptStopped(NodeId);
        }
    }
}

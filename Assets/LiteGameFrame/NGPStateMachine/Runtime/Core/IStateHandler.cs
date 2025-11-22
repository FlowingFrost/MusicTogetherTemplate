namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态处理器接口
    /// 场景中的 MonoBehaviour 实现此接口，接收状态机的生命周期回调
    /// 
    /// 设计理念：
    /// - 脚本作为实际的执行器，包含具体的状态逻辑
    /// - ScriptStateNode 只负责转发生命周期事件
    /// - 脚本通过 StateContext 与状态机交互
    /// </summary>
    public interface IStateHandler
    {
        /// <summary>
        /// 状态进入时触发
        /// 当状态机激活此脚本时调用（对应节点收到 OnEnter 信号）
        /// </summary>
        /// <param name="context">状态上下文，提供状态机引用、绑定访问等能力</param>
        void OnStateEnter(StateContext context);
        
        /// <summary>
        /// 每帧更新（仅当脚本被激活时）
        /// 状态机在 Update 循环中调用活跃节点，节点转发给脚本
        /// </summary>
        /// <param name="context">状态上下文</param>
        void OnStateUpdate(StateContext context);
        
        /// <summary>
        /// 状态退出时触发
        /// 当节点被其他节点的 OnExit 信号强制停止时调用
        /// </summary>
        /// <param name="context">状态上下文</param>
        void OnStateExit(StateContext context);
    }
}

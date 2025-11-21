using System;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态节点接口
    /// 定义了所有状态节点必须实现的生命周期方法
    /// </summary>
    public interface IStateNode
    {
        /// <summary>
        /// 节点唯一标识符
        /// </summary>
        string NodeId { get; }
        
        /// <summary>
        /// 所属状态机引用
        /// </summary>
        IStateMachine StateMachine { get; }
        
        /// <summary>
        /// 初始化节点，注入状态机引用
        /// </summary>
        /// <param name="stateMachine">状态机实例</param>
        void InjectStateMachine(IStateMachine stateMachine);
        
        /// <summary>
        /// 接收到 OnEnter 控制信号时调用
        /// </summary>
        /// <param name="sourceId">发送信号的源节点 ID</param>
        void OnEnterSignal(string sourceId);
        
        /// <summary>
        /// 接收到 OnExit 控制信号时调用
        /// </summary>
        /// <param name="sourceId">发送信号的源节点 ID</param>
        void OnExitSignal(string sourceId);
        
        /// <summary>
        /// 每帧更新（仅当节点在活跃列表中时被调用）
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 清理资源
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// 节点主动请求停止运行
        /// 内部会调用 StateMachine.RemoveActiveNode(this)
        /// </summary>
        void StopRunning();
    }
}

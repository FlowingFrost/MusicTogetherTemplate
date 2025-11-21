using System;
using System.Collections.Generic;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态机接口
    /// 定义了状态机的核心操作
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// 黑板操作：获取值
        /// </summary>
        bool Get<T>(string key, out T value);
        
        /// <summary>
        /// 黑板操作：设置值
        /// </summary>
        bool Set<T>(string key, T value);
        
        /// <summary>
        /// 黑板操作：查找类型
        /// </summary>
        bool Find<T>(string key, out Type valueType);
        
        /// <summary>
        /// 处理节点接收到 OnEnter 信号
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="sourceId">源节点 ID</param>
        void ProcessEnterSignal(IStateNode node, string sourceId);
        
        /// <summary>
        /// 处理节点接收到 OnExit 信号
        /// </summary>
        /// <param name="node">目标节点</param>
        /// <param name="sourceId">源节点 ID</param>
        void ProcessExitSignal(IStateNode node, string sourceId);
        
        /// <summary>
        /// 节点主动请求停止运行（从活跃列表移除）
        /// </summary>
        /// <param name="node">请求停止的节点</param>
        void RemoveActiveNode(IStateNode node);
        
        /// <summary>
        /// 节点主动请求开始运行（加入活跃列表）
        /// 罕见场景，一般由状态机在 ProcessEnterSignal 中自动处理
        /// </summary>
        /// <param name="node">请求运行的节点</param>
        void AddActiveNode(IStateNode node);
        
        /// <summary>
        /// 获取当前所有活跃节点（只读）
        /// </summary>
        IReadOnlyList<IStateNode> ActiveNodes { get; }
    }
}

using System;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态节点基类
    /// 继承自 Node Graph Processor 的 BaseNode，并实现 IStateNode 接口
    /// 所有状态节点都应继承此类
    /// 
    /// 控制流设计：
    /// - 输入：OnEnter（启动节点）、OnExit（强制停止节点）
    /// - 输出：Signal（单一输出信号）
    /// - 输出信号连接到目标节点的 OnEnter 或 OnExit，决定启动还是停止目标
    /// </summary>
    [Serializable]
    public abstract class BaseStateNode : BaseNode, IStateNode
    {
        #region 控制流端口
        
        [Input("OnEnter"), Vertical]
        [Tooltip("接收信号启动节点执行")]
        public ControlFlow inputEnter;
        
        [Input("OnExit"), Vertical]
        [Tooltip("接收信号强制停止节点")]
        public ControlFlow inputExit;
        
        [Output("Signal"), Vertical]
        [Tooltip("输出控制信号")]
        public ControlFlow outputSignal;
        
        #endregion
        
        #region IStateNode 接口实现
        
        public string NodeId => GUID;
        
        [NonSerialized]
        protected IStateMachine _stateMachine;
        public IStateMachine StateMachine => _stateMachine;
        
        public void InjectStateMachine(IStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        /// <summary>
        /// 接收到 OnEnter 信号时的处理逻辑
        /// 子类必须实现
        /// </summary>
        public abstract void OnEnterSignal(string sourceId);
        
        /// <summary>
        /// 接收到 OnExit 信号时的处理逻辑
        /// 子类必须实现
        /// </summary>
        public abstract void OnExitSignal(string sourceId);
        
        /// <summary>
        /// 每帧更新（仅当节点在活跃列表中）
        /// 子类可选择性重写
        /// </summary>
        public virtual void OnUpdate() { }
        
        /// <summary>
        /// 清理资源
        /// 子类可选择性重写
        /// </summary>
        public virtual void Cleanup() { }
        
        /// <summary>
        /// 节点主动停止运行
        /// 子类可重写以添加额外的清理逻辑
        /// </summary>
        public virtual void StopRunning()
        {
            _stateMachine?.RemoveActiveNode(this);
        }
        
        #endregion
        
        #region 信号触发方法
        
        /// <summary>
        /// 触发输出信号
        /// 遍历所有连接的边，根据目标节点的输入端口类型自动判断是启动还是停止目标节点
        /// </summary>
        protected void TriggerSignal()
        {
            TriggerSignalInternal();
        }
        
        /// <summary>
        /// 触发输出信号（公开版本，供 NGPStateMachine 调用）
        /// 用于脚本完成时由状态机触发节点的输出信号
        /// </summary>
        internal void TriggerSignalPublic()
        {
            TriggerSignalInternal();
        }
        
        /// <summary>
        /// 触发输出信号的内部实现
        /// </summary>
        private void TriggerSignalInternal()
        {
            // 获取输出信号端口
            var port = GetPort(nameof(outputSignal), null);
            if (port == null)
            {
                Debug.LogWarning($"[{name}] Port 'outputSignal' not found");
                return;
            }
            
            // 遍历连接的所有边
            var edges = port.GetEdges();
            foreach (var edge in edges)
            {
                var targetNode = edge.inputNode as BaseStateNode;
                if (targetNode == null)
                {
                    Debug.LogWarning($"[{name}] Target node is not a BaseStateNode");
                    continue;
                }
                
                // 关键：在目标节点接收信号之前，先让所有输出节点推送数据到目标的输入端口
                // 遍历目标节点的所有输入端口，从连接的输出节点拉取数据
                foreach (var inputPort in targetNode.inputPorts)
                {
                    var inputEdges = inputPort.GetEdges();
                    foreach (var inputEdge in inputEdges)
                    {
                        // 找到输出节点和对应的输出端口
                        var outputNode = inputEdge.outputNode;
                        var outputPort = inputEdge.outputPort;
                        
                        // 对于非状态节点（如 FloatNode），需要先调用 Process() 来准备数据
                        if (!(outputNode is BaseStateNode))
                        {
                            // 使用反射调用 Process 方法（如果存在）
                            var processMethod = outputNode.GetType().GetMethod("Process", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);
                            
                            if (processMethod != null)
                            {
                                try
                                {
                                    processMethod.Invoke(outputNode, null);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning($"[BaseStateNode] Failed to call Process() on {outputNode.name}: {e.Message}");
                                }
                            }
                        }
                        
                        // 让输出端口推送数据（这会触发 PushDataDelegate）
                        outputPort?.PushData();
                    }
                }
                
                // 根据目标输入端口类型判断信号类型
                string targetPortName = edge.inputPort.fieldName;
                
                if (targetPortName == nameof(inputEnter))
                {
                    // 连接到 OnEnter 输入 → 启动目标节点
                    _stateMachine?.ProcessEnterSignal(targetNode, this.GUID);
                }
                else if (targetPortName == nameof(inputExit))
                {
                    // 连接到 OnExit 输入 → 停止目标节点
                    _stateMachine?.ProcessExitSignal(targetNode, this.GUID);
                }
            }
        }
        
        #endregion
        
        #region BaseNode 重写
        
        public override string name => GetType().Name.Replace("Node", "");
        
        /// <summary>
        /// NGP 调用的初始化方法
        /// 我们在这里不做太多事情，主要初始化由 InjectStateMachine 完成
        /// </summary>
        protected override void Enable()
        {
            base.Enable();
            // 可以在这里添加节点特定的初始化逻辑
        }
        
        protected override void Disable()
        {
            base.Disable();
            Cleanup();
        }
        
        #endregion
    }
}

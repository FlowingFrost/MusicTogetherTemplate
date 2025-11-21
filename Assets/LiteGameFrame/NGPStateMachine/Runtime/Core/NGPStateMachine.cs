using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态机运行时管理器
    /// 负责执行状态机图的逻辑
    /// </summary>
    [AddComponentMenu("State Machine/NGP State Machine")]
    public class NGPStateMachine : MonoBehaviour, IStateMachine
    {
        #region 序列化字段
        
        [Header("Graph")]
        [SerializeField] 
        [Tooltip("状态机图资产")]
        private StateMachineGraph _stateGraph;
        
        [Header("Settings")]
        [SerializeField]
        [Tooltip("是否在 Awake 时自动启动")]
        private bool _autoStart = true;
        
        [SerializeField]
        [Tooltip("是否显示调试日志")]
        private bool _debugLog = false;
        
        [Header("Component Bindings")]
        [SerializeField]
        [Tooltip("Component 节点绑定列表（存储场景引用）")]
        private ComponentBinding[] _componentBindings = new ComponentBinding[0];
        
        #endregion
        
        #region 运行时数据
        
        // 活跃节点列表
        private List<IStateNode> _activeNodes = new List<IStateNode>();
        
        // 黑板数据
        private Dictionary<string, object> _blackboard = new Dictionary<string, object>();
        
        // 是否正在运行
        private bool _isRunning = false;
        
        #endregion
        
        #region 属性
        
        public StateMachineGraph StateGraph => _stateGraph;
        public IReadOnlyList<IStateNode> ActiveNodes => _activeNodes.AsReadOnly();
        public bool IsRunning => _isRunning;
        
        #endregion
        
        #region Unity 生命周期
        
        void Awake()
        {
            if (_stateGraph == null)
            {
                Debug.LogError($"[{gameObject.name}] StateMachineGraph is not assigned!");
                return;
            }
            
            BuildGraph();
            
            if (_autoStart)
            {
                StartStateMachine();
            }
        }
        
        void Update()
        {
            if (!_isRunning) return;
            
            // 遍历活跃节点并更新
            // 使用 ToList() 避免在遍历时修改集合
            foreach (var node in _activeNodes.ToList())
            {
                try
                {
                    node.OnUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{gameObject.name}] Error updating node {node.NodeId}: {e}");
                }
            }
        }
        
        void OnDestroy()
        {
            StopStateMachine();
        }
        
        #endregion
        
        #region 状态机控制
        
        /// <summary>
        /// 构建图结构
        /// 为所有状态节点注入状态机引用
        /// </summary>
        private void BuildGraph()
        {
            if (_stateGraph == null) return;
            
            // 设置图与状态机的双向关联
            _stateGraph.SetStateMachine(this);
            
            // 验证图结构
            if (!_stateGraph.ValidateGraph())
            {
                Debug.LogWarning($"[{gameObject.name}] Graph validation failed");
            }
            
            // 为所有 BaseStateNode 注入 StateMachine 引用
            foreach (var node in _stateGraph.nodes)
            {
                if (node is BaseStateNode stateNode)
                {
                    stateNode.InjectStateMachine(this);
                    
                    if (_debugLog)
                        Debug.Log($"[{gameObject.name}] Injected StateMachine into node: {stateNode.name} ({stateNode.NodeId})");
                }
            }
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] Graph built successfully. Total nodes: {_stateGraph.nodes.Count}");
        }
        
        /// <summary>
        /// 启动状态机
        /// </summary>
        public void StartStateMachine()
        {
            if (_isRunning)
            {
                Debug.LogWarning($"[{gameObject.name}] State machine is already running");
                return;
            }
            
            if (_stateGraph == null)
            {
                Debug.LogError($"[{gameObject.name}] Cannot start: StateGraph is null");
                return;
            }
            
            // 查找入口节点
            var entryNode = _stateGraph.FindEntryNode();
            if (entryNode == null)
            {
                Debug.LogError($"[{gameObject.name}] Cannot start: Entry node not found");
                return;
            }
            
            _isRunning = true;
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] State machine started. Entry node: {entryNode.name}");
            
            // 触发入口节点的 OnEnter 信号
            ProcessEnterSignal(entryNode, "Root");
        }
        
        /// <summary>
        /// 停止状态机
        /// </summary>
        public void StopStateMachine()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            
            // 清理所有活跃节点
            foreach (var node in _activeNodes.ToList())
            {
                try
                {
                    node.Cleanup();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{gameObject.name}] Error cleaning up node {node.NodeId}: {e}");
                }
            }
            
            _activeNodes.Clear();
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] State machine stopped");
        }
        
        /// <summary>
        /// 暂停状态机
        /// </summary>
        public void PauseStateMachine()
        {
            _isRunning = false;
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] State machine paused");
        }
        
        /// <summary>
        /// 恢复状态机
        /// </summary>
        public void ResumeStateMachine()
        {
            _isRunning = true;
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] State machine resumed");
        }
        
        #endregion
        
        #region IStateMachine 接口实现
        
        public void ProcessEnterSignal(IStateNode node, string sourceId)
        {
            if (node == null) return;
            
            // 如果节点不在活跃列表，添加进去
            if (!_activeNodes.Contains(node))
            {
                _activeNodes.Add(node);
                
                if (_debugLog)
                    Debug.Log($"[{gameObject.name}] Node activated: {node.NodeId} (from: {sourceId})");
            }
            
            // 触发节点的 OnEnter 处理
            try
            {
                node.OnEnterSignal(sourceId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{gameObject.name}] Error in OnEnterSignal for node {node.NodeId}: {e}");
            }
        }
        
        public void ProcessExitSignal(IStateNode node, string sourceId)
        {
            if (node == null) return;
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] Node received exit signal: {node.NodeId} (from: {sourceId})");
            
            // 触发节点的 OnExit 处理
            try
            {
                node.OnExitSignal(sourceId);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{gameObject.name}] Error in OnExitSignal for node {node.NodeId}: {e}");
            }
        }
        
        public void RemoveActiveNode(IStateNode node)
        {
            if (node == null) return;
            
            if (_activeNodes.Remove(node))
            {
                if (_debugLog)
                    Debug.Log($"[{gameObject.name}] Node deactivated: {node.NodeId}");
            }
        }
        
        public void AddActiveNode(IStateNode node)
        {
            if (node == null) return;
            
            if (!_activeNodes.Contains(node))
            {
                _activeNodes.Add(node);
                
                if (_debugLog)
                    Debug.Log($"[{gameObject.name}] Node manually activated: {node.NodeId}");
            }
        }
        
        #endregion
        
        #region 黑板操作
        
        public bool Get<T>(string key, out T value)
        {
            if (_blackboard.TryGetValue(key, out var obj) && obj is T typedValue)
            {
                value = typedValue;
                return true;
            }
            
            value = default;
            return false;
        }
        
        public bool Set<T>(string key, T value)
        {
            _blackboard[key] = value;
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] Blackboard set: {key} = {value}");
            
            return true;
        }
        
        public bool Find<T>(string key, out Type valueType)
        {
            if (_blackboard.TryGetValue(key, out var value) && value is T)
            {
                valueType = typeof(T);
                return true;
            }
            
            valueType = null;
            return false;
        }
        
        /// <summary>
        /// 清空黑板
        /// </summary>
        public void ClearBlackboard()
        {
            _blackboard.Clear();
            
            if (_debugLog)
                Debug.Log($"[{gameObject.name}] Blackboard cleared");
        }
        
        /// <summary>
        /// 获取黑板中所有键
        /// </summary>
        public IEnumerable<string> GetBlackboardKeys()
        {
            return _blackboard.Keys;
        }
        
        #endregion
        
        #region Component Binding 管理
        
        /// <summary>
        /// 获取节点绑定的 Component
        /// </summary>
        public T GetComponentBinding<T>(string nodeGUID, string fieldName = "") where T : UnityEngine.Object
        {
            foreach (var binding in _componentBindings)
            {
                if (binding.nodeGUID == nodeGUID && binding.fieldName == fieldName)
                {
                    return binding.target as T;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 设置节点绑定的 Component（编辑器调用）
        /// </summary>
        public void SetComponentBinding(string nodeGUID, string fieldName, UnityEngine.Object target)
        {
            // 查找是否已存在
            for (int i = 0; i < _componentBindings.Length; i++)
            {
                if (_componentBindings[i].nodeGUID == nodeGUID && _componentBindings[i].fieldName == fieldName)
                {
                    _componentBindings[i].target = target;
                    return;
                }
            }
            
            // 不存在则添加新绑定
            var newBinding = new ComponentBinding(nodeGUID, fieldName, target);
            var newBindings = new ComponentBinding[_componentBindings.Length + 1];
            _componentBindings.CopyTo(newBindings, 0);
            newBindings[_componentBindings.Length] = newBinding;
            _componentBindings = newBindings;
        }
        
        /// <summary>
        /// 移除节点的所有绑定（编辑器调用，节点删除时）
        /// </summary>
        public void RemoveComponentBindings(string nodeGUID)
        {
            var newBindings = new System.Collections.Generic.List<ComponentBinding>();
            foreach (var binding in _componentBindings)
            {
                if (binding.nodeGUID != nodeGUID)
                {
                    newBindings.Add(binding);
                }
            }
            _componentBindings = newBindings.ToArray();
        }
        
        /// <summary>
        /// 获取所有绑定（编辑器调用）
        /// </summary>
        public ComponentBinding[] GetAllComponentBindings()
        {
            return _componentBindings;
        }
        
        #endregion
        
        #region 调试信息
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            var info = $"State Machine: {gameObject.name}\n";
            info += $"Running: {_isRunning}\n";
            info += $"Active Nodes: {_activeNodes.Count}\n";
            
            foreach (var node in _activeNodes)
            {
                info += $"  - {node.NodeId}\n";
            }
            
            info += $"Blackboard Entries: {_blackboard.Count}\n";
            
            foreach (var kvp in _blackboard)
            {
                info += $"  - {kvp.Key}: {kvp.Value}\n";
            }
            
            return info;
        }
        
        #endregion
    }
}

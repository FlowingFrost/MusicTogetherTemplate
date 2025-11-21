using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 状态机图资产
    /// 继承自 Node Graph Processor 的 BaseGraph
    /// 用于序列化和保存状态机的图结构
    /// </summary>
    [CreateAssetMenu(fileName = "NewStateMachineGraph", menuName = "State Machine/State Machine Graph")]
    public class StateMachineGraph : BaseGraph
    {
        /// <summary>
        /// 入口节点的 GUID（可选，用于快速定位入口）
        /// </summary>
        [SerializeField]
        public string entryNodeGUID;
        
        /// <summary>
        /// 图的描述信息
        /// </summary>
        [TextArea(3, 5)]
        public string description;
        
        /// <summary>
        /// 图的版本号（用于迁移和兼容性）
        /// </summary>
        public int version = 1;
        
        /// <summary>
        /// 关联的状态机运行时实例（仅在运行时有效）
        /// </summary>
        [NonSerialized]
        private IStateMachine _stateMachineInstance;
        
        /// <summary>
        /// 设置关联的状态机实例
        /// </summary>
        public void SetStateMachine(IStateMachine stateMachine)
        {
            _stateMachineInstance = stateMachine;
        }
        
        /// <summary>
        /// 获取关联的状态机实例
        /// </summary>
        public IStateMachine GetStateMachine()
        {
            return _stateMachineInstance;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 可以在这里添加自定义的初始化逻辑
            // 例如：验证图结构、检查入口节点等
        }
        
        /// <summary>
        /// 查找入口节点
        /// </summary>
        /// <returns>入口节点，如果没有返回 null</returns>
        public BaseStateNode FindEntryNode()
        {
            // 先尝试通过 GUID 查找
            if (!string.IsNullOrEmpty(entryNodeGUID) && nodesPerGUID.TryGetValue(entryNodeGUID, out var node))
            {
                if (node is BaseStateNode stateNode)
                    return stateNode;
            }
            
            // 如果没有设置或找不到，查找第一个 EntryNode 类型的节点
            foreach (var n in nodes)
            {
                if (n != null && n.GetType().Name == "EntryNode")
                {
                    entryNodeGUID = n.GUID;  // 缓存起来
                    return n as BaseStateNode;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 验证图结构
        /// </summary>
        /// <returns>验证是否通过</returns>
        public bool ValidateGraph()
        {
            // 检查是否有入口节点
            var entryNode = FindEntryNode();
            if (entryNode == null)
            {
                Debug.LogWarning($"[{name}] No entry node found in the graph");
                return false;
            }
            
            // 检查是否有孤立节点（可选）
            // 检查是否有循环依赖（可选）
            
            return true;
        }
    }
}

using System;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// Component 绑定数据
    /// 用于将节点 GUID 与场景中的 Component 关联
    /// 类似 Timeline 的 ExposedReference 机制
    /// </summary>
    [Serializable]
    public class ComponentBinding
    {
        /// <summary>
        /// 节点的 GUID（作为绑定的 key）
        /// </summary>
        [SerializeField]
        public string nodeGUID;
        
        /// <summary>
        /// 绑定的字段名（用于支持一个节点绑定多个 Component）
        /// </summary>
        [SerializeField]
        public string fieldName;
        
        /// <summary>
        /// 绑定的 Component 引用
        /// </summary>
        [SerializeField]
        public UnityEngine.Object target;
        
        public ComponentBinding(string nodeGUID, string fieldName, UnityEngine.Object target)
        {
            this.nodeGUID = nodeGUID;
            this.fieldName = fieldName;
            this.target = target;
        }
    }
}


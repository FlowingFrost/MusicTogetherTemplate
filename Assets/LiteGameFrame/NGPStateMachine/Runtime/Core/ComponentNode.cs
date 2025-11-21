using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// Component 节点基类
    /// 用于绑定场景中的 Component，通过 NGPStateMachine 存储引用
    /// 类似 Timeline 的 PlayableAsset 机制
    /// </summary>
    [Serializable]
    public abstract class ComponentNode<T> : BaseStateNode where T : UnityEngine.Object
    {
        /// <summary>
        /// 绑定字段的名称（用于支持多个绑定）
        /// 默认为 "target"
        /// </summary>
        protected virtual string BindingFieldName => "target";
        
        /// <summary>
        /// 获取绑定的 Component
        /// </summary>
        protected T GetBoundComponent()
        {
            if (_stateMachine is NGPStateMachine stateMachine)
            {
                return stateMachine.GetComponentBinding<T>(GUID, BindingFieldName);
            }
            
            Debug.LogWarning($"[{name}] Cannot get bound component: StateMachine is not NGPStateMachine");
            return null;
        }
        
        /// <summary>
        /// 检查是否有绑定的 Component
        /// </summary>
        protected bool HasBoundComponent()
        {
            return GetBoundComponent() != null;
        }
        
        /// <summary>
        /// 获取绑定的 Component，如果没有则记录错误
        /// </summary>
        protected bool TryGetBoundComponent(out T component)
        {
            component = GetBoundComponent();
            
            if (component == null)
            {
                Debug.LogError($"[{name}] Component binding is missing! Please bind a {typeof(T).Name} in the Inspector.");
                return false;
            }
            
            return true;
        }
    }
}


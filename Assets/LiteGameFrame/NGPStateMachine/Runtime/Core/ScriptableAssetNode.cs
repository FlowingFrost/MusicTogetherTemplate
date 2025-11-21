using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// ScriptableObject Asset 节点基类
    /// 用于引用 ScriptableObject 资源
    /// 资源直接序列化在节点中（不需要额外的绑定机制）
    /// </summary>
    [Serializable]
    public abstract class ScriptableAssetNode<T> : BaseStateNode where T : ScriptableObject
    {
        [Input("Asset")]
        [Tooltip("引用的 ScriptableObject 资源")]
        public T asset;
        
        /// <summary>
        /// 获取资源
        /// </summary>
        protected T GetAsset()
        {
            return asset;
        }
        
        /// <summary>
        /// 检查是否有资源
        /// </summary>
        protected bool HasAsset()
        {
            return asset != null;
        }
        
        /// <summary>
        /// 获取资源，如果没有则记录错误
        /// </summary>
        protected bool TryGetAsset(out T assetOut)
        {
            assetOut = asset;
            
            if (assetOut == null)
            {
                Debug.LogError($"[{name}] Asset is missing! Please assign a {typeof(T).Name} asset.");
                return false;
            }
            
            return true;
        }
    }
}


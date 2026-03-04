using UnityEngine;

namespace LightGameFrame.Services
{
    /// <summary>
    /// 基于脚本的服务基类 (非静态单例)
    /// </summary>
    /// <typeparam name="T">具体的服务类型</typeparam>
    public abstract class ScriptServiceBase<T> : MonoBehaviour, IUpdateService where T : ScriptServiceBase<T>
    {
        public bool IsInitialized { get; private set; }
        public bool UpdateEnabled { get; set; } = true;

        /// <summary>
        /// 服务优先级，默认 100。
        /// 子类重写此属性以调整初始化顺序 (越小越先)
        /// </summary>
        public virtual int ServicePriority => 100;

        #region 生命周期 (由 ServiceManager 管理)

        public void Initialize()
        {
            if (IsInitialized) return;

            OnInitialize();
            IsInitialized = true;
            Debug.Log($"[ScriptService] {typeof(T).Name} <Priority:{ServicePriority}> Initialized");
        }

        public void OnUpdate(float deltaTime)
        {
            if (!IsInitialized || !UpdateEnabled) return;
            OnUpdate();
        }

        public void Cleanup()
        {
            if (!IsInitialized) return;

            OnCleanup();
            IsInitialized = false;
            Debug.Log($"[ScriptService] {typeof(T).Name} Cleaned up");
        }

        #endregion

        #region Unity 消息

        protected virtual void Awake()
        {
            // 自动注册：如果服务通过拖拽方式存在于场景中，或者被自动创建
            // 必须通知 ServiceManager 它的存在
            ServiceManager.RegisterService(this);
        }

        protected virtual void OnDestroy()
        {
            if (IsInitialized)
            {
                // 如果是被动销毁（如场景卸载），通知 Manager 注销
                // 注意：通常建议由 Manager 统一控制销毁，而不是直接 Destroy GameObject
                ServiceManager.UnregisterService(this);
            }
        }

        #endregion

        #region 虚方法

        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnCleanup() { }

        #endregion
        
        public string GetStatusInfo()
        {
            return $"[{typeof(T).Name}] Priority:{ServicePriority}, Init:{IsInitialized}, Active:{UpdateEnabled}";
        }
    }
}
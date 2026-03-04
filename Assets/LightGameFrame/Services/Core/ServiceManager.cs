using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.Linq;

namespace LightGameFrame.Services
{
    /// <summary>
    /// 服务管理器 - 管理服务的注册、生命周期和更新
    /// </summary>
    [DefaultExecutionOrder(-1000)] // 确保比大多数脚本先执行
    public class ServiceManager : MonoBehaviour
    {
        private static ServiceManager _instance;
        private static readonly object _lock = new object();

        [Header("Service Control")]
        [SerializeField] private bool autoInitializeOnStart = true;
        [SerializeField] private bool enableServiceUpdates = true;

        /// <summary>
        /// 全局服务列表
        /// </summary>
        private readonly List<IGameService> _services = new List<IGameService>();
        
        /// <summary>
        /// 待注册服务队列 (缓冲区)
        /// </summary>
        private readonly List<IGameService> _pendingServices = new List<IGameService>();

        /// <summary>
        /// 需要更新的服务缓存列表（优化性能）
        /// </summary>
        private readonly List<IUpdateService> _updatableServices = new List<IUpdateService>();

        private bool _isInitialized = false;

        /// <summary>
        /// 服务注册完成事件 - 当服务被注册并初始化后触发
        /// </summary>
        public static System.Action<IGameService> OnServiceRegistered;

        /// <summary>
        /// 服务注销事件 - 当服务被注销时触发
        /// </summary>
        public static System.Action<IGameService> OnServiceUnregistered;

        /// <summary>
        /// 获取服务管理器实例
        /// </summary>
        public static ServiceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<ServiceManager>();
                            
                            if (_instance == null)
                            {
                                GameObject managerObj = new GameObject("[ServiceManager]");
                                _instance = managerObj.AddComponent<ServiceManager>();
                                DontDestroyOnLoad(managerObj);
                                Debug.Log("[ServiceManager] Created singleton instance");
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        #region Unity 生命周期

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                gameObject.name = "[ServiceManager]";
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }

            // 扫描并创建自动服务
            if (autoInitializeOnStart)
            {
                CreateAutoServices();
            }
        }

        private void Update()
        {
            // 1. 处理待注册队列
            ProcessPendingServices();

            // 2. 执行服务更新
            if (!enableServiceUpdates) return;

            // Profiler.BeginSample("ServiceManager.UpdateAll");
            
            for (var i = 0; i < _updatableServices.Count; i++)
            {
                var service = _updatableServices[i];
                if (service != null && service.UpdateEnabled && service.IsInitialized)
                {
                    service.OnUpdate(Time.deltaTime);
                }
            }

            // Profiler.EndSample();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                CleanupAllServices();
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            CleanupAllServices();
        }

        #endregion

        #region 自动扫描逻辑

        /// <summary>
        /// 扫描程序集并创建带有 [AutoService] 特性的服务
        /// </summary>
        private void CreateAutoServices()
        {
            Debug.Log("[ServiceManager] Scanning for [AutoService]...");
            
            // 获取当前域中所有程序集 (通常我们只关心 Assembly-CSharp)
            // 也可以扩展遍历 AppDomain.CurrentDomain.GetAssemblies()
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IGameService).IsAssignableFrom(t))
                .Where(t => t.IsDefined(typeof(AutoServiceAttribute), false));

            int createCount = 0;
            foreach (var type in serviceTypes)
            {
                var attr = (AutoServiceAttribute)type.GetCustomAttributes(typeof(AutoServiceAttribute), false)[0];
                
                // 检查场景中是否已经存在
                // 注意：这里使用 FindFirstObjectByType 兼容旧版 FindObjectOfType
                var existing = FindObjectsOfType(type);
                
                if (existing.Length > 0 && !attr.ForceCreate)
                {
                    Debug.Log($"[ServiceManager] AutoService {type.Name} already exists in scene. Skipping creation.");
                    continue;
                }

                // 创建新服务
                GameObject serviceObj = new GameObject(type.Name);
                var serviceInstance = serviceObj.AddComponent(type) as IGameService;
                DontDestroyOnLoad(serviceObj);
                
                // 被标记为自动注册的服务理应不会主动注册，这里显式调用 RegisterService 也不会有副作用（会被排重）
                // 主要是确保即刻进入 Pending 列表
                RegisterService(serviceInstance);
                
                createCount++;
                Debug.Log($"[ServiceManager] Auto-Created service: {type.Name}");
            }
            
            if (createCount > 0)
            {
                Debug.Log($"[ServiceManager] Auto-created {createCount} services");
            }
        }

        #endregion

        #region 内部逻辑

        /// <summary>
        /// 处理待注册的服务队列
        /// </summary>
        private void ProcessPendingServices()
        {
            if (_pendingServices.Count == 0) return;

            // 按优先级排序：Level 越小越先处理
            _pendingServices.Sort((a, b) => a.ServicePriority.CompareTo(b.ServicePriority));

            // Profiler.BeginSample("ServiceManager.ProcessPending");

            // 复制一份当前帧需要处理的服务，防止处理过程中可能产生的二次添加
            var servicesToInit = _pendingServices.ToArray();
            _pendingServices.Clear();

            foreach (var service in servicesToInit)
            {
                if (_services.Contains(service)) continue;
                
                // 正式加入活跃列表
                _services.Add(service);
                
                if (service is IUpdateService updateService)
                {
                    _updatableServices.Add(updateService);
                }

                // 初始化
                if (!service.IsInitialized)
                {
                    try
                    {
                        service.Initialize();
                        
                        // 服务初始化完成后触发事件
                        OnServiceRegistered?.Invoke(service);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[ServiceManager] Failed to initialize {service.GetType().Name}: {ex}");
                    }
                }
                else
                {
                    // 如果服务已经初始化，也触发事件
                    OnServiceRegistered?.Invoke(service);
                }
            }

            // Profiler.EndSample();
        }

        #endregion

        #region 服务注册管理

        /// <summary>
        /// 注册服务 (进入等待队列，下一帧 Update 统一排序并初始化)
        /// </summary>
        public static void RegisterService(IGameService service)
        {
            if (service == null) return;
            
            // 防止重复添加
            if (Instance._services.Contains(service)) return;
            if (Instance._pendingServices.Contains(service)) return;

            Instance._pendingServices.Add(service);
            
            // 如果是 MonoBehaviour，挂载到节点下保持层级整洁
            if (service is MonoBehaviour mb)
            {
                mb.transform.SetParent(Instance.transform);
            }

            Debug.Log($"[ServiceManager] Pending Register: {service.GetType().Name} (Priority: {service.ServicePriority})");
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        public static void UnregisterService(IGameService service)
        {
            if (_instance == null || service == null) return;

            if (_instance._pendingServices.Contains(service))
            {
                _instance._pendingServices.Remove(service);
            }

            if (_instance._services.Contains(service))
            {
                _instance._services.Remove(service);
                if (service is IUpdateService updateService)
                {
                    _instance._updatableServices.Remove(updateService);
                }
                
                // 如果已初始化，执行清理
                if (service.IsInitialized)
                {
                    service.Cleanup();
                }

                // 触发服务注销事件
                OnServiceUnregistered?.Invoke(service);

                Debug.Log($"[ServiceManager] Unregistered: {service.GetType().Name}");
            }
        }

        public void InitializeAllServices()
        {
            // 现在的逻辑改为：所有初始化都通过 Pending 队列在 Update 中自动处理
            // 此方法主要用于手动触发一次立即处理（如果不希望等待下一帧）
            ProcessPendingServices();
            _isInitialized = true;
        }

        public void CleanupAllServices()
        {
            Debug.Log("[ServiceManager] Cleanup all services...");
            
            // 优先清理待定列表
            _pendingServices.Clear();

            // 反向清理活跃列表
            for (int i = _services.Count - 1; i >= 0; i--)
            {
                var service = _services[i];
                if (service is IUpdateService updateService)
                {
                    _updatableServices.Remove(updateService);
                }
                
                // 如果已初始化，执行清理
                if (service.IsInitialized)
                {
                    service.Cleanup();
                }

                _services.RemoveAt(i);
            }

            _isInitialized = false;
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 获取或创建服务 (懒加载)
        /// </summary>
        /// <typeparam name="T">具体的服务类型</typeparam>
        public static T GetOrCreateService<T>() where T : ScriptServiceBase<T>
        {
            var instance = Instance;
            if (instance == null) return null;

            // 1. 尝试查找已注册的服务 (活跃列表)
            var existing = GetService<T>();
            if (existing != null) return existing;

            // 2. 尝试查找 Pending 列表 (等待初始化)
            foreach (var s in instance._pendingServices)
            {
                if (s is T target) return target;
            }

            // 3. 尝试在场景中查找 (可能没注册)
            var sceneObj = FindObjectOfType<T>();
            if (sceneObj != null)
            {
                // 找到但未注册，补注册
                RegisterService(sceneObj);
                return sceneObj;
            }

            // 4. 创建新实例
            GameObject serviceObj = new GameObject(typeof(T).Name);
            T newService = serviceObj.AddComponent<T>();
            DontDestroyOnLoad(serviceObj); 
            
            // 注册 (加入 Pending 队列，等待 Update 初始化)
            RegisterService(newService);

            Debug.Log($"[ServiceManager] Lazy created service: {typeof(T).Name}");
            return newService;
        }

        /// <summary>
        /// 获取已注册的服务 (仅限活跃列表)
        /// </summary>
        /// <typeparam name="T">服务接口或类型</typeparam>
        public static T GetService<T>() where T : class, IGameService
        {
            if (_instance == null) return null;
            
            // 简单遍历查找
            foreach (var service in _instance._services)
            {
                if (service is T typedService)
                    return typedService;
            }
            return null;
        }

        #endregion

        #region 调试辅助

        /// <summary>
        /// 获取所有服务的状态信息
        /// </summary>
        public string GetAllServicesStatus()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- Service Status (Active: {_services.Count}, Pending: {_pendingServices.Count}) ---");
            
            foreach (var service in _services)
            {
                // 尝试反射获取 GetStatusInfo
                var method = service.GetType().GetMethod("GetStatusInfo");
                if (method != null)
                    sb.AppendLine(method.Invoke(service, null)?.ToString());
                else
                    sb.AppendLine($"[{service.GetType().Name}] Init:{service.IsInitialized}");
            }
            return sb.ToString();
        }

        #endregion
    }
}
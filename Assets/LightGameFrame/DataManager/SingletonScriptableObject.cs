using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LightGameFrame.DataManager
{
    /// <summary>
    /// 修复版单例ScriptableObject基类
    /// 解决了原版本的所有主要问题
    /// </summary>
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _instance;
        private static string TName => typeof(T).Name;
        private static bool _initialized = false;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!_initialized)
                {
                    Initialize();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 静态方法获取资源路径
        /// </summary>
        private static string GetResourcePathStatic()
        {
            // 使用反射或直接返回命名空间+类名作为默认路径
            return $"Data/{TName}";
        }

        /// <summary>
        /// 静态方法获取JSON文件名
        /// </summary>
        private static string GetJsonFileNameStatic()
        {
            return $"{TName}.json";
        }
        /// <summary>
        /// 获取JSON保存路径（使用可写的持久化数据路径）
        /// </summary>
        private static string GetJsonSavePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        /// <summary>
        /// 获取JSON读取路径（优先从持久化路径读取，不存在则从StreamingAssets读取）
        /// </summary>
        private static string GetJsonLoadPath(string fileName)
        {
            string persistentPath = GetJsonSavePath(fileName);
            if (File.Exists(persistentPath))
            {
                return persistentPath;
            }

            return Path.Combine(Application.streamingAssetsPath, fileName);
        }

        /// <summary>
        /// 初始化单例
        /// </summary>
        private static void Initialize()
        {
            if (_initialized) return;

            // 直接使用静态属性获取路径信息，避免创建临时实例
            string resourcePath = GetResourcePathStatic();
            string jsonFileName = GetJsonFileNameStatic();

            // 从Resources加载原始资源（作为默认配置模板）
            T resourceData = Resources.Load<T>(resourcePath);
            
            // 统一创建运行时实例的方式
            if (resourceData != null)
            {
                // 从Resources模板创建副本
                _instance = Instantiate(resourceData);
                Debug.Log($"[{TName}] 从Resources创建实例: {resourcePath}");
            }
            else
            {
                // 创建默认实例
                _instance = CreateInstance<T>();
                Debug.LogWarning($"[{TName}] Resources未找到，创建默认实例: {resourcePath}");
            }

            // 从JSON更新数据（无论哪种方式创建的实例）
            LoadFromJson(_instance, jsonFileName);

            _initialized = true;
            Debug.Log($"[{TName}] 单例初始化完成");
        }
        
        /// <summary>
        /// 从JSON文件加载并更新数据
        /// </summary>
        private static void LoadFromJson(T instance, string jsonFileName)
        {
            if (string.IsNullOrEmpty(jsonFileName)) return;

            string jsonPath = GetJsonLoadPath(jsonFileName);
            if (!File.Exists(jsonPath))
            {
                Debug.Log($"[{TName}] JSON文件不存在，使用默认配置: {jsonPath}");
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(jsonPath);
                JsonUtility.FromJsonOverwrite(jsonContent, instance);
                Debug.Log($"[{TName}] JSON配置已应用: {jsonFileName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{TName}] JSON加载失败: {e.Message}");
            }
        }

        /// <summary>
        /// 保存当前配置到JSON文件（保存到可写的持久化目录）
        /// </summary>
        public static void SaveToJson()
        {
            if (_instance == null)
            {
                Debug.LogError($"[{TName}] 实例未初始化，无法保存");
                return;
            }

            try
            {
                string jsonFileName = GetJsonFileNameStatic();
                string jsonPath = GetJsonSavePath(jsonFileName);
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(jsonPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string jsonContent = JsonUtility.ToJson(_instance, true);
                File.WriteAllText(jsonPath, jsonContent);
                Debug.Log($"[{TName}] 配置已保存: {jsonPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{TName}] 保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 重置为Resources中的默认配置
        /// </summary>
        public static void ResetToDefault()
        {
            if (_instance != null)
            {
                // 在运行时使用安全的销毁方法
                if (Application.isPlaying)
                {
                    Destroy(_instance);
                }
                else
                {
                    DestroyImmediate(_instance);
                }
                _instance = null;
            }
            _initialized = false;
            
            // 重新初始化
            var temp = Instance; // 触发重新初始化
            Debug.Log($"[{TName}] 已重置为默认配置");
        }

        /// <summary>
        /// 删除持久化的JSON文件，下次启动将使用默认配置
        /// </summary>
        public static void DeletePersistedData()
        {
            try
            {
                string jsonFileName = GetJsonFileNameStatic();
                string jsonPath = GetJsonSavePath(jsonFileName);
                if (File.Exists(jsonPath))
                {
                    File.Delete(jsonPath);
                    Debug.Log($"[{TName}] 持久化数据已删除: {jsonPath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{TName}] 删除持久化数据失败: {e.Message}");
            }
        }

        /// <summary>
        /// Editor 中修改原始 .asset 字段时自动触发，将新值同步到运行时副本。
        /// 仅在编辑器运行模式下、且单例已初始化、且当前对象不是副本本身时生效。
        /// </summary>
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!_initialized || _instance == null) return;
            if (this == _instance) return;

            string json = JsonUtility.ToJson(this);
            JsonUtility.FromJsonOverwrite(json, _instance);
            Debug.Log($"[{TName}] Editor 修改已同步到运行时副本");
#endif
        }

#if UNITY_EDITOR
        [Button("同步到运行时副本", ButtonSizes.Large), PropertyOrder(-1)]
        [InfoBox("将此 .asset 的当前值覆写到内存中的单例实例。", InfoMessageType.Info)]
        private void SyncToRuntimeInstance()
        {
            if (!_initialized || _instance == null)
            {
                Debug.LogWarning($"[{TName}] 单例尚未初始化");
                return;
            }
            if (this == _instance)
            {
                Debug.LogWarning($"[{TName}] 当前查看的已是运行时副本，无需同步");
                return;
            }

            string json = JsonUtility.ToJson(this);
            JsonUtility.FromJsonOverwrite(json, _instance);
            Debug.Log($"[{TName}] ✅ 已同步到运行时副本");
        }
#endif

        /// <summary>
        /// 检查单例是否已初始化
        /// </summary>
        public static bool IsInitialized => _initialized && _instance != null;

        /// <summary>
        /// 手动初始化（可选调用）
        /// </summary>
        public static void EnsureInitialized()
        {
            var temp = Instance; // 触发初始化
        }

        /// <summary>
        /// 获取JSON文件的完整读取路径（用于调试）
        /// </summary>
        public static string GetCurrentJsonPath()
        {
            return GetJsonLoadPath(GetJsonFileNameStatic());
        }

        /// <summary>
        /// 获取JSON文件的保存路径（用于调试）
        /// </summary>
        public static string GetJsonSaveLocation()
        {
            return GetJsonSavePath(GetJsonFileNameStatic());
        }
    }
}
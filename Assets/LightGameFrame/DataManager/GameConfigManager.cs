using UnityEngine;

namespace LightGameFrame.DataManager
{
    /// <summary>
    /// 游戏配置ScriptableObject - 单例版本
    /// 支持跨场景访问，自动初始化，无需手动添加到场景
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "DataManager/Game Config")]
    public class GameConfig : SingletonScriptableObject<GameConfig>
    {
        [Header("游戏设置")]
        public string gameName = "My Unity Game";
        public string version = "1.0.0";
        public bool enableDebugMode = false;

        [Header("音频设置")]
        [Range(0f, 1f)]
        public float masterVolume = 1.0f;
        [Range(0f, 1f)]
        public float musicVolume = 0.8f;
        [Range(0f, 1f)]
        public float sfxVolume = 1.0f;

        [Header("图形设置")]
        public int targetFrameRate = 60;
        public bool enableVSync = true;
        public int qualityLevel = 2;

        [Header("用户界面")]
        public string defaultLanguage = "zh-CN";
        public bool showFPS = false;
        public Color uiThemeColor = Color.blue;

    [Header("Markdown 转换设置")]
    [Tooltip("#~###### 标题等级对应的字号，从1级到6级。长度不足6时使用最后一个值填充。")]
    public int[] markdownHeaderSizes = new int[] { 24, 20, 18, 16, 14, 12 };

        #region 单例ScriptableObject实现
        /// <summary>
        /// 静态访问当前配置实例
        /// </summary>
        public static GameConfig Config => Instance;

        void OnEnable()
        {
            // 应用游戏设置
            if (Application.isPlaying && IsInitialized)
            {
                ApplyGameSettings();
            }
        }

        /// <summary>
        /// 应用游戏设置到Unity
        /// </summary>
        public static void ApplyGameSettings()
        {
            if (Instance == null) return;

            Application.targetFrameRate = Instance.targetFrameRate;
            QualitySettings.vSyncCount = Instance.enableVSync ? 1 : 0;
            QualitySettings.SetQualityLevel(Instance.qualityLevel);

            Debug.Log($"游戏设置已应用 - FPS: {Instance.targetFrameRate}, 质量: {Instance.qualityLevel}");
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public static void SetVolume(float master, float music, float sfx)
        {
            if (Instance != null)
            {
                Instance.masterVolume = Mathf.Clamp01(master);
                Instance.musicVolume = Mathf.Clamp01(music);
                Instance.sfxVolume = Mathf.Clamp01(sfx);
            }
        }

        /// <summary>
        /// 切换调试模式
        /// </summary>
        public static void ToggleDebugMode()
        {
            if (Instance != null)
            {
                Instance.enableDebugMode = !Instance.enableDebugMode;
                Debug.Log($"调试模式: {(Instance.enableDebugMode ? "开启" : "关闭")}");
            }
        }

        /// <summary>
        /// 获取配置摘要
        /// </summary>
        public static string GetConfigSummary()
        {
            if (Instance == null) return "配置未初始化";
            
            return $"游戏配置: {Instance.gameName} v{Instance.version}, " +
                   $"调试模式: {(Instance.enableDebugMode ? "开启" : "关闭")}";
        }

        #endregion
    }
}
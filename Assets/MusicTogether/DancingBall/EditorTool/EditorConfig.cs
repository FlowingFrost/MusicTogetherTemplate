using LightGameFrame.DataManager;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// UI全局配置 - 单例ScriptableObject
    /// 存储WindowChrome等UI组件的全局参数
    /// </summary>
    [CreateAssetMenu(fileName = "EditorConfig", menuName = "MusicTogether/DB_Editor Config")]
    public class EditorConfig : SingletonScriptableObject<EditorConfig>
    {
        //DebugColors
        [Header("Debug Colors")]
        public Color normalBlockColor = new Color(1f, 1f, 1f, 0.3f);
        public Color problemBlockColor = new Color(1f, 0.5f, 0.5f, 0.3f);
        public Color tapBlockWithDisplacementColor = new Color(0.1f, 0.9f, 0.2f, 0.3f);
        public Color tapBlockWithoutDisplacementColor = new Color(1.0f, 0.85f, 0.2f, 0.3f);
        public Color normalBlockWithDisplacementColor = new Color(0.2f, 0.8f, 1.0f, 0.3f);
        public Color specialEventBlockColor = new Color(0.5f, 0.5f, 1.0f, 0.3f);

        /// <summary>
        /// 静态访问当前配置实例
        /// </summary>
        public static EditorConfig Config => Instance;
    }
}
using LightGameFrame.DataManager;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightGameFrame.UI
{
    [CreateAssetMenu(fileName = "UISettings", menuName = "LightGameFrame/UI Settings")]
    public class UISettings : SingletonScriptableObject<UISettings>
    {
        public bool useDarkTheme = false;
        [Header("Theme Stylesheets")]
        [Tooltip("通用基础样式表")]
        public StyleSheet commonStyleSheet;

        [Tooltip("暗黑主题样式表")]
        public StyleSheet darkThemeStyleSheet;

        public static UISettings Settings => Instance;
    }
}

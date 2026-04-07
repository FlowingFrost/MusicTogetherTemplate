using LightGameFrame.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.UIManager
{
    public abstract class UIManagerBase
    {
        protected readonly VisualElement Root;

        protected UIManagerBase(VisualElement root)
        {
            Root = root;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var settings = UISettings.Settings;
            if (settings == null) return;

            // 应用基础样式
            if (settings.commonStyleSheet != null && !Root.styleSheets.Contains(settings.commonStyleSheet))
            {
                Root.styleSheets.Add(settings.commonStyleSheet);
            }

            // 根据当前主题，如果是暗色主题则应用暗黑样式表
            if (UISettings.Settings.useDarkTheme)
            {
                if (!Root.styleSheets.Contains(settings.darkThemeStyleSheet))
                {
                    Root.styleSheets.Add(settings.darkThemeStyleSheet);
                }
            }
        }
    }
}

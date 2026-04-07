using MusicTogether.DancingBall.EditorTool.UIManager;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.Editor
{
    [Overlay(typeof(SceneView), "Displacement Debug", defaultDisplay: true)]
    public class DisplacementOverlay : Overlay
    {
        private const string DisplacementUxmlPath = "Assets/MusicTogether/DancingBall/UI/DisplacementOverlay.uxml";
        private EditorCenter EditorCenter => EditorCenter.Instance;
        private DisplacementOverlayManager _overlayManager;
        private VisualElement _root;
        private bool _toolEnabled = true;

        private EditorApplication.CallbackFunction _updateCallback;

        public override void OnCreated()
        {
            base.OnCreated();
            _updateCallback = RefreshUI;
            EditorApplication.update += _updateCallback;
        }

        public override void OnWillBeDestroyed()
        {
            if (_updateCallback != null)
            {
                EditorApplication.update -= _updateCallback;
                _updateCallback = null;
            }
            base.OnWillBeDestroyed();
        }

        public override VisualElement CreatePanelContent()
        {
            _root = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(DisplacementUxmlPath);
            if (visualTree == null)
            {
                _root.Add(new Label($"UXML not found: {DisplacementUxmlPath}"));
                return _root;
            }

            visualTree.CloneTree(_root);
            _overlayManager = new DisplacementOverlayManager(_root);
            _overlayManager.EnableChanged = enabled => _toolEnabled = enabled;
            _overlayManager.RetryBind = BindEditorCenter;
            BindEditorCenter();

            return _root;
        }

        private void BindEditorCenter()
        {
            if (!ValidateEditorCenter()) return;
            _overlayManager?.SetBindedViewVisible(true);
            _overlayManager?.SetEnabledState(_toolEnabled);
        }

        private bool ValidateEditorCenter()
        {
            if (EditorCenter == null || EditorCenter.player == null)
            {
                _overlayManager?.SetBindedViewVisible(false);
                return false;
            }
            return true;
        }

        private void RefreshUI()
        {
            if (_overlayManager == null)
                return;

            if (_root?.panel == null)
            {
                if (_updateCallback != null)
                {
                    EditorApplication.update -= _updateCallback;
                    _updateCallback = null;
                }
                return;
            }

            if (!ValidateEditorCenter())
                return;

            if (!_toolEnabled)
            {
                _overlayManager.SetEnabledState(false);
                _overlayManager.ClearData("已禁用");
                return;
            }

            _overlayManager.SetEnabledState(true);
            _overlayManager.UpdateDebugData(EditorCenter.player);
        }
    }
}

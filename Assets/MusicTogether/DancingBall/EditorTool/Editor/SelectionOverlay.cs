using System.Linq;
using MusicTogether.DancingBall.EditorTool.UIManager;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;


namespace MusicTogether.DancingBall.EditorTool.Editor
{
    [Overlay(typeof(SceneView), "Block Editor", defaultDisplay: true)]
    public class SelectionOverlay : Overlay
    {
        private const string SelectionUxmlPath = "Assets/MusicTogether/DancingBall/UI/SelectionWindow.uxml";
        private EditorCenter EditorCenter => EditorCenter.Instance;
        private SelectionWindowManager _selectionWindowManager;
        
        private bool _toolEnabled = true;
        private int _controlId = -1;

        private EditorApplication.CallbackFunction _updateCallback;

        //生命周期绑定函数
        public override void OnCreated()
        {
            base.OnCreated();
            SceneView.duringSceneGui += OnSceneGUI;
        }
        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (_updateCallback != null)
            {
                EditorApplication.update -= _updateCallback;
                _updateCallback = null;
            }
            base.OnWillBeDestroyed();
        }

        //UI构建
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SelectionUxmlPath);
            if (visualTree == null)
            {
                root.Add(new Label($"UXML not found: {SelectionUxmlPath}"));
                return root;
            }

            visualTree.CloneTree(root);
            _selectionWindowManager = new SelectionWindowManager(root);
            _selectionWindowManager.EnableChanged = enabled => _toolEnabled = enabled;
            _selectionWindowManager.RetryBind = BindEditorCenter;
            BindEditorCenter();
            
            _updateCallback = () => RefreshUI(root);
            EditorApplication.update += _updateCallback;

            return root;
        }
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_toolEnabled) return;

            if (Event.current.type == EventType.Layout)
            {
                _controlId = GUIUtility.GetControlID(FocusType.Keyboard);
                HandleUtility.AddDefaultControl(_controlId);
            }

            Event e = Event.current;
            if (e == null || e.type != EventType.KeyDown) return;

            if (!ValidateEditorCenter()) return;
            if (e.keyCode == KeyCode.LeftArrow)
            {
                EditorCenter.PreviousBlock();
                e.Use();
            }
            else if (e.keyCode == KeyCode.RightArrow)
            {
                EditorCenter.NextBlock();
                e.Use();
            }

        }
        private void RefreshUI(VisualElement root)
        {
            if (root.panel == null)
            {
                if (_updateCallback != null)
                {
                    EditorApplication.update -= _updateCallback;
                    _updateCallback = null;
                }
                return;
            }

            string hint = _toolEnabled ? "← / → 切换" : "已禁用";
            _selectionWindowManager?.SetHint(hint);
            _selectionWindowManager?.SetEnabledState(_toolEnabled);
        }

        //
        private void BindEditorCenter()
        {
            if (!ValidateEditorCenter()) return;
            
            _selectionWindowManager.JumpTo = (roadIndex, blockIndex) => EditorCenter.JumpTo(roadIndex, blockIndex);
            _selectionWindowManager.SetEnabledState(_toolEnabled);
            EditorCenter.OnSelectionChanged += _selectionWindowManager.UpdateSelectionInfo;
            EditorCenter.LookAtObject += LookAt;

            _selectionWindowManager.SetBindedViewVisible(true);
            _selectionWindowManager.SetEnabledState(true);
        }
        private bool ValidateEditorCenter()
        {
            if (EditorCenter == null)
            {
                _selectionWindowManager.SetBindedViewVisible(false);
                return false;
            }
            return true;
        }
        public void LookAt(GameObject go)
        {
            if (go == null) return;
            Selection.activeGameObject = go;
            SceneView.lastActiveSceneView?.FrameSelected();
        }
    }
}

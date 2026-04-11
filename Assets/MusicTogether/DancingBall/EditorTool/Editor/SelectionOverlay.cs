using System;
using System.Linq;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.EditorTool.UIManager;
using MusicTogether.DancingBall.Scene;
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
    private ClassicBlockDisplacementUIManager _classicDisplacementManager;
    private BlockDisplacementDataType _defaultDisplacementType = BlockDisplacementDataType.Classic;
    private IBlock _currentBlock;
    private IBlockDisplacementData _currentDisplacementData;
        
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
            _selectionWindowManager.DefaultDisplacementTypeChanged = OnDefaultDisplacementTypeChanged;

            var classicRoot = root.Q<VisualElement>("classic-root");
            if (classicRoot != null)
            {
                _classicDisplacementManager = new ClassicBlockDisplacementUIManager(classicRoot);
                _classicDisplacementManager.DataChanged = OnDisplacementDataChanged;
            }
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
            EditorCenter.OnBlockSelectionChanged += OnBlockSelectionChanged;
            EditorCenter.LookAtObject += LookAt;

            _selectionWindowManager.SetBindedViewVisible(true);
            _selectionWindowManager.SetEnabledState(true);
            _selectionWindowManager.SetDefaultDisplacementType(_defaultDisplacementType);
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
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            if (TryGetExpandedBounds(go, 3.0f, out var bounds))
            {
                sceneView.Frame(bounds, false);
            }
            else
            {
                sceneView.FrameSelected();
            }
        }

        private bool TryGetExpandedBounds(GameObject target, float expandMultiplier, out Bounds bounds)
        {
            bounds = new Bounds();
            if (target == null) return false;

            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0) return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (expandMultiplier > 1f)
            {
                bounds.Expand(bounds.size * (expandMultiplier - 1f));
            }
            return true;
        }

        private void OnBlockSelectionChanged(IBlock block, IBlockDisplacementData data)
        {
            _currentBlock = block;
            _currentDisplacementData = data;
            RefreshDisplacementPanel();
        }

        private void RefreshDisplacementPanel()
        {
            if (_classicDisplacementManager == null) return;

            if (_currentBlock == null)
            {
                _classicDisplacementManager.SetData(null);
                return;
            }

            if (_currentDisplacementData is ClassicBlockDisplacementData classicData)
            {
                _classicDisplacementManager.SetData(classicData);
                return;
            }

            if (_currentDisplacementData == null)
            {
                _classicDisplacementManager.SetData(CreateDefaultDisplacementData(_currentBlock.BlockLocalIndex) as ClassicBlockDisplacementData);
                return;
            }

            _classicDisplacementManager.SetData(null);
        }

        private IBlockDisplacementData CreateDefaultDisplacementData(int blockLocalIndex)
        {
            return _defaultDisplacementType switch
            {
                BlockDisplacementDataType.Classic => new ClassicBlockDisplacementData(blockLocalIndex),
                _ => new ClassicBlockDisplacementData(blockLocalIndex)
            };
        }

        private void OnDefaultDisplacementTypeChanged(Enum value)
        {
            if (value is BlockDisplacementDataType type)
            {
                _defaultDisplacementType = type;
                if (_currentDisplacementData == null)
                {
                    RefreshDisplacementPanel();
                }
            }
        }

        private void OnDisplacementDataChanged(IBlockDisplacementData data)
        {
            if (data == null || EditorCenter?.selectedRoad == null) return;
            EditorCenter.selectedRoad.ModifyDisplacementData(data.BlockIndex_Local, data);
            EditorCenter.RefreshSelection();
        }
    }
}

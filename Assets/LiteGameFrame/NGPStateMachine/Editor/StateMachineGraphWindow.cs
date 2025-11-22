using UnityEditor;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// StateMachineGraph 编辑器窗口
    /// 类似 Timeline/Animation 窗口，选中带有 NGPStateMachine 组件的物体时自动加载
    /// 支持窗口锁定功能，锁定后不会随选择改变而更新
    /// </summary>
    public class StateMachineGraphWindow : BaseGraphWindow, IHasCustomMenu
    {
        private NGPStateMachine _currentStateMachine;
        private GameObject _lastSelectedGameObject;
        private bool _isLocked;
        
        // 反射相关字段，用于访问 EditorWindow 的内部锁定状态
        private PropertyInfo _isLockedProperty;
        private const string LOCKED_PROPERTY_NAME = "isLocked";
        
        [MenuItem("Window/State Machine Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<StateMachineGraphWindow>("State Machine Graph");
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 初始化反射属性（用于访问 EditorWindow 的内部 isLocked 状态）
            InitializeReflection();
            
            // 同步内部锁定状态
            SyncLockState();
            
            // 订阅选中变化事件
            Selection.selectionChanged += OnSelectionChanged;
            
            // 立即检查当前选中
            OnSelectionChanged();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            // 取消订阅
            Selection.selectionChanged -= OnSelectionChanged;
        }

        protected override void OnDestroy()
        {
            graphView?.Dispose();
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            // 如果窗口被锁定，忽略选择变化
            if (_isLocked)
                return;
            
            // 检查选中的 GameObject 是否有 NGPStateMachine 组件
            var selectedGameObject = Selection.activeGameObject;
            
            if (selectedGameObject == null)
            {
                // 如果没有选中物体，显示提示
                if (_currentStateMachine != null)
                {
                    _currentStateMachine = null;
                    ClearGraph();
                    UpdateTitle();
                }
                return;
            }
            
            // 避免重复加载同一个物体
            if (selectedGameObject == _lastSelectedGameObject)
                return;
            
            _lastSelectedGameObject = selectedGameObject;
            
            // 查找 NGPStateMachine 组件
            var stateMachine = selectedGameObject.GetComponent<NGPStateMachine>();
            
            if (stateMachine != null && stateMachine.StateGraph != null)
            {
                LoadStateMachine(stateMachine);
            }
            else
            {
                // 没有状态机或图为空，清空视图
                if (_currentStateMachine != null)
                {
                    _currentStateMachine = null;
                    ClearGraph();
                    UpdateTitle();
                }
            }
        }
        
        private void LoadStateMachine(NGPStateMachine stateMachine)
        {
            _currentStateMachine = stateMachine;
            
            // 初始化图
            InitializeGraph(stateMachine.StateGraph);
            
            UpdateTitle();
            
            Debug.Log($"[StateMachineGraphWindow] Loaded graph from {stateMachine.gameObject.name}");
        }
        
        private void ClearGraph()
        {
            if (graphView != null)
            {
                rootView.Remove(graphView);
                graphView.Dispose();
                graphView = null;
            }
        }

        protected override void InitializeWindow(BaseGraph baseGraph)
        {
            UpdateTitle();

            if (graphView == null)
            {
                graphView = new StateMachineGraphView(this);
                
                // 传递当前的 StateMachine 引用给 GraphView（用于 Component Binding）
                if (graphView is StateMachineGraphView smGraphView)
                {
                    smGraphView.SetCurrentStateMachine(_currentStateMachine);
                }
            }

            rootView.Add(graphView);
        }

        private void UpdateTitle()
        {
            string windowTitle;
            
            if (_currentStateMachine != null && _currentStateMachine.StateGraph != null)
            {
                windowTitle = $"{_currentStateMachine.gameObject.name} - {_currentStateMachine.StateGraph.name}";
            }
            else if (graphView?.graph != null)
            {
                windowTitle = graphView.graph.name;
            }
            else
            {
                windowTitle = "State Machine Graph (No Selection)";
            }
            
            titleContent = new GUIContent(windowTitle);
        }
        
        #region 窗口锁定功能实现
        
        /// <summary>
        /// 初始化反射，获取 EditorWindow 的内部 isLocked 属性
        /// </summary>
        private void InitializeReflection()
        {
            try
            {
                var editorWindowType = typeof(EditorWindow);
                _isLockedProperty = editorWindowType.GetProperty(
                    LOCKED_PROPERTY_NAME,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );
                
                if (_isLockedProperty == null)
                {
                    Debug.LogWarning("[StateMachineGraphWindow] 无法通过反射获取 isLocked 属性");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StateMachineGraphWindow] 初始化反射失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 同步内部锁定状态：从 EditorWindow 的内部状态读取并更新本地 _isLocked 字段
        /// </summary>
        private void SyncLockState()
        {
            if (_isLockedProperty != null)
            {
                try
                {
                    var value = _isLockedProperty.GetValue(this);
                    if (value is bool locked)
                    {
                        _isLocked = locked;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StateMachineGraphWindow] 同步锁定状态失败: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 设置窗口锁定状态（通过反射设置 EditorWindow 的内部 isLocked 属性）
        /// </summary>
        private void SetLockState(bool locked)
        {
            _isLocked = locked;
            
            if (_isLockedProperty != null)
            {
                try
                {
                    _isLockedProperty.SetValue(this, locked);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StateMachineGraphWindow] 设置锁定状态失败: {ex.Message}");
                }
            }
            
            // 强制重绘窗口以更新锁定图标
            Repaint();
            
            Debug.Log($"[StateMachineGraphWindow] 窗口 {(locked ? "已锁定" : "已解锁")}");
        }
        
        /// <summary>
        /// 实现 IHasCustomMenu 接口，在窗口右上角菜单中添加锁定选项
        /// </summary>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(
                new GUIContent("锁定"), 
                _isLocked, 
                () => SetLockState(!_isLocked)
            );
        }
        
        #endregion
    }
}

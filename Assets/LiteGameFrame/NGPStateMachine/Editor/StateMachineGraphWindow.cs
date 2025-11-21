using UnityEditor;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// StateMachineGraph 编辑器窗口
    /// 类似 Timeline/Animation 窗口，选中带有 NGPStateMachine 组件的物体时自动加载
    /// </summary>
    public class StateMachineGraphWindow : BaseGraphWindow
    {
        private NGPStateMachine _currentStateMachine;
        private GameObject _lastSelectedGameObject;
        
        [MenuItem("Window/State Machine Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<StateMachineGraphWindow>("State Machine Graph");
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
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
        
        private void OnGUI()
        {
            // 如果没有加载图，显示提示信息
            if (graphView == null || graphView.graph == null)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                EditorGUILayout.HelpBox(
                    "Select a GameObject with NGPStateMachine component to edit its State Machine Graph.\n\n" +
                    "Tip: The graph will automatically load when you select a GameObject in the Hierarchy.",
                    MessageType.Info,
                    true
                );
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
        }
    }
}

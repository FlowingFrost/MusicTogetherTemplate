using UnityEditor;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// StateMachineGraph 的图视图
    /// 支持 Component Binding 和运行时状态显示
    /// </summary>
    public class StateMachineGraphView : BaseGraphView
    {
        private NGPStateMachine _currentStateMachine;
        
        public StateMachineGraphView(EditorWindow window) : base(window)
        {
        }
        
        /// <summary>
        /// 设置当前编辑的 StateMachine（用于 Component Binding）
        /// </summary>
        public void SetCurrentStateMachine(NGPStateMachine stateMachine)
        {
            _currentStateMachine = stateMachine;
        }
        
        /// <summary>
        /// 获取当前编辑的 StateMachine
        /// </summary>
        public NGPStateMachine GetCurrentStateMachine()
        {
            return _currentStateMachine;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 调用基类方法显示默认的右键菜单（添加节点等）
            base.BuildContextualMenu(evt);
            
            // 添加自定义菜单项
            if (_currentStateMachine != null)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("State Machine/Refresh Bindings", (e) => RefreshComponentBindings());
                
                if (Application.isPlaying && _currentStateMachine.IsRunning)
                {
                    evt.menu.AppendAction("State Machine/Stop", (e) => _currentStateMachine.StopStateMachine());
                }
                else if (Application.isPlaying && !_currentStateMachine.IsRunning)
                {
                    evt.menu.AppendAction("State Machine/Start", (e) => _currentStateMachine.StartStateMachine());
                }
            }
        }
        
        private void RefreshComponentBindings()
        {
            if (_currentStateMachine == null)
            {
                Debug.LogWarning("[StateMachineGraphView] No state machine selected");
                return;
            }
            
            Debug.Log($"[StateMachineGraphView] Refreshed component bindings for {_currentStateMachine.gameObject.name}");
            EditorUtility.SetDirty(_currentStateMachine);
        }
    }
}

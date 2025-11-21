using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GraphProcessor;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// StateMachineGraph 资源的 Inspector
    /// 显示提示信息，指导用户使用新的编辑方式
    /// </summary>
    [CustomEditor(typeof(StateMachineGraph))]
    public class StateMachineGraphInspector : GraphInspector
    {
        protected override void CreateInspector()
        {
            base.CreateInspector();

            // 添加信息提示
            var helpBox = new HelpBox(
                "To edit this State Machine Graph:\n" +
                "1. Open Window > State Machine Graph Editor\n" +
                "2. Select a GameObject with NGPStateMachine component\n" +
                "3. The graph will load automatically\n\n" +
                "This workflow allows Component Nodes to bind scene objects correctly.",
                HelpBoxMessageType.Info
            );
            root.Add(helpBox);
            
            // 添加快速打开窗口的按钮
            root.Add(new Button(() => 
            {
                StateMachineGraphWindow.OpenWindow();
            })
            {
                text = "Open State Machine Graph Editor"
            });
        }
    }
}

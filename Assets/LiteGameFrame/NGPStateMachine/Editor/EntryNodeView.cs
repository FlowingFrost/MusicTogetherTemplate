using UnityEditor;
using UnityEditor.Experimental.GraphView;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// EntryNode 的自定义视图
    /// 隐藏输入端口（入口节点不应该有输入）
    /// </summary>
    [NodeCustomEditor(typeof(Nodes.EntryNode))]
    public class EntryNodeView : BaseStateNodeView
    {
        public override void Enable(bool fromInspector = false)
        {
            base.Enable(fromInspector);
            
            // 延迟隐藏输入端口，等待端口完全创建
            schedule.Execute(() => 
            {
                try
                {
                    HideInputPorts();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[EntryNodeView] Failed to hide input ports: {e.Message}");
                }
            }).ExecuteLater(100);
        }

        private void HideInputPorts()
        {
            // 安全检查
            if (inputPortViews == null) return;
            
            // 移除所有输入端口的显示
            foreach (var portView in inputPortViews.ToList())
            {
                if (portView != null)
                {
                    portView.style.display = DisplayStyle.None;
                }
            }
        }
    }
}

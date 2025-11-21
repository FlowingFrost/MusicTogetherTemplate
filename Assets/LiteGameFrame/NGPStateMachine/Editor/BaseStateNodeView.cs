using UnityEditor;
using UnityEditor.Experimental.GraphView;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// BaseStateNode 的自定义视图
    /// 为控制流端口设置颜色：
    /// - OnEnter 输入：绿色（启动节点）
    /// - OnExit 输入：红色（停止节点）
    /// - Signal 输出：白色（通用信号）
    /// </summary>
    [NodeCustomEditor(typeof(BaseStateNode))]
    public class BaseStateNodeView : BaseNodeView
    {
        // OnEnter 端口颜色（绿色）
        private static readonly Color EnterPortColor = new Color(0.3f, 0.8f, 0.3f);
        
        // OnExit 端口颜色（红色）
        private static readonly Color ExitPortColor = new Color(0.9f, 0.3f, 0.3f);
        
        // Signal 输出端口颜色（白色）
        private static readonly Color SignalPortColor = new Color(0.9f, 0.9f, 0.9f);

        public override void Enable(bool fromInspector = false)
        {
            base.Enable(fromInspector);
            
            // 延迟着色，等待端口完全创建
            schedule.Execute(() => 
            {
                try
                {
                    ColorizeControlFlowPorts();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BaseStateNodeView] Failed to colorize ports: {e.Message}");
                }
            }).ExecuteLater(100); // 增加延迟时间，确保端口已创建
        }

        private void ColorizeControlFlowPorts()
        {
            // 安全检查：确保端口列表不为空
            if (inputPortViews == null || outputPortViews == null)
            {
                return;
            }

            // 为输入端口着色
            foreach (var portView in inputPortViews)
            {
                if (portView == null || portView.portData == null) continue;
                
                if (portView.portData.displayType == typeof(ControlFlow))
                {
                    Color portColor = GetControlFlowColor(portView.portData.displayName);
                    SetPortColor(portView, portColor);
                }
            }

            // 为输出端口着色
            foreach (var portView in outputPortViews)
            {
                if (portView == null || portView.portData == null) continue;
                
                if (portView.portData.displayType == typeof(ControlFlow))
                {
                    Color portColor = GetControlFlowColor(portView.portData.displayName);
                    SetPortColor(portView, portColor);
                }
            }
        }

        private Color GetControlFlowColor(string portName)
        {
            // 根据端口名称判断颜色
            if (portName != null)
            {
                if (portName.Contains("Enter"))
                {
                    return EnterPortColor;
                }
                else if (portName.Contains("Exit"))
                {
                    return ExitPortColor;
                }
                else if (portName.Contains("Signal"))
                {
                    return SignalPortColor;
                }
            }
            
            // 默认颜色（白色）
            return Color.white;
        }

        private void SetPortColor(PortView portView, Color color)
        {
            if (portView == null) return;
            
            try
            {
                // 获取端口的连接器元素（圆点）
                var connector = portView.Q("connector");
                if (connector != null)
                {
                    // 设置边框颜色
                    connector.style.borderTopColor = color;
                    connector.style.borderBottomColor = color;
                    connector.style.borderLeftColor = color;
                    connector.style.borderRightColor = color;
                    
                    // 设置背景颜色（当有连接时）
                    connector.style.backgroundColor = color;
                }

                // 同时设置端口容器的边框颜色（可选）
                portView.portColor = color;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BaseStateNodeView] Failed to set port color: {e.Message}");
            }
        }
    }
}

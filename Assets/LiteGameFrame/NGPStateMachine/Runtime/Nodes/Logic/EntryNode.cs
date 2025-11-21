using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 入口节点
    /// 状态机的起始点，只有一个输出端口
    /// </summary>
    [System.Serializable, NodeMenuItem("State Machine/Entry")]
    public class EntryNode : BaseStateNode
    {
        // 入口节点不需要输入端口
        // 注意：不要使用 new 关键字重新声明父类字段，这会导致序列化冲突
        // 输入端口会被父类自动创建，我们只需要在逻辑中忽略它们即可
        
        public override string name => "Entry";
        
        public override Color color => new Color(0.2f, 0.8f, 0.2f); // 绿色
        
        [TextArea(2, 4)]
        public string description = "状态机的入口节点";
        
        public override void OnEnterSignal(string sourceId)
        {
            Debug.Log($"[EntryNode] State machine started (source: {sourceId})");
            
            // 入口节点立即发出信号给后续节点
            TriggerSignal();
            
            // 入口节点完成工作后立即停止
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            // 入口节点通常不处理退出信号
            Debug.Log($"[EntryNode] Received exit signal (ignored)");
        }
    }
}

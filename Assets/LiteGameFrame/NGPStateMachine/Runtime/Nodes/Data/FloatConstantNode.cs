using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 浮点数常量节点
    /// 用于提供一个常量浮点数值
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Data/Float Constant")]
    public class FloatConstantNode : BaseNode
    {
        [Tooltip("常量值")]
        public float value = 1.0f;
        
        [Output("Value")]
        [Tooltip("输出的浮点数值")]
        public float output;
        
        public override string name => "Float";
        
        public override Color color => new Color(0.5f, 0.8f, 0.5f); // 浅绿色
        
        protected override void Enable()
        {
            base.Enable();
            // 始终将 value 赋值给 output，确保数据可用
            output = value;
        }
        
        // 当字段值改变时，更新输出
        [CustomPortBehavior(nameof(output))]
        protected IEnumerable<PortData> OutputPortBehavior(List<SerializableEdge> edges)
        {
            // 确保输出总是最新的值
            output = value;
            yield return new PortData
            {
                identifier = "Value",
                displayName = "Value",
                displayType = typeof(float),
                acceptMultipleEdges = true
            };
        }
    }
}

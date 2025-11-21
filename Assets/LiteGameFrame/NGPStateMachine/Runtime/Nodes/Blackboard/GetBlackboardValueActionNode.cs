using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 获取黑板值节点（状态节点版本）
    /// 从状态机的黑板中读取值，并通过输出端口传递
    /// 接收到 OnEnter 信号时读取一次值，然后立即触发信号
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Blackboard/Get Value (Action)")]
    public class GetBlackboardValueActionNode : BaseStateNode
    {
        [Tooltip("黑板键名")]
        public string key = "MyKey";
        
        [Output("Value")]
        [Tooltip("读取到的值")]
        public object value;
        
        [Tooltip("读取失败时的默认值")]
        public object defaultValue = null;
        
        [Tooltip("是否在值为空时触发信号")]
        public bool triggerOnNull = true;
        
        public override string name => "Get Blackboard (Action)";
        
        public override Color color => new Color(0.5f, 0.7f, 0.8f); // 青色
        
        public override void OnEnterSignal(string sourceId)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[GetBlackboardValueActionNode] Key is empty");
                value = defaultValue;
                StopRunning();
                return;
            }
            
            // 先查找类型
            bool found = false;
            if (_stateMachine.Find<object>(key, out Type actualType))
            {
                // 使用反射调用泛型 Get 方法
                var getMethod = typeof(IStateMachine).GetMethod("Get").MakeGenericMethod(actualType);
                var parameters = new object[] { key, null };
                
                try
                {
                    found = (bool)getMethod.Invoke(_stateMachine, parameters);
                    if (found)
                    {
                        value = parameters[1]; // out 参数的值
                        Debug.Log($"[GetBlackboardValueActionNode] Got '{key}' = {value} (type: {actualType.Name})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GetBlackboardValueActionNode] Failed to get value: {e.Message}");
                }
            }
            
            if (!found)
            {
                Debug.LogWarning($"[GetBlackboardValueActionNode] Key '{key}' not found, using default value");
                value = defaultValue;
            }
            
            // 根据配置决定是否触发信号
            if (value != null || triggerOnNull)
            {
                TriggerSignal();
            }
            
            // 立即停止运行（这是一个瞬时节点）
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            // 瞬时节点不需要处理 Exit 信号
            StopRunning();
        }
    }
}

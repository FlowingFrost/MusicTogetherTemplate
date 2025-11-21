using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 设置黑板值节点
    /// 将输入值写入到状态机的黑板（全局字典）中
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Blackboard/Set Value")]
    public class SetBlackboardValueNode : BaseStateNode
    {
        [Tooltip("黑板键名")]
        public string key = "MyKey";
        
        [Input("Value")]
        [Tooltip("要存储的值")]
        public object value;
        
        [Tooltip("写入成功后是否触发信号")]
        public bool triggerOnSuccess = true;
        
        public override string name => "Set Blackboard";
        
        public override Color color => new Color(0.7f, 0.5f, 0.8f); // 紫色
        
        public override void OnEnterSignal(string sourceId)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[SetBlackboardValueNode] Key is empty, cannot set value");
                StopRunning();
                return;
            }
            
            // 使用泛型方法写入黑板
            bool success = false;
            
            if (value == null)
            {
                // 值为 null，尝试设置为 null
                success = _stateMachine.Set<object>(key, null);
            }
            else
            {
                // 根据值的实际类型写入
                var valueType = value.GetType();
                var setMethod = typeof(IStateMachine).GetMethod("Set").MakeGenericMethod(valueType);
                
                try
                {
                    success = (bool)setMethod.Invoke(_stateMachine, new object[] { key, value });
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SetBlackboardValueNode] Failed to set value: {e.Message}");
                }
            }
            
            if (success)
            {
                Debug.Log($"[SetBlackboardValueNode] Set '{key}' = {value} (type: {value?.GetType().Name ?? "null"})");
                
                if (triggerOnSuccess)
                {
                    // 写入成功，触发输出信号
                    TriggerSignal();
                }
            }
            else
            {
                Debug.LogWarning($"[SetBlackboardValueNode] Failed to set '{key}'");
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

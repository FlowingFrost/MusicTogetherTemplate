using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 定时器节点示例
    /// 按照指定间隔重复触发信号，可设置最大触发次数
    /// 这是一个"持久型节点"的完整示例
    /// </summary>
    [System.Serializable, NodeMenuItem("State Machine/Logic/Timer")]
    public class TimerNode : BaseStateNode
    {
        [Tooltip("触发间隔（秒）")]
        public float interval = 1.0f;
        
        [Tooltip("最大触发次数（0=无限）")]
        public int maxTriggers = 0;
        
        [Output("Trigger Count")]
        [Tooltip("当前已触发次数")]
        public int triggerCount;
        
        [Output("Progress")]
        [Tooltip("当前周期进度（0-1）")]
        public float progress;
        
        private float _lastTriggerTime;
        private bool _isRunning;
        
        public override string name => "Timer";
        
        public override Color color => new Color(0.7f, 0.4f, 0.9f); // 紫色
        
        public override void OnEnterSignal(string sourceId)
        {
            _isRunning = true;
            _lastTriggerTime = Time.time;
            triggerCount = 0;
            progress = 0f;
            
            Debug.Log($"[TimerNode] Started timer (interval: {interval}s, maxTriggers: {maxTriggers})");
        }
        
        public override void OnUpdate()
        {
            if (!_isRunning) return;
            
            float elapsed = Time.time - _lastTriggerTime;
            progress = Mathf.Clamp01(elapsed / interval);
            
            if (elapsed >= interval)
            {
                _lastTriggerTime = Time.time;
                triggerCount++;
                progress = 0f;
                
                Debug.Log($"[TimerNode] Triggered #{triggerCount}");
                
                // 每次触发都发出信号（但不停止运行）
                TriggerSignal();
                
                // 达到最大次数时停止
                if (maxTriggers > 0 && triggerCount >= maxTriggers)
                {
                    _isRunning = false;
                    Debug.Log($"[TimerNode] Reached max triggers, stopping");
                    StopRunning();
                }
            }
        }
        
        public override void OnExitSignal(string sourceId)
        {
            _isRunning = false;
            
            Debug.Log($"[TimerNode] Force stopped by exit signal (triggered {triggerCount} times)");
            
            // 停止运行
            StopRunning();
        }
        
        public override void Cleanup()
        {
            _isRunning = false;
        }
    }
}

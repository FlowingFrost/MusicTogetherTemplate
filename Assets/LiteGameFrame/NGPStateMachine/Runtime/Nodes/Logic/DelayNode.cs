using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 延时节点
    /// 接收到 OnEnter 信号后等待指定时间，然后发出 OnEnter 信号
    /// 这是一个典型的"持续型节点"示例
    /// </summary>
    [System.Serializable, NodeMenuItem("State Machine/Logic/Delay")]
    public class DelayNode : BaseStateNode
    {
        [Input("Duration")]
        [Tooltip("延时时长（秒）")]
        public float duration = 1.0f;
        
        [Output("Elapsed Time")]
        [Tooltip("已经过的时间")]
        public float elapsedTime;
        
        private float _startTime;
        private bool _isRunning;
        
        public override string name => "Delay";
        
        public override Color color => new Color(0.4f, 0.6f, 0.9f); // 蓝色
        
        public override void OnEnterSignal(string sourceId)
        {
            _startTime = Time.time;
            _isRunning = true;
            elapsedTime = 0f;
            
            Debug.Log($"[DelayNode] Started delay of {duration}s (source: {sourceId})");
        }
        
        public override void OnExitSignal(string sourceId)
        {
            _isRunning = false;
            
            Debug.Log($"[DelayNode] Force stopped by exit signal (source: {sourceId})");
            
            // 停止运行
            StopRunning();
        }
        
        public override void OnUpdate()
        {
            if (!_isRunning) return;
            
            elapsedTime = Time.time - _startTime;
            
            if (elapsedTime >= duration)
            {
                _isRunning = false;
                
                Debug.Log($"[DelayNode] Delay completed after {elapsedTime:F2}s");
                
                // 延时结束，发出信号给后续节点
                TriggerSignal();
                
                // 主动停止运行
                StopRunning();
            }
        }
        
        public override void Cleanup()
        {
            _isRunning = false;
        }
    }
}

using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// Transform 操作节点
    /// 用于移动、旋转或缩放场景中的 Transform
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Component/Transform Move")]
    public class TransformMoveNode : ComponentNode<Transform>
    {
        public enum MoveMode
        {
            Absolute,  // 绝对位置
            Relative   // 相对偏移
        }
        
        [Input("Target Position")]
        [Tooltip("目标位置")]
        public Vector3 targetPosition = Vector3.zero;
        
        [Tooltip("移动模式")]
        public MoveMode moveMode = MoveMode.Absolute;
        
        [Input("Duration")]
        [Tooltip("移动时长（秒）")]
        public float duration = 1.0f;
        
        [Tooltip("是否使用本地坐标")]
        public bool useLocalSpace = false;
        
        private Vector3 _startPosition;
        private float _elapsedTime;
        private bool _isMoving;
        
        public override string name => "Transform Move";
        public override Color color => new Color(0.3f, 0.7f, 0.9f); // 蓝色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetBoundComponent(out var transform))
                return;
            
            _startPosition = useLocalSpace ? transform.localPosition : transform.position;
            _elapsedTime = 0f;
            _isMoving = true;
            
            Debug.Log($"[TransformMoveNode] Started moving {transform.name} to {targetPosition} over {duration}s");
        }
        
        public override void OnExitSignal(string sourceId)
        {
            _isMoving = false;
            Debug.Log($"[TransformMoveNode] Force stopped");
            StopRunning();
        }
        
        public override void OnUpdate()
        {
            if (!_isMoving) return;
            
            if (!TryGetBoundComponent(out var transform))
            {
                _isMoving = false;
                StopRunning();
                return;
            }
            
            _elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsedTime / duration);
            
            Vector3 endPosition = moveMode == MoveMode.Absolute 
                ? targetPosition 
                : _startPosition + targetPosition;
            
            Vector3 currentPosition = Vector3.Lerp(_startPosition, endPosition, t);
            
            if (useLocalSpace)
                transform.localPosition = currentPosition;
            else
                transform.position = currentPosition;
            
            if (t >= 1.0f)
            {
                _isMoving = false;
                Debug.Log($"[TransformMoveNode] Move completed");
                TriggerSignal();
                StopRunning();
            }
        }
        
        public override void Cleanup()
        {
            _isMoving = false;
        }
    }
}


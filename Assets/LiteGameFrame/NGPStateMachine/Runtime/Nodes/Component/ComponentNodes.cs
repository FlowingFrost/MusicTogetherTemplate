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
    
    /// <summary>
    /// Animator 控制节点
    /// 用于触发 Animator 的参数或动画
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Component/Animator Trigger")]
    public class AnimatorTriggerNode : ComponentNode<Animator>
    {
        public enum ParameterType
        {
            Trigger,
            Bool,
            Int,
            Float
        }
        
        [Tooltip("参数名称")]
        public string parameterName = "TriggerName";
        
        [Tooltip("参数类型")]
        public ParameterType parameterType = ParameterType.Trigger;
        
        [Input("Bool Value")]
        public bool boolValue = true;
        
        [Input("Int Value")]
        public int intValue = 0;
        
        [Input("Float Value")]
        public float floatValue = 0f;
        
        public override string name => "Animator Trigger";
        public override Color color => new Color(0.9f, 0.5f, 0.3f); // 橙色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetBoundComponent(out var animator))
                return;
            
            if (string.IsNullOrEmpty(parameterName))
            {
                Debug.LogError($"[AnimatorTriggerNode] Parameter name is empty!");
                TriggerSignal();
                StopRunning();
                return;
            }
            
            try
            {
                switch (parameterType)
                {
                    case ParameterType.Trigger:
                        animator.SetTrigger(parameterName);
                        Debug.Log($"[AnimatorTriggerNode] Set trigger: {parameterName}");
                        break;
                    
                    case ParameterType.Bool:
                        animator.SetBool(parameterName, boolValue);
                        Debug.Log($"[AnimatorTriggerNode] Set bool {parameterName} = {boolValue}");
                        break;
                    
                    case ParameterType.Int:
                        animator.SetInteger(parameterName, intValue);
                        Debug.Log($"[AnimatorTriggerNode] Set int {parameterName} = {intValue}");
                        break;
                    
                    case ParameterType.Float:
                        animator.SetFloat(parameterName, floatValue);
                        Debug.Log($"[AnimatorTriggerNode] Set float {parameterName} = {floatValue}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AnimatorTriggerNode] Error setting parameter: {e.Message}");
            }
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[AnimatorTriggerNode] Force stopped");
            StopRunning();
        }
    }
    
    /// <summary>
    /// GameObject 激活节点
    /// 用于激活或禁用 GameObject
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Component/GameObject SetActive")]
    public class GameObjectSetActiveNode : ComponentNode<GameObject>
    {
        [Input("Active")]
        [Tooltip("是否激活")]
        public bool active = true;
        
        public override string name => "GameObject SetActive";
        public override Color color => new Color(0.5f, 0.8f, 0.4f); // 绿色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetBoundComponent(out var gameObject))
                return;
            
            gameObject.SetActive(active);
            Debug.Log($"[GameObjectSetActiveNode] Set {gameObject.name}.SetActive({active})");
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[GameObjectSetActiveNode] Force stopped");
            StopRunning();
        }
    }
}


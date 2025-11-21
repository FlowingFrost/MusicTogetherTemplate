using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
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
}


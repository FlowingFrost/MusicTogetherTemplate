using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 条件判断节点
    /// 使用 ConditionEvaluator 解析字符串表达式进行判断
    /// 只有当条件为 true 时才发出信号
    /// </summary>
    [System.Serializable, NodeMenuItem("State Machine/Logic/Condition")]
    public class ConditionNode : BaseStateNode
    {
        [TextArea(2, 4)]
        [Tooltip("条件表达式，例如: health > 0")]
        public string expression = "value > 0";
        
        [Tooltip("表达式中的变量（从黑板或数据端口读取）")]
        public List<string> variableNames = new List<string>();
        
        public override string name => "Condition";
        
        public override Color color => new Color(0.9f, 0.5f, 0.3f); // 橙色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 构建变量字典
            var variables = new Dictionary<string, object>();
            
            // 从黑板读取变量
            foreach (var varName in variableNames)
            {
                if (StateMachine.Get<object>(varName, out var value))
                {
                    variables[varName] = value;
                }
                else
                {
                    Debug.LogWarning($"[ConditionNode] Variable '{varName}' not found in blackboard");
                }
            }
            
            // 求值表达式
            bool result = false;
            try
            {
                result = ConditionEvaluator.Evaluate(expression, variables);
                Debug.Log($"[ConditionNode] Expression '{expression}' evaluated to: {result} (source: {sourceId})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConditionNode] Error evaluating expression '{expression}': {e.Message}");
            }
            
            // 只有条件为 true 时才发出信号
            if (result)
            {
                TriggerSignal();
            }
            else
            {
                Debug.Log($"[ConditionNode] Condition is false, no signal output");
            }
            
            // 条件节点是瞬时节点，立即停止
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[ConditionNode] Force stopped (source: {sourceId})");
            
            // 停止运行
            StopRunning();
        }
    }
}

using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 获取黑板值节点
    /// 从状态机的黑板（全局字典）中读取值并输出
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Blackboard/Get Value")]
    public class GetBlackboardValueNode : BaseNode
    {
        [Tooltip("黑板键名")]
        public string key = "MyKey";
        
        [Tooltip("期望的值类型（用于类型检查）")]
        public TypeReference expectedType = new TypeReference(typeof(object));
        
        [Output("Value")]
        [Tooltip("读取到的值")]
        public object value;
        
        [Output("Found")]
        [Tooltip("是否找到该键")]
        public bool found;
        
        public override string name => "Get Blackboard";
        
        public override Color color => new Color(0.5f, 0.7f, 0.8f); // 青色
        
        // 这是一个数据节点，不是状态节点，所以不继承 BaseStateNode
        // 它会在其他节点需要数据时被自动调用 Process()
        
        protected override void Process()
        {
            // 需要通过某种方式获取状态机引用
            // 由于这是一个纯数据节点，我们需要从图中获取状态机
            var stateMachine = (graph as StateMachineGraph)?.GetStateMachine();
            
            if (stateMachine == null)
            {
                Debug.LogWarning($"[GetBlackboardValueNode] Cannot access state machine");
                found = false;
                value = null;
                return;
            }
            
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[GetBlackboardValueNode] Key is empty");
                found = false;
                value = null;
                return;
            }
            
            // 先查找类型
            if (stateMachine.Find<object>(key, out Type actualType))
            {
                // 使用反射调用泛型 Get 方法
                var getMethod = typeof(IStateMachine).GetMethod("Get").MakeGenericMethod(actualType);
                var parameters = new object[] { key, null };
                
                try
                {
                    found = (bool)getMethod.Invoke(stateMachine, parameters);
                    value = parameters[1]; // out 参数的值
                    
                    if (found)
                    {
                        Debug.Log($"[GetBlackboardValueNode] Got '{key}' = {value} (type: {actualType.Name})");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GetBlackboardValueNode] Failed to get value: {e.Message}");
                    found = false;
                    value = null;
                }
            }
            else
            {
                Debug.LogWarning($"[GetBlackboardValueNode] Key '{key}' not found in blackboard");
                found = false;
                value = null;
            }
        }
    }
    
    /// <summary>
    /// 类型引用辅助类（用于在编辑器中选择类型）
    /// </summary>
    [Serializable]
    public class TypeReference
    {
        [SerializeField]
        private string assemblyQualifiedName;
        
        public TypeReference(Type type)
        {
            assemblyQualifiedName = type?.AssemblyQualifiedName;
        }
        
        public Type Type
        {
            get
            {
                if (string.IsNullOrEmpty(assemblyQualifiedName))
                    return typeof(object);
                
                return Type.GetType(assemblyQualifiedName) ?? typeof(object);
            }
            set
            {
                assemblyQualifiedName = value?.AssemblyQualifiedName;
            }
        }
    }
}

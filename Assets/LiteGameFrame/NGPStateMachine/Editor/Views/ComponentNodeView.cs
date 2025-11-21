using UnityEditor;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System;
using UnityEditor.UIElements;//Object Field的声明用到了此内容，但是我不确定是否正确。

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// ComponentNode 的自定义视图
    /// 在节点上显示 Component 绑定字段
    /// </summary>
    [NodeCustomEditor(typeof(ComponentNode<>))]
    public class ComponentNodeView : BaseNodeView
    {
        public override void Enable()
        {
            base.Enable();
            
            // 延迟添加绑定控件，避免初始化时机问题
            schedule.Execute(() => AddComponentBindingField()).ExecuteLater(100);
        }
        
        private void AddComponentBindingField()
        {
            try
            {
                // 获取节点类型
                var nodeType = nodeTarget.GetType();
                
                // 检查是否是 ComponentNode<T> 的子类
                var baseType = nodeType.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ComponentNode<>))
                    {
                        var componentType = baseType.GetGenericArguments()[0];
                        AddBindingFieldForType(componentType);
                        return;
                    }
                    baseType = baseType.BaseType;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ComponentNodeView] Error adding binding field: {e.Message}");
            }
        }
        
        private void AddBindingFieldForType(Type componentType)
        {
            // 获取当前的 StateMachine
            var graphView = owner as StateMachineGraphView;
            if (graphView == null)
            {
                Debug.LogWarning("[ComponentNodeView] GraphView is not StateMachineGraphView");
                return;
            }
            
            var stateMachine = graphView.GetCurrentStateMachine();
            if (stateMachine == null)
            {
                Debug.LogWarning("[ComponentNodeView] No StateMachine selected");
                return;
            }
            
            // 创建 Object 字段
            var bindingField = new ObjectField($"Bind {componentType.Name}")
            {
                objectType = componentType,
                allowSceneObjects = true
            };
            
            // 获取当前绑定的值
            var currentBinding = stateMachine.GetComponentBinding<UnityEngine.Object>(nodeTarget.GUID, "target");
            bindingField.value = currentBinding;
            
            // 监听值变化
            bindingField.RegisterValueChangedCallback(evt =>
            {
                stateMachine.SetComponentBinding(nodeTarget.GUID, "target", evt.newValue);
                EditorUtility.SetDirty(stateMachine);
            });
            
            // 添加到控件容器
            controlsContainer.Add(bindingField);
        }
    }
}


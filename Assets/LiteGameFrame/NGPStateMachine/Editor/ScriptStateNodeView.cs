using UnityEditor;
using UnityEditor.UIElements;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace LiteGameFrame.NGPStateMachine.Editor
{
    /// <summary>
    /// ScriptStateNode 的自定义编辑器视图
    /// 
    /// 功能：
    /// 1. 显示目标脚本绑定字段（targetScriptField）
    /// 2. 显示额外绑定字段列表（additionalBindingFields）
    /// 3. 为每个字段创建 ObjectField，支持场景对象绑定
    /// 4. 自动验证绑定的脚本是否实现 IStateHandler 接口
    /// </summary>
    [NodeCustomEditor(typeof(ScriptStateNode))]
    public class ScriptStateNodeView : BaseStateNodeView
    {
        private ScriptStateNode _scriptNode;
        private NGPStateMachine _stateMachine;
        
        public override void Enable(bool fromInspector = false)
        {
            base.Enable(fromInspector);
            
            _scriptNode = nodeTarget as ScriptStateNode;
            
            // 延迟添加绑定字段，确保视图完全初始化
            schedule.Execute(() => 
            {
                try
                {
                    AddScriptBindingFields();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ScriptStateNodeView] Failed to add binding fields: {e.Message}");
                }
            }).ExecuteLater(150);
        }
        
        /// <summary>
        /// 添加脚本绑定字段到节点视图
        /// </summary>
        private void AddScriptBindingFields()
        {
            if (_scriptNode == null) return;
            
            // 获取状态机引用
            var graphView = owner as StateMachineGraphView;
            if (graphView == null)
            {
                Debug.LogWarning("[ScriptStateNodeView] GraphView is not StateMachineGraphView");
                return;
            }
            
            _stateMachine = graphView.GetCurrentStateMachine();
            if (_stateMachine == null)
            {
                Debug.LogWarning("[ScriptStateNodeView] No StateMachine selected");
                return;
            }
            
            // 添加分隔线
            var separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            separator.style.marginTop = 5;
            separator.style.marginBottom = 5;
            controlsContainer.Add(separator);
            
            // 添加标题
            var titleLabel = new Label("Script Bindings");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 5;
            controlsContainer.Add(titleLabel);
            
            // 1. 添加目标脚本字段
            AddTargetScriptField();
            
            // 2. 添加额外绑定字段
            if (_scriptNode.additionalBindingFields != null && _scriptNode.additionalBindingFields.Length > 0)
            {
                var additionalLabel = new Label("Additional Bindings:");
                additionalLabel.style.marginTop = 10;
                additionalLabel.style.marginBottom = 5;
                additionalLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                controlsContainer.Add(additionalLabel);
                
                foreach (var fieldName in _scriptNode.additionalBindingFields)
                {
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        AddBindingField(fieldName, typeof(UnityEngine.Object));
                    }
                }
            }
            
            // 添加帮助信息
            AddHelpBox();
        }
        
        /// <summary>
        /// 添加目标脚本绑定字段
        /// </summary>
        private void AddTargetScriptField()
        {
            var scriptField = new ObjectField("🎯 Target Script")
            {
                objectType = typeof(MonoBehaviour),
                allowSceneObjects = true
            };
            
            scriptField.style.marginBottom = 5;
            
            // 获取当前绑定的脚本
            var currentScript = _stateMachine.GetComponentBinding<MonoBehaviour>(
                _scriptNode.GUID, 
                _scriptNode.targetScriptField
            );
            scriptField.value = currentScript;
            
            // 监听值变化
            scriptField.RegisterValueChangedCallback(evt =>
            {
                var newScript = evt.newValue as MonoBehaviour;
                
                // 验证脚本是否实现 IStateHandler 接口
                if (newScript != null && !(newScript is IStateHandler))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Script", 
                        $"Script {newScript.GetType().Name} must implement IStateHandler interface!\n\n" +
                        "Required methods:\n" +
                        "- void OnStateEnter(StateContext context)\n" +
                        "- void OnStateUpdate(StateContext context)\n" +
                        "- void OnStateExit(StateContext context)", 
                        "OK"
                    );
                    scriptField.value = evt.previousValue;
                    return;
                }
                
                // 保存绑定
                _stateMachine.SetComponentBinding(
                    _scriptNode.GUID, 
                    _scriptNode.targetScriptField, 
                    newScript
                );
                EditorUtility.SetDirty(_stateMachine);
                
                // 显示反馈
                if (newScript != null)
                {
                    Debug.Log($"[ScriptStateNode] Bound script: {newScript.GetType().Name} on {newScript.gameObject.name}");
                }
                else
                {
                    Debug.Log($"[ScriptStateNode] Unbound script");
                }
            });
            
            controlsContainer.Add(scriptField);
        }
        
        /// <summary>
        /// 添加通用绑定字段
        /// </summary>
        private void AddBindingField(string fieldName, Type objectType)
        {
            var bindingField = new ObjectField($"  🔗 {fieldName}")
            {
                objectType = objectType,
                allowSceneObjects = true
            };
            
            bindingField.style.marginBottom = 3;
            
            // 获取当前绑定的对象
            var currentBinding = _stateMachine.GetComponentBinding<UnityEngine.Object>(
                _scriptNode.GUID, 
                fieldName
            );
            bindingField.value = currentBinding;
            
            // 监听值变化
            bindingField.RegisterValueChangedCallback(evt =>
            {
                _stateMachine.SetComponentBinding(
                    _scriptNode.GUID, 
                    fieldName, 
                    evt.newValue
                );
                EditorUtility.SetDirty(_stateMachine);
                
                // 显示反馈
                if (evt.newValue != null)
                {
                    Debug.Log($"[ScriptStateNode] Bound {fieldName}: {evt.newValue.name}");
                }
            });
            
            controlsContainer.Add(bindingField);
        }
        
        /// <summary>
        /// 添加帮助信息框
        /// </summary>
        private void AddHelpBox()
        {
            var helpBox = new HelpBox(
                "💡 Usage:\n" +
                "1. Bind a MonoBehaviour that implements IStateHandler\n" +
                "2. Optional: Add additional bindings in additionalBindingFields\n" +
                "3. Access bindings in script: context.GetBinding<T>(\"fieldName\")\n" +
                "4. Complete state: context.RequestComplete()",
                HelpBoxMessageType.Info
            );
            
            helpBox.style.marginTop = 10;
            controlsContainer.Add(helpBox);
        }
    }
}

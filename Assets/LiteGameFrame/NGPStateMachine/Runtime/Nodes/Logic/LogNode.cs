using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 日志节点
    /// 支持格式化字符串，可以从黑板读取变量并嵌入到日志消息中
    /// 格式: "Debug info: time = %f, score = %d" + 变量名列表
    /// 
    /// 格式说明符:
    /// %s - 字符串
    /// %d - 整数
    /// %f - 浮点数
    /// %b - 布尔值
    /// %v - 通用值（调用 ToString）
    /// </summary>
    [System.Serializable, NodeMenuItem("State Machine/Logic/Log")]
    public class LogNode : BaseStateNode
    {
        public enum LogLevel
        {
            Log,
            Warning,
            Error
        }
        
        [Tooltip("日志级别")]
        public LogLevel logLevel = LogLevel.Log;
        
        [TextArea(2, 4)]
        [Tooltip("格式化消息（使用 %s, %d, %f, %b, %v 作为占位符）")]
        public string message = "Log message";
        
        [Tooltip("黑板变量名列表（按顺序对应格式化占位符）")]
        public List<string> variableNames = new List<string>();
        
        [Input("Value")]
        [Tooltip("可选：直接输入的值（会附加到消息末尾）")]
        public object inputValue;
        
        public override string name => "Log";
        
        public override Color color => new Color(0.8f, 0.8f, 0.2f); // 黄色
        
        public override void OnEnterSignal(string sourceId)
        {
            string fullMessage = FormatMessage();
            
            if (inputValue != null)
            {
                fullMessage += $" | Input: {inputValue}";
            }
            
            fullMessage += $" (source: {sourceId})";
            
            // 根据级别打印日志
            switch (logLevel)
            {
                case LogLevel.Log:
                    Debug.Log($"[LogNode] {fullMessage}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"[LogNode] {fullMessage}");
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[LogNode] {fullMessage}");
                    break;
            }
            
            // 立即发出信号
            TriggerSignal();
            
            // 瞬时节点，立即停止
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[LogNode] Force stopped (source: {sourceId})");
            
            // 停止运行
            StopRunning();
        }
        
        /// <summary>
        /// 格式化消息，将占位符替换为黑板变量的值
        /// </summary>
        private string FormatMessage()
        {
            if (string.IsNullOrEmpty(message))
                return "";
            
            if (_stateMachine == null)
            {
                Debug.LogWarning("[LogNode] StateMachine is null, cannot access blackboard");
                return message;
            }
            
            // 查找所有格式说明符
            var matches = Regex.Matches(message, @"%[sdfbv]");
            if (matches.Count == 0)
            {
                // 没有格式说明符，直接返回原始消息
                return message;
            }
            
            if (matches.Count > variableNames.Count)
            {
                Debug.LogWarning($"[LogNode] Not enough variables: found {matches.Count} placeholders but only {variableNames.Count} variables");
            }
            
            // 构建格式化后的消息
            StringBuilder result = new StringBuilder();
            int lastIndex = 0;
            int varIndex = 0;
            
            foreach (Match match in matches)
            {
                // 添加占位符之前的文本
                result.Append(message.Substring(lastIndex, match.Index - lastIndex));
                
                // 获取对应的变量值
                string formattedValue = "<?>";
                if (varIndex < variableNames.Count)
                {
                    string varName = variableNames[varIndex];
                    object value = GetBlackboardValue(varName);
                    
                    if (value != null)
                    {
                        // 根据格式说明符格式化值
                        string formatSpec = match.Value;
                        formattedValue = FormatValue(value, formatSpec);
                    }
                    else
                    {
                        formattedValue = $"<{varName}:null>";
                    }
                }
                else
                {
                    formattedValue = "<missing>";
                }
                
                result.Append(formattedValue);
                lastIndex = match.Index + match.Length;
                varIndex++;
            }
            
            // 添加剩余的文本
            if (lastIndex < message.Length)
            {
                result.Append(message.Substring(lastIndex));
            }
            
            return result.ToString();
        }
        
        /// <summary>
        /// 从黑板获取变量值
        /// </summary>
        private object GetBlackboardValue(string varName)
        {
            if (string.IsNullOrEmpty(varName))
                return null;
            
            // 先查找类型
            if (_stateMachine.Find<object>(varName, out Type actualType))
            {
                // 使用反射调用泛型 Get 方法
                var getMethod = typeof(IStateMachine).GetMethod("Get").MakeGenericMethod(actualType);
                var parameters = new object[] { varName, null };
                
                try
                {
                    bool found = (bool)getMethod.Invoke(_stateMachine, parameters);
                    if (found)
                    {
                        return parameters[1]; // out 参数的值
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LogNode] Failed to get '{varName}': {e.Message}");
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 根据格式说明符格式化值
        /// </summary>
        private string FormatValue(object value, string formatSpec)
        {
            try
            {
                switch (formatSpec)
                {
                    case "%s": // 字符串
                        return value.ToString();
                    
                    case "%d": // 整数
                        if (value is int || value is long || value is short || value is byte)
                            return value.ToString();
                        if (value is float f)
                            return ((int)f).ToString();
                        if (value is double d)
                            return ((int)d).ToString();
                        return Convert.ToInt32(value).ToString();
                    
                    case "%f": // 浮点数
                        if (value is float || value is double)
                            return string.Format("{0:F2}", value);
                        if (value is int || value is long)
                            return string.Format("{0:F2}", (float)Convert.ToDouble(value));
                        return string.Format("{0:F2}", Convert.ToDouble(value));
                    
                    case "%b": // 布尔值
                        if (value is bool b)
                            return b.ToString();
                        return Convert.ToBoolean(value).ToString();
                    
                    case "%v": // 通用值
                    default:
                        return value.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LogNode] Failed to format value '{value}' with spec '{formatSpec}': {e.Message}");
                return value?.ToString() ?? "null";
            }
        }
    }
}

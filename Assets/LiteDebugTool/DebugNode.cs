using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace LiteDebugTool
{
    public enum LogLevel {Head, Content, End}
    public readonly struct LogData
    {
        public readonly LogType logType;
        public readonly string message;
        public readonly string stackTrace;
        
        public LogData(LogType logType, string message, string stackTrace)
        {
            this.logType = logType;
            this.message = message;
            this.stackTrace = stackTrace;
        }
        public static LogData Info(string message,LogType type= LogType.Log)=> new LogData(type, message, null);
        public static LogData Error(string message, string stackTrace,LogType type= LogType.Error)=> new LogData(type, message, stackTrace);
    }
    public class DebugNode
    {
        protected DebugNode parent;
        public Action<DebugNode> OnParentDestroy;
        protected string name;
    
        //Internal Function
        private DebugNode(string nodeName, DebugNode parentNode = null,bool isRoot = false)
        {
            name = nodeName;
            parent = parentNode;
            if(isRoot) parent = this;
        }

        
        
        
        //Public Function
        public static DebugNode CreateRoot(string name)
        {
            return new DebugNode(name,null,true);
        }

        public DebugNode CreateChild(string name)
        {
            return new DebugNode(name, this, false);
        }

        public void OnDestroy()
        {
            OnParentDestroy.Invoke(parent);
        }
        // 记录日志
        public virtual void LogBegin(string message, LogType logType = LogType.Log)
        {
            message = $"{name}-{message}:";
            ReturnLog(LogData.Info(message,logType),0);
        }
        public virtual void LogEnd(string message, LogType logType = LogType.Log)
        {
            message = $"{name}-{message}";
            ReturnLog(LogData.Info(message, logType),0);
        }

        public virtual void LogInfo(string message, LogType logType = LogType.Log)
        {
            ReturnLog(LogData.Info(message,logType));
        }

        public virtual void LogError(string message, string stackTrace, LogType logType = LogType.Error)
        {
            ReturnLog(LogData.Error(message,stackTrace,logType));
        }

        public virtual void ReturnLog(LogData data,int level = 1)
        {
            if (parent == null || parent == this)
            {
                DebugLogger.Instance.LogMessage(data, level);
            }
            else
            {
                parent.ReturnLog(data, level + 1);
            }
        }
        /*public virtual void Log(string message,LogType logType = LogType.Log,string stackTrace = null,int level = 1)
        {
            // 递归调用父节点的日志方法
            if (parent != null && parent != this)
            {
                parent.Log(message,logType,stackTrace,level + 1);
            }
            else
            {
                if (parent != this)
                {
                    message = "missing parent!" + message;
                }
                // 到达根节点，输出到单例
                DebugLogger.Instance.LogMessage(message, level, logType, stackTrace);
            }
        }*/
    }
}
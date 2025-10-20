using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteDebugTool
{
    //public enum LogLevel {Head, Content, End}
    public readonly struct LogData
    {
        public readonly DateTime time;
        public readonly LogType logType;
        public readonly string message;
        public readonly string stackTrace;

        public LogData(LogType logType, string message, string stackTrace)
        {
            this.time = DateTime.Now;
            this.logType = logType;
            this.message = message;
            this.stackTrace = stackTrace;
        }
        public static LogData Info(string message, LogType type = LogType.Log) => new LogData(type, message, null);
        public static LogData Error(string message, string stackTrace, LogType type = LogType.Error) => new LogData(type, message, stackTrace);
    

    }

    public readonly struct LogEntry
    {
        public readonly string Information;
        public readonly int Level;
        public readonly string Data;

        public LogEntry(string information, int level, string data)
        {
            Information = information;
            Level = level;
            Data = data;
        }
        static string GetDirectory(int currentLevel, bool isLast = false)
        {
            if (!isLast)
                return string.Concat(Enumerable.Repeat("\u2502 ", currentLevel));
            else
                return string.Concat(Enumerable.Repeat("\u2502 ", currentLevel - 1)) + "\u2514\u2500";
        }
        public string ToString(bool isLast = false)
        {
            string indent = GetDirectory(Level, isLast);
            return $"{Information}{indent}{Data}";
        }
    }

    public class DebugNode
    {
        protected DebugNode parent;
        //public int Level;
        public Action<DebugNode> OnParentDestroy;
        protected string name;

        //Internal Function
        private DebugNode(string nodeName, DebugNode parentNode = null, bool isRoot = false)
        {
            name = nodeName;
            parent = parentNode;
            //Level = parentNode == null ? 0 : parentNode.Level + 1;
            if (isRoot) parent = this;
        }

        //生成日志
        static string LengthWrapper(string content, int count, string wrapperCharL = "[", string wrapperCharR = "]")
        {
            int spaceCount = count - content.Length;
            if (spaceCount < 0) spaceCount = 0;
            string spaces = string.Concat(Enumerable.Repeat(" ", spaceCount));
            return string.Concat(wrapperCharL, content, spaces, wrapperCharR);
        }

        static string GetLogType(LogType type)
        {
            return type switch
            {
                LogType.Log => "Info",
                LogType.Warning => "Warning",
                LogType.Assert => "Assert",
                LogType.Error => "Error",
                LogType.Exception => "Exception",
                _ => "",
            };
        }
        static string GetLogTime(DateTime time)
        {
            return $"[{time:hh:mm:ss}]";
        }
        public virtual void Log(string message, LogType logType = LogType.Log, string stackTrace = null)
        {
            //预期结构：information = [hh:mm:ss] - [LogType] data = message + stackTrace
            string information = $"{GetLogTime(DateTime.Now)} - {LengthWrapper(GetLogType(logType), 9)}";
            string data = (stackTrace == null) ? $"{message}" : $"{message}: {stackTrace}";
            ReturnLog(information, data);
        }
        public virtual void LogBegin(string message, LogType logType = LogType.Log)
        {
            string information = $"{GetLogTime(DateTime.Now)} - {LengthWrapper(GetLogType(logType), 9)}";
            string data = $"[{name}]-{message}:";
            ReturnLog(information, data, 0);
        }
        public virtual void LogEnd(string message, LogType logType = LogType.Log)
        {
            string information = $"{GetLogTime(DateTime.Now)} - {LengthWrapper(GetLogType(logType), 9)}";
            string data = $"[{name}]-{message}:";
            ReturnLog(information, data, 0);
        }

        public virtual void LogInfo(string message, LogType logType = LogType.Log) => Log(message, logType);
        public virtual void LogWarning(string message, string stackTrace = null, LogType logType = LogType.Warning) => Log(message, logType, stackTrace);
        public virtual void LogError(string message, string stackTrace, LogType logType = LogType.Error)=>Log(message, logType, stackTrace);

        //节点管理
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
            OnParentDestroy?.Invoke(parent);
        }

        





        // 记录日志
        
        public virtual void ReturnLog(string information, string data, int level = 1)
        {
            if (parent == null || parent == this)
            {
                LogEntry entry = new LogEntry(information, level, data);
                DebugLogger.Instance.Log(entry);
            }
            else
            {
                parent.ReturnLog(information, data, level + 1);
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
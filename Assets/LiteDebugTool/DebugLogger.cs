using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using LiteGameFrame.CoreInfrastructure;
using UnityEditor.Experimental.GraphView;

namespace LiteDebugTool
{
    public class DebugLogger : Singleton<DebugLogger>
    {
        public override bool Persistent => true;
        
        public bool recordUnityLog = false;
        public string logName = "GameLog";
        
        private Queue<string> logQueue = new Queue<string>();
        private const int MAX_QUEUE_SIZE = 100;
        private string logFilePath;

        //Wrapper
        private int lastLogLevel;
        private LogData lastLogData;
        private DateTime lastLogTime;
        protected override void Awake()
        {
            base.Awake();
            logName += DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            lastLogLevel = 0;
            lastLogTime = DateTime.Now;
            lastLogData = LogData.Info("Log begin, saved at" + logName);
            logFilePath = Path.Combine(Application.persistentDataPath, logName);
            Debug.Log(logFilePath);
            if(recordUnityLog)
                Application.logMessageReceived += HandleUnityLog;
        }

        private void OnDestroy()
        {
            if(recordUnityLog)
                Application.logMessageReceived -= HandleUnityLog;
            FlushLogsToFile();
        }

        // 处理Unity自带日志
        private void HandleUnityLog(string logString, string stackTrace,LogType type)
        {
            // 可以选择将Unity日志也整合到我们的系统中
            //LogMessage("UnityLog" + logString, 0,type,stackTrace);
        }

        // 记录日志消息
        public void LogMessage(LogData data, int indentLevel)
        {
            PushLog(indentLevel);
            lastLogData = data;
            lastLogTime = DateTime.Now;
            lastLogLevel = indentLevel;
        }

        string GetDirectory(int currentLevel)
        {
            if(lastLogLevel <= currentLevel)
                return string.Concat(Enumerable.Repeat("\u2502 ", lastLogLevel));
            else
                return string.Concat(Enumerable.Repeat("\u2502 ", lastLogLevel - 1)) + "\u2514\u2500";
        }

        string LengthWrapper(string content, int count,string wrapperCharL = "[",string wrapperCharR = "]")
        {
            int spaceCount = count - content.Length;
            if (spaceCount < 0) spaceCount = 0;
            string spaces = string.Concat(Enumerable.Repeat(" ", spaceCount));
            return string.Concat(wrapperCharL, content, spaces, wrapperCharR);
        }

        void PushLog(int currentLevel)
        {
            string indent = GetDirectory(currentLevel);
            string logType = "";
            string formattedMessage ="";
            int logTypeLength = 9;
            switch (lastLogData.logType)
            {
                case LogType.Log:
                    logType = LengthWrapper("Info",logTypeLength);
                    goto default;
                case LogType.Warning:
                    logType = LengthWrapper("Warning",logTypeLength);
                    goto default;
                case LogType.Assert:
                    logType = LengthWrapper("Assert",logTypeLength);
                    goto hasStackTrace;
                case LogType.Error:
                    logType = LengthWrapper("Error",logTypeLength);
                    goto hasStackTrace;
                case LogType.Exception:
                    logType = LengthWrapper("Exception",logTypeLength);
                    goto hasStackTrace;
                hasStackTrace:
                    formattedMessage = $"{Environment.NewLine}{string.Concat(Enumerable.Repeat(" ", logTypeLength+3+8+5 ))}{indent}{lastLogData.stackTrace}";
                    goto default;
                default:
                    formattedMessage = $"[{lastLogTime:hh:mm:ss}] - {logType} {indent}{lastLogData.message}" + formattedMessage;
                    break; 
            }
            
            // 输出到Unity控制台
            //Debug.Log(formattedMessage);

            // 添加到队列
            logQueue.Enqueue(formattedMessage);

            // 如果队列过大，写入文件并清空队列
            if (logQueue.Count >= MAX_QUEUE_SIZE)
            {
                FlushLogsToFile();
            }
        }

        // 将日志写入文件
        public void FlushLogsToFile()
        {
            if (logQueue.Count == 0) return;

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    while (logQueue.Count > 0)
                    {
                        writer.WriteLine(logQueue.Dequeue());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"写入日志文件失败: {e.Message}");
            }
        }

        // 清空日志文件
        public void ClearLogFile()
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            logQueue.Clear();
        }
    }
}
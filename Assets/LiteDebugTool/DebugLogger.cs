using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LiteGameFrame.CoreInfrastructure;
 

namespace LiteDebugTool
{
    public class DebugLogger : Singleton<DebugLogger>
    {
        public override bool Persistent => true;
        
        public string logName = "GameLog";
        private string logFilePath;
        //Settings
        public bool recordUnityLog = false;
        private const int WRITE_IN_TRIGGER = 100;
        private const int MIN_CACHE_SIZE = 50;
        //Runtime data
        private List<LogEntry> logCache = new List<LogEntry>();
        private DebugNode unityLogNode;

        public void Log(LogEntry logEntry)
        {
            logCache.Add(logEntry);
            if (logCache.Count >= WRITE_IN_TRIGGER)
            {
                FlushLogsToFile();
            }
        }


        public void FlushLogsToFile()
        {
            FlushLogsToFile(false);
        }

        // force=true 时强制写出所有剩余缓存
        public void FlushLogsToFile(bool force)
        {
            if (logCache.Count == 0) return;

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    if (force)
                    {
                        while (logCache.Count > 0)
                        {
                            LogEntry entry = logCache[0];
                            // 查看下一个元素（如果有）来判断是否是最后一个分支
                            bool isLast = logCache.Count > 1 ? (logCache[1].Level < entry.Level) : true;
                            logCache.RemoveAt(0);
                            string log = entry.ToString(isLast);
                            writer.WriteLine(log);
                        }
                    }
                    else
                    {
                        while (logCache.Count > MIN_CACHE_SIZE + 1)
                        {
                            LogEntry entry = logCache[0];
                            logCache.RemoveAt(0);
                            // 现在 logCache[0] 仍然存在（至少剩下 MIN_CACHE_SIZE+1 个）
                            bool isLast = logCache[0].Level < entry.Level;
                            string log = entry.ToString(isLast);
                            writer.WriteLine(log);
                        }
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

            logCache.Clear();
        }

        //unity Log
        private void InitializeUnityLogNode()
        {
            unityLogNode = DebugNode.CreateRoot("UnityLog");
        }
        private void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            if (unityLogNode == null)
                InitializeUnityLogNode();
            unityLogNode.Log(logString, type, stackTrace);
        }

        //life cycle
        protected override void Awake()
        {
            base.Awake();
            logName += DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            logFilePath = Path.Combine(Application.persistentDataPath, logName);
            Debug.Log(logFilePath);
            if (recordUnityLog)
            {
                InitializeUnityLogNode();
                Application.logMessageReceived += HandleUnityLog; 
            }
        }

        private void OnDestroy()
        {
            if (recordUnityLog)
            {
                Application.logMessageReceived -= HandleUnityLog;
                unityLogNode = null;
            }
            // 销毁时强制写出所有剩余缓存
            FlushLogsToFile(true);
        }
    }
}
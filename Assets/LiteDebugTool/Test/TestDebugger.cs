using System;
using UnityEngine;
using LiteGameFrame.CoreInfrastructure;
using UnityEngine.SceneManagement;

namespace LiteDebugTool.Test
{
    public class TestDebugger : MonoBehaviour
    {
        public void Test()
        {
            DebugNode rootNode = DebugNode.CreateRoot("TestThread");
            var childNode = rootNode.CreateChild("SubThread");
            var grandChildNode1 = childNode.CreateChild("SpecificWorker1");
            var grandChildNode2 = childNode.CreateChild("SpecificWorker2");
            
            rootNode.LogBegin("GameLaunch");
            childNode.LogBegin("LoadingScene");
            childNode.LogInfo("加载UI中");
            grandChildNode1.LogBegin("生成'加载中'UI");
            grandChildNode1.LogInfo("生成Prefab");
            GameObject g = Resources.Load<GameObject>("Prefabs/Game");
            try
            {
                g.name = "Game";
            }
            catch (Exception e)
            {
                grandChildNode1.LogError("未找到Prefab文件！", e.StackTrace,  LogType.Exception);
            }
            grandChildNode1.LogEnd("UI生成完毕");
            grandChildNode2.LogBegin("准备场景资源");
            try
            {
                SceneManager.LoadSceneAsync("1111");
            }
            catch (Exception e)
            {
                grandChildNode2.LogError("加载场景失败", e.StackTrace, LogType.Exception);
            }
            grandChildNode2.LogEnd("加载场景完毕");
            childNode.LogEnd("准备完毕");
            rootNode.LogInfo("开始游戏");
            rootNode.LogEnd("游戏停止");
            DebugLogger.Instance.FlushLogsToFile();
        }
    }
}
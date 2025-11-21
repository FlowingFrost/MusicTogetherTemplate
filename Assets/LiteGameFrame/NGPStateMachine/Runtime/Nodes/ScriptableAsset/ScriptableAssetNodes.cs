using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// 示例 ScriptableObject 数据
    /// 用于演示 ScriptableAssetNode 的使用
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameData", menuName = "State Machine/Example/Game Data")]
    public class ExampleGameData : ScriptableObject
    {
        public string levelName;
        public int requiredScore;
        public float timeLimit;
        public bool enableHardMode;
        
        public void PrintInfo()
        {
            Debug.Log($"[GameData] Level: {levelName}, Score: {requiredScore}, Time: {timeLimit}s, HardMode: {enableHardMode}");
        }
    }
    
    /// <summary>
    /// 加载游戏数据节点
    /// 从 ScriptableObject 读取数据并写入黑板
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/ScriptableAsset/Load Game Data")]
    public class LoadGameDataNode : ScriptableAssetNode<ExampleGameData>
    {
        [Tooltip("是否打印数据信息")]
        public bool printInfo = true;
        
        [Tooltip("是否写入黑板")]
        public bool writeToBlackboard = true;
        
        public override string name => "Load Game Data";
        public override Color color => new Color(0.6f, 0.4f, 0.8f); // 紫色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetAsset(out var gameData))
            {
                TriggerSignal();
                StopRunning();
                return;
            }
            
            if (printInfo)
            {
                gameData.PrintInfo();
            }
            
            if (writeToBlackboard)
            {
                StateMachine.Set("levelName", gameData.levelName);
                StateMachine.Set("requiredScore", gameData.requiredScore);
                StateMachine.Set("timeLimit", gameData.timeLimit);
                StateMachine.Set("enableHardMode", gameData.enableHardMode);
                
                Debug.Log($"[LoadGameDataNode] Wrote data to blackboard");
            }
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[LoadGameDataNode] Force stopped");
            StopRunning();
        }
    }
    
    /// <summary>
    /// 通用 ScriptableObject 加载节点
    /// 可以加载任何 ScriptableObject 并将其引用存入黑板
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/ScriptableAsset/Load Asset")]
    public class LoadScriptableAssetNode : ScriptableAssetNode<ScriptableObject>
    {
        [Tooltip("存入黑板的键名")]
        public string blackboardKey = "loadedAsset";
        
        public override string name => "Load Asset";
        public override Color color => new Color(0.6f, 0.4f, 0.8f); // 紫色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetAsset(out var scriptableAsset))
            {
                TriggerSignal();
                StopRunning();
                return;
            }
            
            if (string.IsNullOrEmpty(blackboardKey))
            {
                Debug.LogError($"[LoadScriptableAssetNode] Blackboard key is empty!");
                TriggerSignal();
                StopRunning();
                return;
            }
            
            StateMachine.Set(blackboardKey, scriptableAsset);
            Debug.Log($"[LoadScriptableAssetNode] Loaded {scriptableAsset.name} to blackboard key '{blackboardKey}'");
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[LoadScriptableAssetNode] Force stopped");
            StopRunning();
        }
    }
}


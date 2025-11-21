using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    /// <summary>
    /// GameObject 激活节点
    /// 用于激活或禁用 GameObject
    /// </summary>
    [Serializable, NodeMenuItem("State Machine/Component/GameObject SetActive")]
    public class GameObjectSetActiveNode : ComponentNode<GameObject>
    {
        [Input("Active")]
        [Tooltip("是否激活")]
        public bool active = true;
        
        public override string name => "GameObject SetActive";
        public override Color color => new Color(0.5f, 0.8f, 0.4f); // 绿色
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetBoundComponent(out var gameObject))
                return;
            
            gameObject.SetActive(active);
            Debug.Log($"[GameObjectSetActiveNode] Set {gameObject.name}.SetActive({active})");
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            Debug.Log($"[GameObjectSetActiveNode] Force stopped");
            StopRunning();
        }
    }
}


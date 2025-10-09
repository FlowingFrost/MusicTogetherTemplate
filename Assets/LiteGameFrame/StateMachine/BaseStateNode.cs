using System.Collections.Generic;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    public class BaseStateNode : IStateNode
    {
        public IStateMachine StateMachine { get; private set; }
        public StateNodeData Data { get; set; }
        public IState State { get; set; }

        public bool BuildNode(IStateMachine stateMachine, StateNodeData data, IState state)
        {
            StateMachine = stateMachine;
            Data = data;
            State = state;
            if (StateMachine == null || Data.NodeId == null || State == null)
            {
                Debug.LogWarning($"Failed to build state node: {data.NodeId}");
                return false;
            }
            return true;
        }

        public virtual void OnEnter(string sourceId)
        {
            //Default Action Was Launch the State
            if (!StateMachine.StartState(this, sourceId))
                Debug.LogWarning($"Failed to start state: {Data.NodeId}");
        }
        public virtual void OnComplete()
        {
            if (!StateMachine.CompleteState(this))
                Debug.LogWarning($"Failed to complete state: {Data.NodeId}");
        }
    }
}
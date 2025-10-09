using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    //State
    public interface IState
    {
        string Description { get; }
        void Enter(IStateNode stateNode);
        void Update();
        void Exit();
    }

    //State Machine
    public interface IStateMachine
    {
        StateGraph stateGraph { get; }
        List<BindingData> bindings { get; }
        //public Dictionary
        bool StartState();
        bool StartState(IStateNode stateNode, string sourceId);
        bool CompleteState();
        bool CompleteState(IStateNode stateNode);
        bool Find<T>(string key,out Type valueType);
        bool Get<T>(string key, out T value);
        bool Set<T>(string key, T value);
        List<IStateNode> CurrentStateNodes { get; }
    }

    //State Node
    public interface IStateNode
    {
        StateNodeData Data { get; set; }
        IState State { get; set; }
        bool BuildNode(IStateMachine stateMachine, StateNodeData data, IState state);
        void OnEnter(string sourceId);
        void OnComplete();
    }
    //Graph Data
    [Serializable]
    public struct BindingData
    {
        public string NodeId;
        [ReadOnly(true)] public string typeAssemblyQualified;
        public object target;
    }
}

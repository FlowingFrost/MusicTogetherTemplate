using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    public class StateMachine : MonoBehaviour, IStateMachine
    {
        public StateGraph stateGraph { get; private set; }
        public List<BindingData> bindings { get; private set; } = new List<BindingData>();

        private Dictionary<string, IStateNode> stateNodes;
        public List<IStateNode> CurrentStateNodes { get; private set; } = new List<IStateNode>();
        private bool isProcessing;
        private readonly Dictionary<string, object> _data = new();
        //internal function
        private void BuildGraph()
        {
            stateNodes = new Dictionary<string, IStateNode>();
            foreach (var node in stateGraph.stateNodes)
            {
                var data = bindings.Find(binding => binding.NodeId == node.NodeId);
                if (data.NodeId != null)
                {
                    IStateNode stateNode = (IStateNode)Activator.CreateInstance(Type.GetType(data.typeAssemblyQualified));
                    if (stateNode.BuildNode(this as IStateMachine, node, data.target as IState))
                    {
                        stateNodes.Add(stateNode.Data.NodeId, stateNode);
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid StateNode created: {node.NodeId}");
                    }
                }
            }
        }
        private bool Validate()
        {
            if (!isProcessing)
            {
                Debug.LogWarning("Access denied. StateMachine is not processing.");
                return false;
            }
            return true;
        }


        public bool StartState()
        {
            isProcessing = true;
            if (stateNodes.TryGetValue(stateGraph.entryNodeId, out var startNode))
            {
                StartState(startNode, "Root");
                return true;
            }
            else
            {
                Debug.LogWarning($"Entry State not found: {stateGraph.entryNodeId}");
            }
            return false;
        }

        public bool StartState(IStateNode stateNode, string sourceId)
        {
            if (!Validate())
                return false;
            stateNode.OnEnter(sourceId);
            CurrentStateNodes.Add(stateNode);
            return true;
        }

        public bool CompleteState()
        {
            isProcessing = false;
            foreach (var stateNode in CurrentStateNodes)
            {
                CompleteState(stateNode);
            }
            return true;
        }

        public bool CompleteState(IStateNode stateNode)
        {
            CurrentStateNodes.Remove(stateNode);
            stateNode.State.Exit();
            if (!isProcessing)
            {
                foreach(var node in stateNode.Data.SuccessorIds)
                {
                    if (stateNodes.TryGetValue(node, out var successor))
                    {
                        successor.OnEnter(stateNode.Data.NodeId);
                    }
                }
            }
            return true;
        }

        public bool Find<T>(string key, out System.Type valueType)
        {
            if (_data.TryGetValue(key, out var value) && value is T)
            {
                valueType = typeof(T);
                return true;
            }
            valueType = null;
            return false;
        }

        public bool Get<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out var obj) && obj is T)
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool Set<T>(string key, T value)
        {
            if (_data.ContainsKey(key))
            {
                _data[key] = value;
                return true;
            }
            else
            {
                _data[key] = value;
                return true;
            }
            //return false;
        }




        //Processer
        void Awake()
        {
            BuildGraph();
        }

        void Update()
        {
            if (!isProcessing) return;
            foreach (var stateNode in CurrentStateNodes)
            {
                stateNode.State.Update();
            }
        }
    }
}
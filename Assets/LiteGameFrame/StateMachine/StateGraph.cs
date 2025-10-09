using System.Collections.Generic;
using UnityEngine;

namespace LiteGameFrame.StateMachine
{
    public struct StateNodeData
    {
        public string NodeId { get; }
        public List<string> PredecessorIds { get; }
        public List<string> SuccessorIds { get; }
        public string LaunchCondition { get; }
    }
    public struct StateNodeEdge
    {
        public string SourceId { get; }
        public string TargetId { get; }

        public StateNodeEdge(string sourceId, string targetId)
        {
            SourceId = sourceId;
            TargetId = targetId;
        }
    }

    [CreateAssetMenu(fileName = "NewStateGraph", menuName = "State Machine/State Graph")]
    public class StateGraph : ScriptableObject
    {
        [SerializeField]
        public List<StateNodeData> stateNodes = new List<StateNodeData>();
        public List<StateNodeEdge> stateNodeEdges = new List<StateNodeEdge>();
        public string entryNodeId;


        //internal functions
        private void CheckEdge(string from,string to)
        {
            if (!stateNodeEdges.Exists(edge => edge.SourceId == from && edge.TargetId == to))
            {
                stateNodeEdges.Add(new StateNodeEdge(from, to));
            }
        }

        private void CheckNode(string from, string to)
        {
            if (!stateNodes.Exists(node => node.NodeId == from) || !stateNodes.Exists(node => node.NodeId == to))
            {
                Debug.LogWarning($"StateNode(s) not found: {from}, {to}");
            }
            var fromNode = stateNodes.Find(node => node.NodeId == from);
            if (!fromNode.SuccessorIds.Contains(to))
            {
                fromNode.SuccessorIds.Add(to);
            }
            var toNode = stateNodes.Find(node => node.NodeId == to);
            if (!toNode.PredecessorIds.Contains(from))
            {
                toNode.PredecessorIds.Add(from);
            }
        }

        public void AddNode(StateNodeData nodeData)
        {
            stateNodes.Add(nodeData);
        }

        public void AddEdge(StateNodeEdge edge)
        {
            stateNodeEdges.Add(edge);
        }

        public bool FindNode(string nodeId, out StateNodeData nodeData)
        {
            nodeData = stateNodes.Find(node => node.NodeId == nodeId);
            return nodeData.NodeId != null;
        }

        public void OrganizeNodes()
        {
            foreach (var node in stateNodes)
            {
                foreach (string predecessorId in node.PredecessorIds)
                {
                    CheckEdge(predecessorId, node.NodeId);
                }

                foreach (string successorId in node.SuccessorIds)
                {
                    CheckEdge(node.NodeId, successorId);
                }
            }
            foreach (var edge in stateNodeEdges)
            {
                CheckNode(edge.SourceId, edge.TargetId);
            }
        }
    }
}
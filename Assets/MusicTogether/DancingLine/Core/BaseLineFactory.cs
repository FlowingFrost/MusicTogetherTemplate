using System;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseLineFactory : MonoBehaviour, ILineFactory
    {
        public Type NodeType;
        public Type TailType;

        public virtual bool NewNode(out ILineNode node)
        {
            if (NodeType != null && typeof(ILineNode).IsAssignableFrom(NodeType))
            {
                node = (ILineNode) Activator.CreateInstance(NodeType);
                if (node.AssignTailType(TailType))
                {
                    return true;
                }
                else
                {
                    Debug.LogError($"无法为节点分配线尾类型 {TailType}");
                }
            }
            else
            {
                Debug.LogError($"节点类型 {NodeType} 无效：未实现 ILineNode 接口");
            }
            node = null;
            return false;
        }
    }
}
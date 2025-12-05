using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /*public abstract class BaseLineFactory : SerializedMonoBehaviour, ILineFactory
    {
        [ValueDropdown(nameof(GetNodeTypeOptions))]
        public Type NodeType;

        [ValueDropdown(nameof(GetTailTypeOptions))]
        public Type TailType;

        // 获取节点类型选项（最简单的实现）
        private List<Type> GetNodeTypeOptions()
        {
            var types = new List<Type>();
            types.Add(null);// 添加 null 选项
            // 查找所有实现 ILineNode 的类
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(ILineNode).IsAssignableFrom(type) &&
                            !type.IsAbstract && type.IsClass)
                        {
                            types.Add(type);
                        }
                    }
                }
                catch { }// 忽略无法加载的程序集
            }
            return types;
        }
        private List<Type> GetTailTypeOptions()
        {
            var types = new List<Type>();
            types.Add(null);// 添加 null 选项
            // 查找所有实现 ILineNode 的类
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (typeof(ILineTail).IsAssignableFrom(type) &&
                            !type.IsAbstract && type.IsClass)
                        {
                            types.Add(type);
                        }
                    }
                }
                catch { }// 忽略无法加载的程序集
            }
            return types;
        }

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
    }*/
}
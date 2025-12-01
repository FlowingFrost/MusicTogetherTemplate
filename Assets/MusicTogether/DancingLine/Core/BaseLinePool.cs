using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 线容器具体实现。存储所有的输入信息，提供最新的位置。
    /// 使用泛型参数支持不同类型的节点和线尾，无需进一步抽象。
    /// </summary>
    public class BaseLinePool : MonoBehaviour, ILinePool
    {
        [SerializeField]private ILineFactory _lineFactory;
        internal readonly List<ILineNode> LineNodes = new List<ILineNode>();
        internal readonly List<ILineNode> PendingNodes = new List<ILineNode>();
        internal bool _dirty = false;

        //内部工具
        /// <summary>
        /// 列表 pending units 中有待添加的新节点，此处进行验证并添加。
        /// </summary>
        internal virtual void Validate()
        {
            if (!_dirty) return;
            if (PendingNodes.Count == 0) { _dirty = false; return; }
            //没有待添加节点，直接返回
            
            PendingNodes.Sort((a, b) => a.BeginTime.CompareTo(b.BeginTime));
            int insertIndex = 0;
            
            //准备插入节点
            if (LineNodes.Count == 0)
            {
                LineNodes.AddRange(PendingNodes);
                PendingNodes.Clear();
                _dirty = false;
                return;
            }//最简单的情况，当前列表为空，直接添加
            if (PendingNodes[0].BeginTime >= LineNodes[^1].BeginTime)
            {
                insertIndex = LineNodes.Count;
                LineNodes.AddRange(PendingNodes);
                PendingNodes.Clear();
            }//次之简单的情况，直接添加到末尾
            else
            {
                insertIndex = LineNodes.BinarySearch(PendingNodes[0], Comparer<ILineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                if (insertIndex < 0) insertIndex = ~insertIndex;
                
                int insertIndexInLoop = 0;
                while (PendingNodes.Count > 0)
                {
                    insertIndexInLoop = LineNodes.BinarySearch(PendingNodes[0], Comparer<ILineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                    if (insertIndexInLoop < 0) insertIndexInLoop = ~insertIndexInLoop;
                    LineNodes.Insert(insertIndex, PendingNodes[0]);
                    PendingNodes.RemoveAt(0);
                }
            }//否则用二分查找添加到中间

            Vector3 newPosition = Vector3.zero;
            if(insertIndex - 1 >= 0) newPosition = LineNodes[insertIndex - 1].UpdatePosition(LineNodes[insertIndex].BeginTime);
            for(int i = insertIndex; i < LineNodes.Count - 1; i++)
            {
                LineNodes[i].AdjustNode(newPosition);
                newPosition = LineNodes[i].UpdatePosition(LineNodes[i+1].BeginTime);
            }
            LineNodes[^1].AdjustNode(newPosition);
            _dirty = false;
            //将所有插入的节点更新数据
        }

        //外部工具
        public virtual void AddNode(double time, IDirection direction)
        { 
            _lineFactory.NewNode(out var node);
            node.Init(time, direction.DirectionVector);
            PendingNodes.Add(node);
            _dirty = true;
        }
        /*public virtual void AddNode(List<ILineNode> units)
        {
            PendingNodes.AddRange(units);
            _dirty = true;
        }*/
        
        //生命周期
        public virtual Vector3 GetPosition(double time)
        {
            Validate();
            ILineNode targetUnit = LineNodes[^1];
            return targetUnit.UpdatePosition(time);
        }
    }
}
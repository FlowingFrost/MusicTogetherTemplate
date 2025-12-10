using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Basic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    /// <summary>
    /// 线容器具体实现。存储所有的输入信息，提供最新的位置。
    /// 使用泛型参数支持不同类型的节点和线尾，无需进一步抽象。
    /// </summary>
    public class BaseLinePool : SerializedMonoBehaviour, ILinePool
    {
        //[SerializeField]internal ILineFactory _lineFactory;
        public ILineController LineController;
        public Transform tailHolder;
        [SerializeField, Required] internal GameObject lineTailPrefab;
        public virtual int CurrentIndex => lineNodes.Count - 1;
        public List<ILineNode> LineNodes => lineNodes;
        internal readonly List<ILineNode> lineNodes = new List<ILineNode>();
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
            if (lineNodes.Count == 0)
            {
                lineNodes.AddRange(PendingNodes);
                PendingNodes.Clear();
                _dirty = false;
                return;
            }//最简单的情况，当前列表为空，直接添加
            if (PendingNodes[0].BeginTime >= lineNodes[^1].BeginTime)
            {
                insertIndex = lineNodes.Count;
                lineNodes.AddRange(PendingNodes);
                PendingNodes.Clear();
            }//次之简单的情况，直接添加到末尾
            else
            {
                insertIndex = lineNodes.BinarySearch(PendingNodes[0], Comparer<ILineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                if (insertIndex < 0) insertIndex = ~insertIndex;
                
                int insertIndexInLoop = 0;
                while (PendingNodes.Count > 0)
                {
                    insertIndexInLoop = lineNodes.BinarySearch(PendingNodes[0], Comparer<ILineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                    if (insertIndexInLoop < 0) insertIndexInLoop = ~insertIndexInLoop;
                    lineNodes.Insert(insertIndex, PendingNodes[0]);
                    PendingNodes.RemoveAt(0);
                }
            }//否则用二分查找添加到中间

            Vector3 newPosition = Vector3.zero;
            if(insertIndex - 1 >= 0) newPosition = lineNodes[insertIndex - 1].UpdatePosition(lineNodes[insertIndex].BeginTime);
            for(int i = insertIndex; i < lineNodes.Count - 1; i++)
            {
                lineNodes[i].AdjustNode(newPosition);
                newPosition = lineNodes[i].UpdatePosition(lineNodes[i+1].BeginTime);
            }
            lineNodes[^1].AdjustNode(newPosition);
            _dirty = false;
            //将所有插入的节点更新数据
        }

        public virtual void AddNode(double time)
        {
            if (lineNodes.Count == 0)
            {
                Debug.LogError("线条池中无节点，无法添加仅含时间的节点，请使用AddNode(double time, IDirection direction)方法");
                return;
            }
            if (LineController.GetDirectionByID(lineNodes[CurrentIndex].Direction.NextDirectionID, out var newDirection))
            {
                AddNode(time, newDirection);
                Debug.Log($"根据ID{CurrentIndex}（时间：{lineNodes[CurrentIndex].BeginTime}方向：{lineNodes[CurrentIndex].Direction.ID}）成功获取下一个方向，添加节点，时间：{time}方向：{newDirection.ID}");
            }
            else
            {
                Debug.LogError("无法根据ID获取下一个方向，添加节点失败");
            }
        }
        //外部工具
        public virtual void AddNode(double time, IDirection direction)
        {
            var nodeObj = Instantiate(lineTailPrefab, tailHolder, false);
            //nodeObj.transform.localRotation = Quaternion.LookRotation(direction.DirectionVector);
            var node = nodeObj.GetComponent<ILineNode>();
            node.Init(time, direction);
            PendingNodes.Add(node);
            _dirty = true;
        }
        
        public void ClearLaterNodes(double time)
        {
            Validate();
            for (int i = lineNodes.Count - 1; i >= 0; i--)
            {
                if (lineNodes[i].BeginTime >= time)
                {
                    lineNodes[i].DeleteNode();
                    lineNodes.RemoveAt(i);
                }
            }
        }
        
        //生命周期
        public virtual Vector3 GetPosition(double time)
        {
            Validate();
            ILineNode targetUnit = lineNodes[^1];
            return targetUnit.UpdatePosition(time);
        }
    }
}
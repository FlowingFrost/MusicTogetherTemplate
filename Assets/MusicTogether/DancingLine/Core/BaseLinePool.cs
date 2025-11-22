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
    /// <typeparam name="TNode">基本输入节点</typeparam>
    /// <typeparam name="TTail">基本线尾</typeparam>
    public class BaseLinePool<TNode,TTail> 
        where TNode : BaseLineNode<TTail> 
        where TTail : BaseLineTail
    {
        [ShowInInspector]internal readonly List<TNode> _units = new List<TNode>();
        internal readonly List<TNode> _pendingUnits = new List<TNode>();
        internal bool _dirty = false;

        //内部工具
        /// <summary>
        /// 列表 pending units 中有待添加的新节点，此处进行验证并添加。
        /// </summary>
        internal virtual void Validate()
        {
            if (!_dirty) return;
            if (_pendingUnits.Count == 0) { _dirty = false; return; }
            //没有待添加节点，直接返回
            
            _pendingUnits.Sort((a, b) => a.BeginTime.CompareTo(b.BeginTime));
            int insertIndex = 0;
            
            //准备插入节点
            if (_units.Count == 0)
            {
                _units.AddRange(_pendingUnits);
                _pendingUnits.Clear();
                _dirty = false;
                return;
            }//最简单的情况，当前列表为空，直接添加
            if (_pendingUnits[0].BeginTime >= _units[^1].BeginTime)
            {
                insertIndex = _units.Count;
                _units.AddRange(_pendingUnits);
                _pendingUnits.Clear();
            }//次之简单的情况，直接添加到末尾
            else
            {
                insertIndex = _units.BinarySearch(_pendingUnits[0], Comparer<TNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                if (insertIndex < 0) insertIndex = ~insertIndex;
                
                int insertIndexInLoop = 0;
                while (_pendingUnits.Count > 0)
                {
                    insertIndexInLoop = _units.BinarySearch(_pendingUnits[0], Comparer<TNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                    if (insertIndexInLoop < 0) insertIndexInLoop = ~insertIndexInLoop;
                    _units.Insert(insertIndex, _pendingUnits[0]);
                    _pendingUnits.RemoveAt(0);
                }
            }//否则用二分查找添加到中间

            Vector3 newPosition = Vector3.zero;
            if(insertIndex - 1 >= 0) newPosition = _units[insertIndex - 1].GetNodePosition(_units[insertIndex].BeginTime);
            for(int i = insertIndex; i < _units.Count - 1; i++)
            {
                _units[i].SetBeginPosition(newPosition);
                newPosition = _units[i].GetNodePosition(_units[i+1].BeginTime);
            }
            _units[^1].SetBeginPosition(newPosition);
            _dirty = false;
            //将所有插入的节点更新数据
        }

        //外部工具
        public virtual void AddNode(TNode unit)
        {
            _pendingUnits.Add(unit);
            _dirty = true;
        }
        public virtual void AddNode(List<TNode> units)
        {
            _pendingUnits.AddRange(units);
            _dirty = true;
        }
        
        //生命周期
        public virtual Vector3 GetPosition(double time)
        {
            Validate();
            TNode targetUnit = _units[^1];
            return targetUnit.UpdateNode(time);
        }
    }
}
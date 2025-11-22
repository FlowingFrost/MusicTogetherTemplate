using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingLine
{
    /// <summary>
    /// 基本线尾
    /// </summary>
    public abstract class BaseTail
    {
        public abstract BaseTail NewTail(Vector3 beginPosition, Vector3 directionVector);
        public abstract void UpdateTail(float length);
    }
    
    /// <summary>
    /// 基本输入节点信息 注：节点内声明目标线尾
    /// </summary>
    /// <typeparam name="T">基本线尾</typeparam>
    public abstract class BaseNode<T> where T : BaseTail
    {
        public double BeginTime;
        public Vector3 BeginPosition;
        public Vector3 DirectionVector;
        public T Tail;

        protected BaseNode(double beginTime, Vector3 beginPosition, Vector3 directionVector)
        {
            this.BeginTime = beginTime;
            this.BeginPosition = beginPosition;
            this.DirectionVector = directionVector;
        }
        public abstract Vector3 UpdateNode(double time);
        public abstract void DeleteNode();
    }

    /// <summary>
    /// 线容器。存储所有的输入信息，提供最新的位置。
    /// </summary>
    /// <typeparam name="TNode">基本输入节点</typeparam>
    /// <typeparam name="TTail">基本线尾</typeparam>
    public class LinePool<TNode,TTail> where TNode : BaseNode<TTail> where TTail : BaseTail
    {
        private readonly List<TNode> _units = new List<TNode>();
        private readonly List<TNode> _pendingUnits = new List<TNode>();
        private bool _dirty = false;

        //内部工具
        /// <summary>
        /// 列表 pending units 中有待添加的新节点，此处进行验证并添加。
        /// </summary>
        private void Validate()
        {
            if (!_dirty) return;
            if (_pendingUnits.Count == 0) { _dirty = false; return; }
            //没有待添加节点，直接返回
            
            _pendingUnits.Sort((a, b) => a.BeginTime.CompareTo(b.BeginTime));
            int insertIndex = 0;
            //准备插入节点
            if (_pendingUnits[0].BeginTime >= _units[^1].BeginTime)
            {
                insertIndex = _units.Count;
                _units.AddRange(_pendingUnits);
                _pendingUnits.Clear();
            }//最简单的情况，直接添加到末尾
            else
            {
                insertIndex = _units.BinarySearch(_pendingUnits[0], Comparer<TNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                if (insertIndex < 0) insertIndex = ~insertIndex;
                
                int insertIndexInLoop = 0;
                while (_pendingUnits.Count > 0);
                {
                    insertIndexInLoop = _units.BinarySearch(_pendingUnits[0], Comparer<TNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                    if (insertIndexInLoop < 0) insertIndexInLoop = ~insertIndexInLoop;
                    _units.Insert(insertIndex, _pendingUnits[0]);
                    _pendingUnits.RemoveAt(0);
                }
            }//否则用二分查找添加到中间
            
            for(int i = insertIndex; i < _units.Count - 1; i++)
            {
                _units[i].UpdateNode(_units[i+1].BeginTime);
            }//将所有插入的节点更新数据
        }

        //外部工具
        public void AddUnit(TNode unit)
        {
            _pendingUnits.Add(unit);
            _dirty = true;
        }
        public void AddUnit(List<TNode> units)
        {
            _pendingUnits.AddRange(units);
            _dirty = true;
        }
        
        //生命周期
        public Vector3 GetPosition(double time)
        {
            Validate();
            TNode lastUnit = _units[^1];
            return lastUnit.UpdateNode(time);
        }
    }
}
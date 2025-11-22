using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class LinePool : BaseLinePool<LineNode,LineTail>
    {
        public Transform tailHolder;
        internal int CurrentIndex = 0;
        
        
        internal override void Validate()
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
                insertIndex = _units.BinarySearch(_pendingUnits[0], Comparer<LineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                if (insertIndex < 0) insertIndex = ~insertIndex;
                
                int insertIndexInLoop = 0;
                while (_pendingUnits.Count > 0)
                {
                    insertIndexInLoop = _units.BinarySearch(_pendingUnits[0], Comparer<LineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
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
            CurrentIndex = insertIndex;
            _dirty = false;
            //将所有插入的节点更新数据
        }
        
        public void AddNode(double time, Vector3 directionVector)
        {
            LineNode newNode = new LineNode(time, directionVector, tailHolder);
            AddNode(newNode);
        }
        
        public override Vector3 GetPosition(double time)
        {
            Validate();
            if (_units.Count == 0) return Vector3.zero;
            while (CurrentIndex > 0 && _units[CurrentIndex].BeginTime > time)
            {
                _units[CurrentIndex].UpdateNode(time);
                CurrentIndex--;
            }//currentIndex达到目标项或0
            while (CurrentIndex < _units.Count - 1 && _units[CurrentIndex + 1].BeginTime <= time)
            {
                var nextBegin = _units[CurrentIndex].UpdateNode(_units[CurrentIndex + 1].BeginTime);
                _units[CurrentIndex + 1].SetBeginPosition(nextBegin);
                Debug.Log("SetBeginPosition:" + nextBegin);
                CurrentIndex++;
            }//currentIndex达到目标项或末尾
            var targetUnit = _units[CurrentIndex];
            return targetUnit.UpdateNode(time);
        }
    }
}
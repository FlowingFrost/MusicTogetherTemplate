using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class LinePool : BaseLinePool
    {
        public Transform tailHolder;
        internal int CurrentIndex = 0;
        
        
        internal override void Validate()
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
            if(insertIndex - 1 >= 0) newPosition = LineNodes[insertIndex - 1].GetNodePosition(LineNodes[insertIndex].BeginTime);
            for(int i = insertIndex; i < LineNodes.Count - 1; i++)
            {
                LineNodes[i].AdjustNode(newPosition);
                newPosition = LineNodes[i].GetNodePosition(LineNodes[i+1].BeginTime);
            }
            LineNodes[^1].AdjustNode(newPosition);
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
            if (LineNodes.Count == 0) return Vector3.zero;
            while (CurrentIndex > 0 && LineNodes[CurrentIndex].BeginTime > time)
            {
                LineNodes[CurrentIndex].UpdateNode(time);
                CurrentIndex--;
            }//currentIndex达到目标项或0
            while (CurrentIndex < LineNodes.Count - 1 && LineNodes[CurrentIndex + 1].BeginTime <= time)
            {
                var nextBegin = LineNodes[CurrentIndex].UpdateNode(LineNodes[CurrentIndex + 1].BeginTime);
                LineNodes[CurrentIndex + 1].AdjustNode(nextBegin);
                Debug.Log("SetBeginPosition:" + nextBegin);
                CurrentIndex++;
            }//currentIndex达到目标项或末尾
            var targetUnit = LineNodes[CurrentIndex];
            return targetUnit.UpdateNode(time);
        }
    }
}
using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class LinePool : BaseLinePool
    {
        internal int currentIndex = 0;
        public override int CurrentIndex => currentIndex;
        
        internal override void Validate()
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
            currentIndex = insertIndex;
            _dirty = false;
            //将所有插入的节点更新数据
        }
        
        /*public void AddNode(double time, Vector3 directionVector)该方法已迁移至BaseLinePool
        {
            _lineFactory.NewNode(out var newNode);//new LineNode(time, directionVector, tailHolder);
            
            AddNode(newNode);
        }*/
        
        public override Vector3 GetPosition(double time)
        {
            Validate();
            if (lineNodes.Count == 0) return Vector3.zero;
            //currentIndex指向的node超前：隐藏
            while (currentIndex > 0 && lineNodes[currentIndex].BeginTime > time)//currentIndex达到目标项或0
            {
                lineNodes[currentIndex].SetActive(false);
                currentIndex--;
            }
            
            //currentIndex指向的node滞后：显示并更新起点
            while (currentIndex < lineNodes.Count - 1 && lineNodes[currentIndex + 1].BeginTime <= time)//currentIndex达到目标项或末尾
            {
                lineNodes[currentIndex].SetActive(true);
                var nextBegin = lineNodes[currentIndex].UpdatePosition(lineNodes[currentIndex + 1].BeginTime);
                lineNodes[currentIndex + 1].AdjustNode(nextBegin);
                Debug.Log(
                    $"经过已有节点，ID{currentIndex}（时间：{lineNodes[currentIndex].BeginTime}方向：{lineNodes[currentIndex].Direction.ID}）");//("SetBeginPosition:" + nextBegin);
                currentIndex++;
            }
            var targetUnit = lineNodes[currentIndex];
            targetUnit.SetActive(true);
            return targetUnit.UpdatePosition(time);
        }
        
        [Button("清除当前时间点之后的节点")]
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

        public string DebugInformation()
        {
            string NodeInfo(int ID)
            {
                var node = lineNodes[ID];
                return $"ID:{ID}, BeginTime:{node.BeginTime}, Direction:{node.Direction.ID}";
            }
            string info = "节点信息:\n";
            //前后几个node的信息
            for (int i = currentIndex-2; i <= currentIndex + 2; i++)
            {
                if (i >= 0 && i < lineNodes.Count)
                {
                    info += NodeInfo(i) + "\n";
                }
            }
            return info;
        }
    }
}
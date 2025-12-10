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
            base.ClearLaterNodes(time);
            if (currentIndex >= lineNodes.Count)
                currentIndex = Math.Max(0, lineNodes.Count - 1);
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
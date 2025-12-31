using System.Collections.Generic;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Classic
{
    public class ClassicPreview : SerializedMonoBehaviour, ILevelUnion
    {
        //绑定
        public ILevelManager levelManager;
        public Transform lineHeadHolder;
        //[OnValueChanged(nameof(Reset))]
        public Transform lineTailHolder;
        //运行参数
        [ShowInInspector]private double time => levelManager.LevelTime;
        protected int currentIndex = 0;
        
        //运行时数据
        [ShowInInspector]protected readonly List<ILineNode> lineNodes= new List<ILineNode>();
        
        //私有方法
        protected virtual void Reset()
        {
            if (lineTailHolder == null)
                return;
            lineNodes.Clear();
            lineNodes.AddRange(lineTailHolder.GetComponentsInChildren<ILineNode>(true));
            lineNodes.Sort(((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
        }
        protected virtual void GetPosition(double time)
        {
            if (lineNodes.Count == 0)
            {
                lineHeadHolder.gameObject.SetActive(false);
                return;
            }
            if (!lineHeadHolder.gameObject.activeSelf) lineHeadHolder.gameObject.SetActive(true);
            //currentIndex指向的node超前：隐藏
            if (currentIndex >= lineNodes.Count) currentIndex = lineNodes.Count;
            while (currentIndex > 0 && lineNodes[currentIndex].BeginTime > time)//currentIndex达到目标项或0
            {
                if (currentIndex < lineNodes.Count-1)
                    lineNodes[currentIndex].UpdatePosition(lineNodes[currentIndex + 1].BeginTime, out _,time);
                else
                    lineNodes[currentIndex].UpdatePosition(time, out _,time);
                currentIndex--;
            }
            //currentIndex指向的node滞后：显示并更新起点
            while (currentIndex < lineNodes.Count - 1 && lineNodes[currentIndex + 1].BeginTime <= time)//currentIndex达到目标项或末尾
            {
                lineNodes[currentIndex].UpdatePosition(lineNodes[currentIndex + 1].BeginTime, out var nextBegin,time);//获取下一个节点的起点，同时更新当前节点显隐
                //Debug.Log($"nextBegin:{nextBegin}, BeginTime:{lineNodes[currentIndex + 1].BeginTime}  CurrentTime:{time}  ID:{currentIndex}");//("GetNextBegin:" + nextBegin);
                lineNodes[currentIndex + 1].SetBeginPosition(nextBegin);
                /*Debug.Log(
                    $"经过已有节点，ID{currentIndex}（时间：{lineNodes[currentIndex].BeginTime}方向：{lineNodes[currentIndex].Direction.ID}）");//("SetBeginPosition:" + nextBegin);*/
                currentIndex++;
            }
            //Debug.Log($"正在使用节点ID:{currentIndex}  时间：{time}  节点时间：{lineNodes[currentIndex].BeginTime}");//("UseNode:" + currentIndex);
            var targetUnit = lineNodes[currentIndex];
            targetUnit.UpdatePosition(time, out var newPos, time);
            lineHeadHolder.position = newPos;
        }
        public void AwakeUnion()
        {
            Reset();
        }

        public void StartUnion(double startTime = 0d)
        {
            
        }

        public void UpdateUnion()
        {
            GetPosition(time);
        }
    }
}
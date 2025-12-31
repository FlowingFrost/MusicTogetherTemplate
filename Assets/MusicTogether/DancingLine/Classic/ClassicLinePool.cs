using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Basic;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingLine.Classic
{
    /// <summary>
    /// 线容器具体实现。存储所有的输入信息，提供最新的位置。
    /// 使用泛型参数支持不同类型的节点和线尾，无需进一步抽象。
    /// </summary>
    public class ClassicLinePool : SerializedMonoBehaviour, ILinePool
    {
        //绑定
        //public ILevelManager LevelManager => LineComponent.LevelManager;
        public ILineComponent LineComponent;
        private ILineController LineController => LineComponent.Controller;
        public Transform tailHolder;
        [SerializeField, Required] internal GameObject lineTailPrefab;
        //运行参数
        internal int _currentIndex = 0;
        internal ILineNode _currentNode => lineNodes.Count > 0 ? lineNodes[_currentIndex] : null;
        internal bool _dirty = false;
        //数据存储
        internal readonly List<ILineNode> lineNodes = new List<ILineNode>();
        internal readonly List<ILineNode> PendingNodes = new List<ILineNode>();
        //接口
        public int CurrentIndex => _currentIndex;
        public bool IsEmpty => lineNodes.Count == 0 && PendingNodes.Count == 0;
        
        /*public Transform TailHolder
        {
            get
            {
#if UNITY_EDITOR
                if (IsEditorPreviewing)
                    return tailHilderInEditor;
#endif
                return tailHolder;
            }
        }*/
        public IReadOnlyList<ILineNode> LineNodes => lineNodes;

        protected virtual void Validate(double currentTime)
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
                    if (PendingNodes[0].BeginTime > currentTime) break;
                    insertIndexInLoop = lineNodes.BinarySearch(PendingNodes[0], Comparer<ILineNode>.Create((a, b) => a.BeginTime.CompareTo(b.BeginTime)));
                    if (insertIndexInLoop < 0) insertIndexInLoop = ~insertIndexInLoop;
                    lineNodes.Insert(insertIndexInLoop, PendingNodes[0]);
                    PendingNodes.RemoveAt(0);
                }
            }//否则用二分查找添加到中间

            Vector3 newPosition = Vector3.zero;
            if(insertIndex - 1 >= 0) lineNodes[insertIndex - 1].UpdatePosition(lineNodes[insertIndex].BeginTime, out newPosition);
            for(int i = insertIndex; i < lineNodes.Count - 1; i++)
            {
                lineNodes[i].SetBeginPosition(newPosition);
                lineNodes[i].UpdatePosition(lineNodes[i+1].BeginTime, out newPosition);
            }
            lineNodes[^1].SetBeginPosition(newPosition);
            _dirty = false;
            //将所有插入的节点更新数据
        }
        protected virtual bool NextNode(out ILineNode nextNode)
        {
            //Validate();
            if (_currentIndex + 1 < lineNodes.Count)
            {
                nextNode = lineNodes[_currentIndex + 1];
                return true;
            }
            nextNode = null;
            return false;
        }
        public void Init(double time, IDirection direction, MotionType nodeType)
        {
            ClearLaterNodes(null);
            AddNodeByBeginTime(time, direction, nodeType);
        }
        public void AddNodeByBeginPoint(Vector3 beginPoint, IDirection direction, MotionType nodeType)
        {
            double beginTime = _currentNode.GetEndTime(beginPoint);
            Debug.Log($"落地检测成功，新的节点时间为{beginTime}，落点为{beginPoint}");
            AddNodeByBeginTime(beginTime, direction, nodeType);
        }
        public virtual void AddNodeByBeginTime(double time, IDirection direction, MotionType nodeType, Vector3? beginVelocity = null, Vector3? acceleration = null)
        {
            string debugInfo = $"添加节点，时间：{time}，方向：{direction.ID}，类型：{nodeType}";
            if (beginVelocity.HasValue && acceleration.HasValue)
            {
                AddNode(time, direction, beginVelocity.Value, acceleration.Value, nodeType);
                Debug.Log(debugInfo + $"，起始速度：{beginVelocity.Value}，加速度：{acceleration.Value}");
                return;
            }
            if (lineNodes.Count == 0)
            {
                AddNode(time, direction, Vector3.zero, Vector3.zero, nodeType);
                Debug.Log(debugInfo + "，线条池中无节点，使用零速度和零加速度初始化");
                return;
            }
            var currentNode = lineNodes[_currentIndex];
            switch (nodeType)
            {
                case MotionType.Falling: //需要把LineComponent里对应的部分删除
                    switch (currentNode.NodeType)
                    {
                        case MotionType.Falling:
                            currentNode.GetEndMotion(time, out var endVelocityFalling, out var accelerationFalling);
                            AddNode(time, direction, beginVelocity ?? endVelocityFalling, acceleration ?? accelerationFalling, nodeType);
                            Debug.Log(debugInfo + $"，初速度：{beginVelocity ?? endVelocityFalling}，加速度：{acceleration ?? accelerationFalling}");
                            break;
                        default:
                            AddNode(time, direction, Vector3.zero, LineComponent.Gravity, nodeType);
                            Debug.Log(debugInfo + "，使用零速度和重力加速度初始化");
                            break;
                    }
                    break;
                case MotionType.FallingToGrounded:
                    if (NextNode(out var nextNode))
                    {
                        nextNode.SetDirection(direction);
                    }
                    break;
                default:
                    AddNode(time, direction, Vector3.zero, Vector3.zero, nodeType);
                    Debug.Log(debugInfo + "，使用零速度和零加速度初始化");
                    break;
            }
        }
        public void AddNode(double time, IDirection direction, Vector3 beginVelocity, Vector3 acceleration, MotionType nodeType)
        {
            var nodeObj = Instantiate(lineTailPrefab, tailHolder, false);
            var node = nodeObj.GetComponent<ILineNode>();
            node.Init(time, direction);
            node.InitMotion(beginVelocity, acceleration, nodeType);
            PendingNodes.Add(node);
            _dirty = true;
        }
        public void ClearLaterNodes(double? time = null)
        {
            if (!time.HasValue)
            {
                Debug.Log("清除所有节点");
                _currentIndex = 0;
                lineNodes.Clear();
                foreach (Transform child in tailHolder.transform)
                {
/*#if UNITY_EDITOR
                    Debug.Log($"清除中，IsEditorPreviewing: {LevelManager.IsEditorPreviewing}， child: {child.name}");
                    if (LevelManager.IsEditorPreviewing)
                        DestroyImmediate(child.gameObject);
                    else
#endif*/
                        Destroy(child.gameObject);
                }
                return;
            }
            
            Validate(time.Value);
            PendingNodes.Clear();
            for (int i = lineNodes.Count - 1; i >= 0; i--)
            {
                if (lineNodes[i].BeginTime >= time.Value)
                {
                    lineNodes[i].DeleteNode();
                    lineNodes.RemoveAt(i);
                }
            }
            if (_currentIndex >= lineNodes.Count)
                _currentIndex = Math.Max(0, lineNodes.Count - 1);
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
            for (int i = _currentIndex-2; i <= _currentIndex + 2; i++)
            {
                if (i >= 0 && i < lineNodes.Count)
                {
                    info += NodeInfo(i) + "\n";
                }
            }
            return info;
        }
        
        //生命周期
        public virtual void GetPosition(double time, out Vector3 position, out Vector3 velocity)
        {
            position = Vector3.zero; velocity = Vector3.zero;
            Validate(time);
            if (lineNodes.Count == 0) return;
            //currentIndex指向的node超前：隐藏
            bool currentChanged = false;
            if (_currentIndex >= lineNodes.Count) _currentIndex = lineNodes.Count;
            while (_currentIndex > 0 && lineNodes[_currentIndex].BeginTime > time)//currentIndex达到目标项或0
            {
                if (_currentIndex < lineNodes.Count-1)
                    lineNodes[_currentIndex].UpdatePosition(lineNodes[_currentIndex + 1].BeginTime, out _, time);
                else
                    lineNodes[_currentIndex].UpdatePosition(time, out  _, time);
                _currentIndex--;
                currentChanged = true;
            }
            //currentIndex指向的node滞后：显示并更新起点
            while (_currentIndex < lineNodes.Count - 1 && lineNodes[_currentIndex + 1].BeginTime <= time)//currentIndex达到目标项或末尾
            {
                lineNodes[_currentIndex].UpdatePosition(lineNodes[_currentIndex + 1].BeginTime, out var nextBegin, time);//获取下一个节点的起点，同时更新当前节点显隐
                lineNodes[_currentIndex + 1].SetBeginPosition(nextBegin);
                /*Debug.Log(
                    $"经过已有节点，ID{_currentIndex}（时间：{lineNodes[_currentIndex].BeginTime}方向：{lineNodes[_currentIndex].Direction.ID}）");//("SetBeginPosition:" + nextBegin);*/
                _currentIndex++;
                currentChanged = true;
            }
            var targetUnit = lineNodes[_currentIndex];
            if (currentChanged && LineComponent.LevelState == LevelState.Previewing)
            {
                LineComponent.SetCurrentDirection(targetUnit.Direction.ID);
                LineComponent.SetCurrentMotionType(targetUnit.NodeType);
            }
            targetUnit.UpdatePosition(time, out position, out velocity, time);
        }
    }
}
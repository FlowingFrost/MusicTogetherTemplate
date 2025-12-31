using System.Collections.Generic;
using MusicTogether.DancingLine.Classic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MusicTogether.DancingLine.FrostLine
{
    public class MovementArea : SerializedMonoBehaviour, ILinePool
    {
        //绑定
        //预设信息
        [OdinSerialize]protected List<ClassicDirection> Directions = new List<ClassicDirection>();
        //运行参数
        internal int _currentIndex = 0;
        internal ILineNode _currentNode => lineNodes.Count > 0 ? lineNodes[_currentIndex] : null;
        internal bool _dirty = false;
        internal readonly List<ILineNode> lineNodes = new List<ILineNode>();
        internal readonly List<ILineNode> PendingNodes = new List<ILineNode>();
        //接口
        public int CurrentIndex => _currentIndex;
        public bool IsEmpty => lineNodes.Count == 0 && PendingNodes.Count == 0;
        public IReadOnlyList<ILineNode> LineNodes => lineNodes;
        
        public void Init(double time, IDirection direction, MotionType nodeType)
        {
            throw new System.NotImplementedException();
        }

        public void AddNodeByBeginPoint(Vector3 beginPoint, IDirection direction, MotionType nodeType)
        {
            throw new System.NotImplementedException();
        }

        public void AddNodeByBeginTime(double time, IDirection direction, MotionType nodeType, Vector3? beginVelocity = null,
            Vector3? acceleration = null)
        {
            throw new System.NotImplementedException();
        }

        public void AddNode(double time, IDirection direction, Vector3 beginVelocity, Vector3 acceleration, MotionType nodeType)
        {
            throw new System.NotImplementedException();
        }

        public void ClearLaterNodes(double? time)
        {
            throw new System.NotImplementedException();
        }

        public void GetPosition(double time, out Vector3 position, out Vector3 velocity)
        {
            throw new System.NotImplementedException();
        }
    }
}
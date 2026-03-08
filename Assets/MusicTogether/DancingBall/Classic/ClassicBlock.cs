using System.Collections.Generic;
using MusicTogether.DancingBall.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicBlock : MonoBehaviour, IBlock
    {
        [SerializeField] protected ClassicBlockMaker blockMaker;
        protected int indexInRoad;
        
        public IBlockMaker BlockMaker => blockMaker;
        public Transform Transform => transform;
        public int IndexInRoad
        {
            get => indexInRoad;
            set => indexInRoad = value;
        }

        public List<Vector3> GetPositionsInBlock()
        {
            return new List<Vector3>(){ transform.position };
        }
    }
}
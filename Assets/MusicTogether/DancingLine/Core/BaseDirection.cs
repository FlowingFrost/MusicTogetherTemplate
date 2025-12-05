using System;
using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    [Serializable]
    public class BaseDirection : IDirection
    {
        [SerializeField] private int _id;
        [SerializeField] private int _nextDirectionID;
        [SerializeField] private Vector3 _directionVector;
        
        public int ID { get => _id; set => _id = value; }
        public int NextDirectionID { get => _nextDirectionID; set => _nextDirectionID = value; }
        public Vector3 DirectionVector { get => _directionVector; set => _directionVector = value; }
    }
}
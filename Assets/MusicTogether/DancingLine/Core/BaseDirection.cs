using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MusicTogether.DancingLine.Core
{
    [Serializable]
    public class BaseDirection : IDirection
    {
        [FormerlySerializedAs("_id")] [SerializeField] private int id;
        [FormerlySerializedAs("_nextDirectionID")] [SerializeField] private int nextDirectionID;
        [FormerlySerializedAs("_directionVector")] [SerializeField] private Vector3 directionVector;
        
        public int ID { get => id; set => id = value; }
        public int NextDirectionID { get => nextDirectionID; set => nextDirectionID = value; }
        public Vector3 DirectionVector { get => directionVector; set => directionVector = value; }
    }
}
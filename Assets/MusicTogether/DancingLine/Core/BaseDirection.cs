using UnityEngine;

namespace MusicTogether.DancingLine.Core
{
    public abstract class BaseDirection : IDirection
    {
        public int ID { get; set; }
        public int NextDirectionID { get; set; }
        public Vector3 DirectionVector { get; set; }
    }
}
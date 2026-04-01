using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Player
{
    public record MovementData()
    {
        public double Time { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        
        public MovementData(double time, Vector3 position, Quaternion rotation) : this()
        {
            this.Time = time;
            this.Position = position;
            this.Rotation = rotation;
        }
    }
}

using MusicTogether.LevelManagement;
using UnityEngine;

namespace MusicTogether.DancingLine.Interfaces
{
    public interface ILineComponent : ILevelUnion
    {
        //ILinePool Pool { get; }
        //ILineController Controller { get; }
        //IPhysicsDetector PhysicsDetector { get; }
        LevelState LevelState { get; }
        //IDirection CurrentDirection { get; }
        //Vector3 Gravity { get; }
        
        //bool GetDirectionByID(int targetID, out IDirection direction);
        //bool SetCurrentDirection(int targetID);
        void Move();
        void Turn();
        //void Turn(int? newDirectionID);//([CanBeNull] IDirection direction);
        //void OnGroundedChanged(bool grounded, Vector3 groundPoint);
        //void SetCurrentMotionType(MotionType motionType);
        //void OnGravityChanged(Vector3 newGravity);
        void ClearNodesAfterNow();
    }
}
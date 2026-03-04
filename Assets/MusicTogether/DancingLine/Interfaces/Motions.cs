using UnityEngine;

namespace MusicTogether.DancingLine.Interfaces
{
    public record MotionState
    {
        public Transform ParentTransform => SelfTransform.parent;
        public Transform SelfTransform { get; set; }
        public Vector3 WorldSpacePosition => ParentTransform.TransformPoint(ParentSpacePosition);
        public Quaternion WorldSpaceRotation => ParentTransform.rotation * ParentSpaceRotation;
        public Vector3 ParentSpacePosition { get; set; }
        public Quaternion ParentSpaceRotation { get; set; }
        
        public Vector3 ObjectVecToParent(Vector3 objSpaceVec) => ParentSpaceRotation * objSpaceVec;
        public Vector3 ParentVecToWorld(Vector3 parentSpaceVec) => ParentTransform.TransformDirection(parentSpaceVec);
        public Vector3 ObjectVecToWorld(Vector3 objSpaceVec) => ParentVecToWorld(ObjectVecToParent(objSpaceVec));
        public Vector3 WorldVecToParent(Vector3 worldVec) => ParentTransform.InverseTransformDirection(worldVec);
        public Vector3 ParentVecToObject(Vector3 parentSpaceVec) => Quaternion.Inverse(ParentSpaceRotation) * parentSpaceVec;
        public Vector3 WorldVecToObject(Vector3 worldVec) => Quaternion.Inverse(ParentSpaceRotation) * WorldVecToParent(worldVec);
        public Vector3 ParentSpaceUpDirection => ObjectVecToParent(Vector3.up).normalized;
        public Vector3 WorldSpaceUpDirection => ParentTransform.TransformDirection(ParentSpaceUpDirection).normalized;
        
        
        public Vector3 WorldPosToObject(Vector3 worldPos) => ParentVecToObject(ParentTransform.InverseTransformPoint(worldPos) - ParentSpacePosition);//先得到其在parentSpace内的样子，然后减去物体位置得到相对于物体的位移（parentSpace）接下来得到相对于物体的位移
        public Vector3 WorldPosToParent(Vector3 worldPos) => ParentTransform.InverseTransformPoint(worldPos);
        
        public Vector3 ObjectPosToWorld(Vector3 objPos) => ParentPosToWorld(ObjectVecToParent(objPos) + ParentSpacePosition);
        public Vector3 ParentPosToWorld(Vector3 parentPos) => ParentTransform.TransformPoint(parentPos);
    }
    
    public record PhysicsState
    {
        public Vector3 Velocity { get; set; }
        public Vector3 Gravity { get; set; }
        public NodeMotionType NodeMotionType { get; set; }
    }
    
    public record MotionCalculationResult
    {
        public float Delta { get; set; }
        public float T1 { get; set; }
        public float T2 { get; set; }
        public float FinalT { get; set; }
        public Vector3 Displacement { get; set; }
    }
    
    public enum NodeMotionType
    {
        Grounded,
        GroundedToFalling,
        Falling,
        FallingToGrounded
    }

    public enum NodeInputType
    {
        Turn,
        Continue,
        Event,
        Pre
    }
}
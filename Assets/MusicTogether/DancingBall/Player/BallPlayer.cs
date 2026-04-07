using MusicTogether.DancingBall.Scene;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Player
{
    public class BallPlayer : SerializedMonoBehaviour
    {
        public enum PlayerState { }

        public struct DisplacementDebugData
        {
            public bool HasData;
            public float DeltaTimeRatio;
            public float CorrectionT;
            public Vector3 StandardDisplacement;
            public Vector3 ActualDisplacement;
            public Vector3 ActualOnStandardVector;
            public Vector3 ActualOnOrthogonalVector;
            public Vector3 PreviousDataPosition;
            public Vector3 CurrentDataPosition;
            public Vector3 PreviousMotionPosition;
        }

        private const float StandardDisplacementSqrEpsilon = 1e-6f;

        [SerializeField] private ILevelManager levelManager;
        private double Time => levelManager.LevelTime;
        [SerializeField] private IMap map;
        [SerializeField] private float ballRadius;
        [SerializeField] private AnimationCurve motionCorrectionCurve;
        private int currentRoadIndex = 0;
        private int currentDataIndex = 0;
        
        private IRoad CurrentRoad => map.Roads[currentRoadIndex];
        private MovementData CurrentData => CurrentRoad.MovementDatum[currentDataIndex];
        private MovementData previousData, nextData;
        private double CurrentDataTime => CurrentData.Time;
        private double PreviousDataTime => GetPreviousData(out var data)? data.Time : 0;
        private bool IsRoadIndexOutOfRange(int roadIndex) => roadIndex < 0 || roadIndex >= map.Roads.Count;
        private bool IsDataIndexOutOfRange(int dataIndex) => dataIndex < 0 || dataIndex >= CurrentRoad.MovementDatum.Count;

        private double previousMotionPointTime;
        private Vector3 previousMotionPointPosition;
        private Quaternion previousMotionPointRotation;

        private DisplacementDebugData _debugData;

        public bool TryGetDebugData(out DisplacementDebugData data)
        {
            data = _debugData;
            return _debugData.HasData;
        }

        private bool DetectInput() => Input.GetMouseButtonDown(0);

        private void RecordPreviousMotionPoint()
        {
            previousMotionPointTime = Time;
            previousMotionPointPosition = transform.position;
            previousMotionPointRotation = transform.GetChild(0).rotation;
        }
        private bool GetPreviousData(out MovementData data)
        {
            data = null;
            if (IsDataIndexOutOfRange(currentDataIndex - 1))
            {
                if (IsRoadIndexOutOfRange(currentRoadIndex - 1))
                    return false;
                else
                {
                    data = map.Roads[currentRoadIndex - 1].MovementDatum[^1];
                    return true;
                }
            }

            data = map.Roads[currentRoadIndex].MovementDatum[currentDataIndex - 1];
            return true;
        }
        private bool GotoPreviousData()
        {
            if (IsDataIndexOutOfRange(currentDataIndex - 1))
            {
                if (IsRoadIndexOutOfRange(currentRoadIndex - 1))
                    return false;
                else
                {
                    nextData = CurrentData;
                    GetPreviousData(out previousData);
                    
                    currentRoadIndex--;
                    currentDataIndex = map.Roads[currentRoadIndex].MovementDatum.Count - 1;
                    //transform.SetParent(CurrentRoad.Transform);
                    transform.position = CurrentData.GetPlayerPosition(ballRadius);
                    transform.GetChild(0).rotation = CurrentData.GetPlayerRotation();
                    RecordPreviousMotionPoint();
                    return true;
                }
            }

            currentDataIndex--;
            RecordPreviousMotionPoint();
            return true;
        }

        private bool GetNextData(out MovementData data)
        {
            data = null;
            if (IsDataIndexOutOfRange(currentDataIndex + 1))
            {
                if (IsRoadIndexOutOfRange(currentRoadIndex + 1))
                    return false;
                else
                {
                    data = map.Roads[currentRoadIndex + 1].MovementDatum[0];
                    return true;
                }
            }
            data = map.Roads[currentRoadIndex].MovementDatum[currentDataIndex + 1];
            return true;
        }
        private bool GotoNextData()
        {
            if (IsDataIndexOutOfRange(currentDataIndex + 1))
            {
                if (IsRoadIndexOutOfRange(currentRoadIndex + 1))
                    return false;
                else
                {
                    previousData = CurrentData;
                    GetNextData(out nextData);
                    
                    currentRoadIndex++;
                    currentDataIndex = 0;
                    //transform.SetParent(CurrentRoad.Transform);

                    transform.position = CurrentData.GetPlayerPosition(ballRadius);
                    transform.GetChild(0).rotation = CurrentData.GetPlayerRotation();
                    RecordPreviousMotionPoint();
                    return true;
                }
            }

            currentDataIndex++;
            RecordPreviousMotionPoint();
            return true;
        }
        private void UpdateTimeOnPlaying(double currentTime)
        {
            if (DetectInput() && CurrentData.NeedTap)
            {
                if (previousData == null || PreviousDataTime < currentTime)
                    GotoNextData();
                //previousMotionPointTime = PreviousDataTime;
            }
            //超时切换
            else if (currentTime > CurrentDataTime)
            {
                while (currentTime > CurrentDataTime && !CurrentData.NeedTap)// && (!CurrentData.NeedTap || DetectInput()) 
                {
                    if (!GotoNextData()) break;
                }
            }
            /*else if (PreviousDataTime > currentTime)
            {
                while (PreviousDataTime > currentTime)
                {
                    if (!GotoPreviousData()) break;
                }
            }*/
        }
        private void UpdateTimeOnPreview(double currentTime)
        {
            if (currentTime > CurrentDataTime)
            {
                while (currentTime > CurrentDataTime)
                {
                    if (!GotoNextData()) break;
                }
            }
            else if (PreviousDataTime > currentTime)
            {
                while (PreviousDataTime > currentTime)
                {
                    if (!GotoPreviousData()) break;
                }
            }
        }

        private void SetPlayerTransformOnPlaying(double currentTime)
        {
            //if (transform.parent != CurrentRoad.Transform)
                //transform.SetParent(CurrentRoad.Transform);
            double previousTime = previousMotionPointTime;
            double motionTimeLength = 0;
            Vector3 previousPosition = previousMotionPointPosition;
            Quaternion previousRotation = previousMotionPointRotation;
            

            Vector3 nextPosition = Vector3.zero;
            Quaternion nextRotation = Quaternion.identity;
            if (CurrentData != null)
            {
                motionTimeLength = CurrentDataTime - previousTime;
                nextPosition = CurrentData.GetPlayerPosition(ballRadius);
                nextRotation = CurrentData.GetPlayerRotation();
            }

            float deltaTimeRatio = motionTimeLength == 0
                ? 1f
                : (float)((currentTime - previousTime) / motionTimeLength);
            Vector3 currentPosition = Vector3.LerpUnclamped(previousPosition, nextPosition, deltaTimeRatio);
            float deltaTimeRatioClamped = Mathf.Clamp01(deltaTimeRatio);
            
            Quaternion currentRotation = Quaternion.Lerp(previousRotation, nextRotation, motionCorrectionCurve.Evaluate(deltaTimeRatioClamped));
            
            DisplacementDebugData? debugData = null;
            
            GetPreviousData(out previousData);
            if (previousData != null && CurrentData != null)
            {
                

                Vector3 previousDataPosition = previousData.GetPlayerPosition(ballRadius);
                Vector3 currentDataPosition = CurrentData.GetPlayerPosition(ballRadius);
                Vector3 standardDisplacement = currentDataPosition - previousDataPosition;
                Vector3 actualDisplacement = currentDataPosition - previousPosition;

                if (standardDisplacement.sqrMagnitude > StandardDisplacementSqrEpsilon)
                {
                    Vector3 actualDisplacementOnStandardDirection = Vector3.Project(actualDisplacement, standardDisplacement);
                    Vector3 actualDisplacementOnOrthogonalDirection = actualDisplacement - actualDisplacementOnStandardDirection;
                    currentPosition = previousPosition +
                                      actualDisplacementOnStandardDirection * deltaTimeRatio +
                                      actualDisplacementOnOrthogonalDirection *
                                      motionCorrectionCurve.Evaluate(deltaTimeRatioClamped);

                    Vector3 standardDirection = standardDisplacement.normalized;


                    debugData = new DisplacementDebugData
                    {
                        HasData = true,
                        DeltaTimeRatio = deltaTimeRatio,
                        CorrectionT = deltaTimeRatioClamped,
                        StandardDisplacement = standardDisplacement,
                        ActualDisplacement = actualDisplacement,
                        ActualOnStandardVector = actualDisplacementOnStandardDirection,
                        ActualOnOrthogonalVector = actualDisplacementOnOrthogonalDirection,
                        PreviousDataPosition = previousDataPosition,
                        CurrentDataPosition = currentDataPosition,
                        PreviousMotionPosition = previousMotionPointPosition
                    };
                }
                else
                {
                    debugData = new DisplacementDebugData
                    {
                        HasData = false,
                        StandardDisplacement = standardDisplacement,
                        ActualDisplacement = actualDisplacement,
                        DeltaTimeRatio = deltaTimeRatio,
                        CorrectionT = deltaTimeRatioClamped,
                        PreviousDataPosition = previousDataPosition,
                        CurrentDataPosition = currentDataPosition,
                        PreviousMotionPosition = previousMotionPointPosition
                    };
                }
            }
            UpdateDebugData(debugData);
            
            
            
            //deltaTimeRatio = Mathf.Clamp01(deltaTimeRatio);
            
            
            transform.position = currentPosition;
            transform.GetChild(0).rotation = currentRotation;
        }
        private void SetPlayerTransformOnPreview(double currentTime)
        {
            double previousTime = 0;
            Vector3 previousPosition = Vector3.zero;
            Quaternion previousRotation = Quaternion.identity;
            if (previousData != null)
            {
                previousTime = PreviousDataTime;
                previousPosition = previousData.GetPlayerPosition(ballRadius);
                previousRotation = previousData.GetPlayerRotation();
            }
            double motionTimeLength = 0;
            Vector3 nextPosition = Vector3.zero;
            Quaternion nextRotation = Quaternion.identity;
            if (CurrentData != null)
            {
                motionTimeLength = CurrentDataTime - previousTime;
                nextPosition = CurrentData.GetPlayerPosition(ballRadius);
                nextRotation = CurrentData.GetPlayerRotation();
            }
            
            float deltaTimeRatio = motionTimeLength == 0
                ? 1f
                : (float)((currentTime - previousTime) / motionTimeLength);
            Vector3 currentPosition = Vector3.LerpUnclamped(previousPosition, nextPosition, deltaTimeRatio);
            Quaternion currentRotation = Quaternion.LerpUnclamped(previousRotation, nextRotation, deltaTimeRatio);
            transform.position = currentPosition;
            transform.GetChild(0).rotation = currentRotation;
        }

        public void Update()
        {
            switch (levelManager.CurrentLevelState)
            {
                case LevelState.Playing:
                    UpdateTimeOnPlaying(Time);
                    SetPlayerTransformOnPlaying(Time);
                    break;
                case LevelState.Previewing:
                    UpdateTimeOnPreview(Time);
                    SetPlayerTransformOnPreview(Time);
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (map == null || map.Roads == null || map.Roads.Count == 0)
                return;
            if (IsRoadIndexOutOfRange(currentRoadIndex) || IsDataIndexOutOfRange(currentDataIndex))
                return;

            MovementData currentData = CurrentData;
            if (currentData == null)
                return;

            bool hasPreviousData = GetPreviousData(out var gizmoPreviousData);
            Vector3 previousDataPosition = hasPreviousData
                ? gizmoPreviousData.GetPlayerPosition(ballRadius)
                : previousMotionPointPosition;
            Vector3 currentDataPosition = currentData.GetPlayerPosition(ballRadius);
            Vector3 previousMotionPosition = previousMotionPointPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(previousDataPosition, ballRadius * 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(currentDataPosition, ballRadius * 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(previousDataPosition, currentDataPosition);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(previousMotionPosition, currentDataPosition);

            Vector3 standardDisplacement = currentDataPosition - previousDataPosition;
            if (standardDisplacement.sqrMagnitude > StandardDisplacementSqrEpsilon)
            {
                Vector3 actualDisplacement = currentDataPosition - previousMotionPosition;
                Vector3 actualDisplacementOnStandard = Vector3.Project(actualDisplacement, standardDisplacement);
                Vector3 actualDisplacementOnOrthogonal = actualDisplacement - actualDisplacementOnStandard;

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(previousMotionPosition + actualDisplacementOnOrthogonal, previousMotionPosition + actualDisplacementOnOrthogonal + actualDisplacementOnStandard);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(previousMotionPosition, previousMotionPosition + actualDisplacementOnOrthogonal);
            }
        }

        private void UpdateDebugData(DisplacementDebugData? debugData)
        {
            _debugData = debugData ?? new DisplacementDebugData { HasData = false };
        }
    }
}
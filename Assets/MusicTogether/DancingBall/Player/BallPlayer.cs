using MusicTogether.DancingBall.Scene;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Player
{
    public class BallPlayer : SerializedMonoBehaviour
    {
        public enum PlayerState { }

        [SerializeField] private ILevelManager levelManager;
        private double Time => levelManager.LevelTime;
        [SerializeField] private IMap map;
        [SerializeField] private float ballRadius;
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
        private void UpdateTime(double currentTime)
        {
            if (DetectInput() && CurrentData.NeedTap)
            {
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

        private void SetPlayerTransform(double currentTime)
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
            //deltaTimeRatio = Mathf.Clamp01(deltaTimeRatio);
            Vector3 currentPosition = Vector3.LerpUnclamped(previousPosition, nextPosition, deltaTimeRatio);
            Quaternion currentRotation = Quaternion.LerpUnclamped(previousRotation, nextRotation, deltaTimeRatio);
            transform.position = currentPosition;
            transform.GetChild(0).rotation = currentRotation;
        }

        public void Update()
        {
            UpdateTime(levelManager.LevelTime);
            SetPlayerTransform(levelManager.LevelTime);
        }
    }
}
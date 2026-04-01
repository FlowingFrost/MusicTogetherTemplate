using System.Collections.Generic;
using System.Linq;
using MusicTogether.Archived_DancingBall.DancingBall;
using MusicTogether.Archived_DancingBall.Scene;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Player
{
    public class BallPlayer : SerializedMonoBehaviour
    {
        [SerializeField] private ILevelManager levelManager;
        private double Time => levelManager.LevelTime;
        [SerializeField] private Map map;
        private SceneData SceneData => map.SceneData;
        private int currentRoadIndex = -1;
        private int currentBlockIndex = 0;
        
        private List<MovementData> _movementDataQueue = new List<MovementData>();
        private MovementData _currentBeginData;
        private MovementData _currentEndData;
        
        private bool DetectInput()
        {
            return Input.GetMouseButtonDown(0);
        }

        private void NextData(double currentTime)
        {
            if (_movementDataQueue != null && _movementDataQueue.Count > 0)
            {
                _currentBeginData = new MovementData(currentTime, transform.position, transform.rotation);
                _currentEndData = _movementDataQueue[0];
                _movementDataQueue.RemoveAt(0);
            }

            if (SceneData.GetRoadData(currentRoadIndex, out var data))
            {
                if (currentBlockIndex >= data.NoteEndIndex) NextRoad();
            }
            else
            {
                NextRoad();
            }

            if (map.TryGetRoad(currentRoadIndex, out var road))
            {
                SceneData.TryGetNext_Block_InCurrentRoad_WhichNeedTap(currentRoadIndex, currentBlockIndex,
                    out var tapIndex);
                SceneData.TryGetNext_Block_InCurrentRoad_WhichHasDisplacementRule(currentRoadIndex, currentBlockIndex, out var displacementIndex);
                int blockEndIndex = data.NoteEndIndex;
                int targetEndIndex = Mathf.Min(tapIndex, displacementIndex);
                _movementDataQueue.AddRange(road.GetBlockMovementDatas(currentBlockIndex+1, targetEndIndex));
                currentBlockIndex = targetEndIndex;
            }
            else
            {
                NextRoad();
            }
        }

        private void NextRoad()
        {
            currentRoadIndex++;
            currentBlockIndex = -1;
            if (map.TryGetRoad(currentRoadIndex, out var road))
            {
                _currentBeginData = road.GetBlockMovementDatas(0, 1).First();
                _movementDataQueue.AddRange(road.GetBlockMovementDatas(0, 0).Skip(1));
            }
        }
        private void Move(double currentTime)
        {
            if (currentTime > _currentEndData.Time) NextData(currentTime);
            
            double delta = (currentTime - _currentBeginData.Time)/(_currentEndData.Time - _currentBeginData.Time);
            delta = delta < 0 ? 0 : delta;
            transform.position = Vector3.Lerp(_currentBeginData.Position, _currentEndData.Position, (float)delta);
            transform.rotation = Quaternion.Lerp(_currentBeginData.Rotation, _currentEndData.Rotation, (float)delta);
        }

        private void Awake()
        {
            currentRoadIndex = -1;
            NextRoad();
        }

        private void Update()
        {
            if (DetectInput())
            {
                NextData(Time);
            }
            Move(Time);
        }
    }
}
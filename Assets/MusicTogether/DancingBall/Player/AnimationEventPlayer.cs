using System.Collections.Generic;
using MusicTogether.DancingBall.Scene;
using MusicTogether.LevelManagement;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Player
{
	public class AnimationEventPlayer : SerializedMonoBehaviour
	{
		[SerializeField] private ILevelManager levelManager;
		[SerializeField] private IMap map;

		private readonly List<IAnimationEventData> activeEvents = new List<IAnimationEventData>();
		private int[] nextBeginIndexByRoad = new int[0];
		private int[] nextEndIndexByRoad = new int[0];
		private double previousTime;

		private double Time => levelManager != null ? levelManager.LevelTime : 0d;

		private void OnEnable()
		{
			InitializeCursors();
			previousTime = Time;
		}

		private void InitializeCursors()
		{
			activeEvents.Clear();

			if (map == null || map.Roads == null)
				return;

			int roadCount = map.Roads.Count;
			nextBeginIndexByRoad = new int[roadCount];
			nextEndIndexByRoad = new int[roadCount];
		}

		private void ResetState(double currentTime)
		{
			for (int i = activeEvents.Count - 1; i >= 0; i--)
			{
				activeEvents[i].OnEnd(currentTime);
			}
			activeEvents.Clear();
			InitializeCursors();
		}

		public void Update()
		{
			if (map == null || map.Roads == null || levelManager == null)
				return;

			double currentTime = Time;
			if (currentTime < previousTime)
			{
				ResetState(currentTime);
				previousTime = currentTime;
				return;
			}

			for (int roadIndex = 0; roadIndex < map.Roads.Count; roadIndex++)
			{
				IRoad road = map.Roads[roadIndex];
				if (road == null)
					continue;

				int nextBeginIndex = nextBeginIndexByRoad[roadIndex];
				int nextEndIndex = nextEndIndexByRoad[roadIndex];

				if (road.AnimationEventDatum == null || road.AnimationEventDatum.Count == 0)
				{
					nextBeginIndexByRoad[roadIndex] = nextBeginIndex;
					nextEndIndexByRoad[roadIndex] = nextEndIndex;
					continue;
				}

				if (currentTime < road.RoadBeginTime || currentTime > road.RoadEndTime)
				{
					nextBeginIndexByRoad[roadIndex] = nextBeginIndex;
					nextEndIndexByRoad[roadIndex] = nextEndIndex;
					continue;
				}

				while (nextBeginIndex < road.AnimationEventDatum.Count &&
				       road.AnimationEventDatum[nextBeginIndex].BeginTime <= currentTime)
				{
					IAnimationEventData evt = road.AnimationEventDatum[nextBeginIndex];
					evt.OnBegin(currentTime);
					activeEvents.Add(evt);
					nextBeginIndex++;
				}

				while (nextEndIndex < road.AnimationEventDatum.Count &&
				       road.AnimationEventDatum[nextEndIndex].EndTime < currentTime)
				{
					IAnimationEventData evt = road.AnimationEventDatum[nextEndIndex];
					evt.OnEnd(currentTime);
					nextEndIndex++;
				}

				nextBeginIndexByRoad[roadIndex] = nextBeginIndex;
				nextEndIndexByRoad[roadIndex] = nextEndIndex;
			}

			for (int i = activeEvents.Count - 1; i >= 0; i--)
			{
				IAnimationEventData evt = activeEvents[i];
				evt.OnUpdate(currentTime);
				if (!evt.IsActive || currentTime > evt.EndTime)
				{
					evt.OnEnd(currentTime);
					activeEvents.RemoveAt(i);
				}
			}

			previousTime = currentTime;
		}

		public void NotifyBlockClicked(int roadIndex, int dataIndex, double currentTime)
		{
			if (map == null || map.Roads == null)
				return;

			IRoad road = roadIndex >= 0 && roadIndex < map.Roads.Count ? map.Roads[roadIndex] : null;
			if (road == null || road.AnimationEventDatum == null)
				return;

			for (int i = 0; i < road.AnimationEventDatum.Count; i++)
			{
				IAnimationEventData evt = road.AnimationEventDatum[i];
				if (evt.RoadIndex == roadIndex && evt.DataIndex == dataIndex)
				{
					evt.OnClicked(currentTime);
					break;
				}
			}
		}
	}
}

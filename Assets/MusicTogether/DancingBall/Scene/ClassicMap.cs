using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.EditorTool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Scene
{
    public class ClassicMap : SerializedMonoBehaviour, IMap
    {
        [Header("References")]
        [SerializeField] private EditManager editManager;
        [SerializeField] private Factory factory;
        [SerializeField] private SceneData sceneData;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private readonly List<IRoad> roads = new List<IRoad>();

        public Transform Transform => transform;
        public EditManager EditManager => editManager;
        public SceneData SceneData => sceneData;
        public List<IRoad> Roads => roads;
        public bool IsDataValid => sceneData != null && editManager != null && factory != null;

        //操作功能
        [Button]
        public void RebuildRoads()
        {
            var roadDataList = sceneData.roadDataList;
            // 去重：保留 blocks 最多的
            var duplicateRoads = roads
                .GroupBy(r => r.RoadData?.roadName)
                .Where(g => g.Key != null && g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(r => r.Blocks.Count).Skip(1))
                .ToList();
            RemoveRoads(duplicateRoads);

            // 移除无效 road（不在数据列表中）
            var validNames = new HashSet<string>(roadDataList.Select(r => r.roadName));
            var roadToRemove = roads.Where(r => r.RoadData == null || !validNames.Contains(r.RoadData.roadName)).ToList();
            RemoveRoads(roadToRemove);
            
            //移除不在roads列表中的road（丢失绑定的子物体）
            var missbindingRoadToRemove = GetComponentsInChildren<IRoad>().Where(r => !roads.Contains(r)).ToList();
            RemoveRoads(missbindingRoadToRemove);
            
            // 添加缺失 road
            var roadToAdd = roadDataList
                .Where(d => !roads.Exists(r => r.RoadData != null && r.RoadData.roadName == d.roadName))
                .ToList();
            AddRoads(roadToAdd);
        }
        public void RefreshAllRoads()
        {
            if (!IsDataValid) return;

            for (int i = 0; i < roads.Count; i++)
            {
                roads[i].RefreshRoadBlocks();
            }
        }
        //地图操作
        /// <summary>
        /// 从prefab或空物体创建Road。未预装Road脚本时自动添加ClassicRoad
        /// </summary>
        private IRoad CreateRoad(RoadData roadData)
        {
            //数据校验：空校验与重复校验
            if (!IsDataValid) return null;
            if (roads.Exists(r => r.RoadData != null && r.RoadData.roadName == roadData.roadName))
            {
                Debug.LogError($"Road with name {roadData.roadName} already exists in the map.");
                return null;
            }

            var parent = Transform;

            GameObject roadObj = roadPrefab != null
                ? Instantiate(roadPrefab, parent, false)
                : new GameObject();
            roadObj.name = string.IsNullOrEmpty(roadData.roadName) ? "Road" : $"Road_{roadData.roadName}";
            if (roadPrefab == null && parent != null)
            {
                roadObj.transform.SetParent(parent);
            }

            if (!roadObj.TryGetComponent<IRoad>(out var road))
            {
                road = roadObj.AddComponent<ClassicRoad>();
            }

            road.Init(this, roadData);
            roads.Add(road);
            return road;
        }
        public List<IRoad> CreateRoads(List<RoadData> roadDataList)
        {
            var list = new List<IRoad>();
            if (roadDataList == null) return list;
            foreach (var roadData in roadDataList)
            {
                var road = CreateRoad(roadData);
                if (road != null) list.Add(road);
            }
            return list;
        }
        public void AddRoads(List<RoadData> roadDataToAdd)
        {
            foreach (var roadData in roadDataToAdd)
            {
                var newRoad = CreateRoad(roadData);
                if (newRoad != null && !Roads.Contains(newRoad)) Roads.Add(newRoad);
            }
        }
        public void RemoveRoads(List<IRoad> roadsToRemove)
        {
            foreach (var road in roadsToRemove)
            {
                Roads.Remove(road);
                if (road is MonoBehaviour roadBehaviour)
                {
                    DestroyImmediate(roadBehaviour.gameObject);
                }
            }
        }
    }
}

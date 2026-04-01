using System.Collections.Generic;
using MusicTogether.Archived_DancingBall.DancingBall;
using MusicTogether.Archived_DancingBall.EditorTool;
using Sirenix.OdinInspector;
using UnityEngine;
using SceneDataAsset = MusicTogether.Archived_DancingBall.DancingBall.SceneData;

namespace MusicTogether.Archived_DancingBall.Scene
{
    public class Map : MonoBehaviour
    {
        public float ballRadius = 1f;
        public Factory Factory;
        public EditorTool.EditorTool EditorTool;
        public EditorActionDispatcher Dispatcher;
        public DisplacementApplier DisplacementApplier;
        public SceneDataAsset SceneData;
        public List<Road> Roads = new List<Road>();

        [Button]
        public void AddRoad(int targetSegmentIndex, int noteBeginIndex, int noteEndIndex)
        {
            int roadIndex = Roads.Count;
            // Calculate spawn position and rotation from the last block of the previous road
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            if (roadIndex > 0)
            {
                var prevRoad = Roads.Find(r => r.RoadIndex == roadIndex - 1);
                if (prevRoad != null && prevRoad.blocks.Count > 0)
                {
                    // Find the block with the largest index
                    Block lastBlock = null;
                    int maxIndex = -1;
                    foreach (var b in prevRoad.blocks)
                    {
                        if (b.blockLocalIndex > maxIndex)
                        {
                            maxIndex = b.blockLocalIndex;
                            lastBlock = b;
                        }
                    }

                    if (lastBlock != null)
                    {
                        position = lastBlock.transform.position;
                        rotation = lastBlock.transform.rotation;
                    }
                }
            }

            // Create the road
            var road = Factory?.CreateRoad(this, roadIndex);
            if (road == null) return;

            // Apply transform settings
            road.transform.position = position;
            road.transform.rotation = rotation;
            road.transform.localScale = Vector3.one;

            // Set SceneData
            if (SceneData != null)
            {
                var roadData = new RoadData(roadIndex, targetSegmentIndex, noteBeginIndex, noteEndIndex);
                SceneData.SetRoadData(roadData);
            }
            road.RefreshBlocks();
        }
        
        public void RefreshRoads()
        {
            foreach (var road in Roads)
            {
            }
        }
        public bool TryGetRoad(int roadIndex, out Road road)
        {
            road = Roads.Find(r => r.RoadIndex == roadIndex);
            return road != null;
        }

        public bool GetBlock(int roadIndex, int blockLocalIndex, out Block block)
        {
            block = null;
            var road = Roads.Find(r => r.RoadIndex == roadIndex);
            if (road == null) return false;
            block = road.blocks.Find(b => b.blockLocalIndex == blockLocalIndex);
            return block != null;
        }

        /// <summary>
        /// 获取下一块：若到路尾则返回下一路第一块。
        /// </summary>
        public bool NextBlock(int roadIndex, int currentBlockLocalIndex, out Block nextBlock)
        {
            nextBlock = null;
            if (SceneData == null) return false;
            if (!SceneData.GetRoadData(roadIndex, out var roadData)) return false;
            int blockCount = Mathf.Max(0, roadData.BlockCount);

            if (currentBlockLocalIndex + 1 < blockCount)
            {
                return GetBlock(roadIndex, currentBlockLocalIndex + 1, out nextBlock);
            }

            if (SceneData.GetRoadData(roadIndex + 1, out _))
            {
                return GetBlock(roadIndex + 1, 0, out nextBlock);
            }

            return false;
        }
    }
}

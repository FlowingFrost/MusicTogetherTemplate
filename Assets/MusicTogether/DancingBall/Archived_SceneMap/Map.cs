using System.Collections.Generic;
using MusicTogether.DancingBall.Archived_EditorTool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Archived_SceneMap
{
    public class Map : MonoBehaviour
    {
        public Factory factory;
        public EditorTool editorTool;
        public EditorActionDispatcher dispatcher;
        public DisplacementApplier displacementApplier;
        public MapData mapData;
        public List<Road> roads = new List<Road>();

        [Button]
        public void AddRoad()
        {
            factory.CreateRoad(this, roads.Count);
        }
        
        //Road数据库方法
        private int GetRoadListIndex_ByRoadGlobalIndex(int roadGlobalIndex) => roads.FindIndex(r => r.roadGlobalIndex == roadGlobalIndex);
        public bool TryGetRoad_ByRoadGlobalIndex(int roadGlobalIndex, out Road road)
        {
            road = roads.Find(r => r.roadGlobalIndex == roadGlobalIndex);
            return road != null;
        }
        
        /// <summary>
        /// 根据全局Block索引获取所在Road。注意：查找的是已经实例化的Block。查找数据请访问MapData。
        /// </summary>
        public bool TryGetRoad_ByBlockGlobalIndex(int targetBlockIndex, out Road road)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i].blocks.Exists(b => b.globalBlockIndex == targetBlockIndex))
                {
                    road = roads[i];
                    return true;
                }
            }

            road = null;
            return false; // Not found
        }
        
        
        
        public bool GetBlock(int roadIndex, int blockIndex, out Block block)
        {
            block = roads.Find(r => r.roadGlobalIndex == roadIndex).blocks.Find(b => b.globalBlockIndex == blockIndex);
            return block != null;
        }
        public bool NextBlock(int roadIndex, int currentBlockIndex, out Block nextBlock)
        {
            nextBlock = null;
            if (currentBlockIndex >= mapData.GetRoadEndBlockIndex(roadIndex))
            {
                var nextRoad = roads.Find(r => r.roadGlobalIndex == roadIndex + 1);
                if (nextRoad == null) return false;
                nextBlock = nextRoad.blocks[0];
                return true;
            }

            var road = roads.Find(r => r.roadGlobalIndex == roadIndex);
            if (road != null) return true;
            return false;
        }
    }
}
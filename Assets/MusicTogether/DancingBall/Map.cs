using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Map : MonoBehaviour
    {
        public Factory factory;
        public EditorTool editorTool;
        public DisplacementApplier displacementApplier;
        public MapData mapData;
        public List<Road> roads = new List<Road>();

        [Button]
        public void AddRoad()
        {
            factory.CreateRoad(this, roads.Count);
        }
        
        public int FindRoadIndexByBlockIndex(int targetBlockIndex)
        {
            for (int i = 0; i < roads.Count; i++)
            {
                if (roads[i].blocks.Exists(b => b.globalBlockIndex == targetBlockIndex))
                {
                    return i;
                }
            }
            return -1; // Not found
        }
    }
}
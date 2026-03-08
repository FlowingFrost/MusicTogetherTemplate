using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class Map : MonoBehaviour
    {
        public Factory factory;
        public EditorTool editorTool;
        public MapData mapData;
        public List<Road> roads = new List<Road>();
    }
}
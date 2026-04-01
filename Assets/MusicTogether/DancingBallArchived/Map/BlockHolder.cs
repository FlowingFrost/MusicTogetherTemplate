using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Map
{
    public class BlockHolder : MonoBehaviour
    {
        [Title("Resources")]
        public RoadHolder roadHolder;
        public int blockIndex;
        public int globalIndex;
        public float noteIndex;
        public Block block;

        [Title("Data")] 
        [ReadOnly]public bool isClickPoint;
        public bool isTurnPoint;
        public bool jump;

        [Title("Pre-Processed Data")] 
        public float animTime,tapTime;
        
    }
}
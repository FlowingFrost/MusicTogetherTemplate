using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Map
{
    public class RoadHolder : MonoBehaviour
    {
        [Title("Resources")]
        public MapHolder mapHolder;
        public int roadIndex;
        public List<BlockHolder> blockHolders;
        
        [Title("Data")] 
        public int tapPrefabIndex;

        //[Title("Information")]
        //[Title("Pre-Processed Data")] 
        
    }
}
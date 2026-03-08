using System.Collections.Generic;
using MusicTogether.DancingBallOld.Interfaces;
using Sirenix.OdinInspector;

namespace MusicTogether.DancingBallOld
{
    public class MapData : SerializedScriptableObject
    {
        public List<IBlockData> BlockDataList;
    }
}
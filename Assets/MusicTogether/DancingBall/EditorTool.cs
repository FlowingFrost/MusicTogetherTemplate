using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class EditorTool : MonoBehaviour
    {
        public void RefreshRoadBlocks(Road targetRoad)
        {
            if (targetRoad == null) return;
            if (targetRoad.map == null) return;
            if (targetRoad.map.factory == null) return;
            if (targetRoad.map.mapData == null) return;

            var mapData = targetRoad.map.mapData;
            mapData.GetRoadData(targetRoad.roadIndex, out var roadData);
            int blockCount = roadData.beginBlockIndex + mapData.GetRoadEndBlockIndex(targetRoad.roadIndex);
            OnRoadBlockCountChanged(targetRoad, blockCount);
        }
        
        public void OnRoadBlockCountChanged(Road targetRoad, int newCount)
        {
            if (targetRoad == null) return;
            if (targetRoad.map == null) return;
            if (targetRoad.map.factory == null) return;

            int formerCount = targetRoad.blocks.Count;
            if (formerCount < newCount)
            {
                targetRoad.map.factory.CreateBlocks(targetRoad, formerCount, newCount - formerCount);
            }
            else
            {
                var blocksToRemove = targetRoad.blocks.FindAll(b => b.blockIndex >= newCount);
                foreach (var block in blocksToRemove)
                {
                    targetRoad.blocks.Remove(block);
                    DestroyImmediate(block.gameObject);
                }
            }
            
            for (int i = 0; i < targetRoad.blocks.Count; i++)
            {
                targetRoad.blocks[i].blockIndex = i;
            }
        }
    }
}
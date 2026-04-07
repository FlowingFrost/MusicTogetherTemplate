using System;
using System.Collections.Generic;
using MusicTogether.DancingBall.Player;
using UnityEngine;

namespace MusicTogether.DancingBall.Scene
{
    public class ClassicTileHolder : MonoBehaviour, ITileHolder
    {
        public float tileThickness = 0.2f;
        public Transform tileParent;
        public Transform bottomTile, forwardTile, backwardTile;
        public void SetTileActive(bool forward, bool backward, bool bottom = true)
        {
            if (tileParent == null) return;
            if (forwardTile != null) forwardTile.gameObject.SetActive(forward);
            if (backwardTile != null) backwardTile.gameObject.SetActive(backward);
            if (bottomTile != null) bottomTile.gameObject.SetActive(bottom);
        }

        /// <summary>
        /// 返回所有已启用的地板Transform和他们的厚度。
        /// </summary>
        public List<MovementData> GetTileMovementDatum(double currentBlockTime, double singleBlockDuration, bool blockNeedTap)
        {
            int activeTileCount = 0;
            if (forwardTile != null && forwardTile.gameObject.activeSelf) activeTileCount++;
            if (bottomTile != null && bottomTile.gameObject.activeSelf) activeTileCount++;
            if (backwardTile != null && backwardTile.gameObject.activeSelf) activeTileCount++;
            
            List<MovementData> datum = new List<MovementData>();
            if (backwardTile != null && backwardTile.gameObject.activeSelf)
            {
                datum.Add(GenerateMovementData(backwardTile, datum.Count));
            }
            if (bottomTile != null && bottomTile.gameObject.activeSelf)
            {
                datum.Add(GenerateMovementData(bottomTile, datum.Count));
            }
            if (forwardTile != null && forwardTile.gameObject.activeSelf)
            {
                datum.Add(GenerateMovementData(forwardTile, datum.Count));
            }

            MovementData GenerateMovementData(Transform tileTransform, int currentListCount)
            {
                return new MovementData(
                    currentListCount == 0 ? blockNeedTap : false, 
                    activeTileCount switch
                    {
                        0 => currentBlockTime,
                        1 => currentBlockTime,
                        2 => currentBlockTime - singleBlockDuration*0.1 + currentListCount*singleBlockDuration*0.2,
                        3 => currentBlockTime - singleBlockDuration*0.1 + currentListCount*singleBlockDuration*0.1,
                        _ => currentBlockTime,
                    }, 
                    tileTransform, 
                    tileThickness);
            }
            return datum;
        }
    }
}
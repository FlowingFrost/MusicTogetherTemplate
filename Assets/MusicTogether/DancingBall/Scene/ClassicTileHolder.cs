using System;
using System.Collections.Generic;
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
        public List<(Transform, float)> GetTilePoses()
        {
            List<(Transform, float)> tileTransforms = new List<(Transform, float)>();
            if (forwardTile != null && forwardTile.gameObject.activeSelf) tileTransforms.Add((forwardTile, tileThickness));
            if (backwardTile != null && backwardTile.gameObject.activeSelf) tileTransforms.Add((backwardTile, tileThickness));
            if (bottomTile != null && bottomTile.gameObject.activeSelf) tileTransforms.Add((bottomTile, tileThickness));
            return tileTransforms;
        }
    }
}
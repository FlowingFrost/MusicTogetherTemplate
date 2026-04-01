using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Scene
{
    public class TileHolder : MonoBehaviour
    {
        public float TileThickness = 0.2f;
        public Transform tileParent;
        public Transform bottomTile, forwardTile, backwardTile;

        public void SetTileActive(bool forward, bool backward, bool bottom = true)
        {
            if (tileParent == null) return;
            if (forwardTile != null) forwardTile.gameObject.SetActive(forward);
            if (backwardTile != null) backwardTile.gameObject.SetActive(backward);
            if (bottomTile != null) bottomTile.gameObject.SetActive(bottom);
        }

        public List<Transform> GetTileTransforms()
        {
            List<Transform> tileTransforms = new List<Transform>();
            if (forwardTile != null && forwardTile.gameObject.activeSelf) tileTransforms.Add(forwardTile);
            if (backwardTile != null && backwardTile.gameObject.activeSelf) tileTransforms.Add(backwardTile);
            if (bottomTile != null && bottomTile.gameObject.activeSelf) tileTransforms.Add(bottomTile);
            return tileTransforms;
        }
    }
}

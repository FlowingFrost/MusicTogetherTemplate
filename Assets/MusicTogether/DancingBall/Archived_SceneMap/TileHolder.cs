using UnityEngine;

namespace MusicTogether.DancingBall.Archived_SceneMap
{
    public class TileHolder : MonoBehaviour
    {
        public Transform tileParent;
        public Transform bottomTile, forwardTile, backwardTile;
        
        public void SetTileActive(bool forward, bool backward, bool bottom = true)
        {
            if (tileParent == null) return;
            if (forwardTile != null) forwardTile.gameObject.SetActive(forward);
            if (backwardTile != null) backwardTile.gameObject.SetActive(backward);
            if (bottomTile != null) bottomTile.gameObject.SetActive(bottom);
        }
    }
}
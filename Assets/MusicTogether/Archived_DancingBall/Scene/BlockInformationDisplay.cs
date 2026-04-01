using UnityEngine;

namespace MusicTogether.Archived_DancingBall.Scene
{
    public class BlockInformationDisplay : MonoBehaviour
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        [SerializeField] private Renderer blockMask;
        

        public void RefreshBlockDisplay(Color color)
        {
            if (blockMask == null) blockMask = GetComponentInChildren<MeshRenderer>();
            var mpb = new MaterialPropertyBlock();
            blockMask.GetPropertyBlock(mpb);
            mpb.SetColor(Color1, color);
            blockMask.SetPropertyBlock(mpb);
        }
    }
}
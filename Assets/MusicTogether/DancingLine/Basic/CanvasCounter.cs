using UnityEngine;
using UnityEngine.UI;

namespace MusicTogether.DancingLine.Basic
{
    public class CanvasCounter : MonoBehaviour
    {
        public Slider ProgressBar;
        public double beginTime;
        public float duration;
        
        public void UpdateProgress(double levelProgress)
        {
            float elapsed = (float)(levelProgress - beginTime)/duration;
            ProgressBar.value = elapsed;
        }
    }
}
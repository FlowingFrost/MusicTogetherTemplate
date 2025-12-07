using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicTogether.DancingLine.Basic
{
    public class CanvasCounter : MonoBehaviour
    {
        public Slider ProgressBar;
        public TextMeshProUGUI text;
        public double beginTime;
        public float duration;
        
        public void UpdateProgress(double levelProgress)
        {
            float elapsed = (float)(levelProgress - beginTime)/duration;
            ProgressBar.value = elapsed;
            if (elapsed > 0)
            {
                text.gameObject.SetActive(true);
                ProgressBar.gameObject.SetActive(true);
                text.text = $"暂停中 ({(float)(levelProgress - beginTime)}s)";
            }
            else
            {
                text.gameObject.SetActive(false);
                ProgressBar.gameObject.SetActive(false);
            }
        }
    }
}
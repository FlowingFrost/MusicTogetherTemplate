using UnityEngine;

namespace LightGameFrame.InputDetector
{
    public class InputDetectorWithTrigger : InputDetector
    {
        void OnTriggerEnter(Collider other)
        {
            /*if (other.GetComponent<MainLine>() == null || isDetecting)
        {
            return;
        }*/
            BeginDetection();
        }
    }
}
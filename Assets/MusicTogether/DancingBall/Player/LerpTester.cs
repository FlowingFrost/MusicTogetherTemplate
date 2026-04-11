using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.Player
{
    public class LerpTester : MonoBehaviour
    {
        [Range(0f, 1.2f)]
        public float deltaValue;
        public Transform previousTransform;
        public Transform currentTransform;

        public void Update()
        {
            Vector3 currentPosition = Vector3.LerpUnclamped(previousTransform.position, currentTransform.position, deltaValue);
            Quaternion currentRotation = Quaternion.Lerp(previousTransform.rotation, currentTransform.rotation, deltaValue);
            transform.position = currentPosition;
            transform.rotation = currentRotation;
        }
    }
}
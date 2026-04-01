using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Map
{
    public class Tap : MonoBehaviour
    {
        [Title("Resources")]
        public MeshRenderer[] tapCorners;
        public MeshRenderer tapEffect;

        [Title("Data")] 
        public TimeRange timeRange;
        public float meshAngle;
        public float beginRadius;
        public AnimationCurve radiusCurve;
        public Gradient colorGradient;
        public float fadeTime;
        public AnimationCurve circleFadeCurve,effectFadeCurve;
        [Title("Debug")] 
        [Range(0,2)][SerializeField] private float previewTime;
        private float lastPreviewTime;
        
        [Title("Pre-Processed Data")]
        private float tapTime;
        private float angle;
        private MaterialPropertyBlock propBlock;
        private int colorID;

        [Title("RunTimeData")] 
        private Color tapColor;
        private float radius;
        private bool tapped;
        private float actualTapTime;
        
        void Awake()
        {
            propBlock = new MaterialPropertyBlock();
            colorID = Shader.PropertyToID("_Color");
        }

        public void StartAnimation(float taptime)
        {
            tapTime = taptime;
            angle = 360/tapCorners.Length;
            propBlock = new MaterialPropertyBlock();
            tapEffect.GetPropertyBlock(propBlock);
            for (int i = 0; i < tapCorners.Length; i++)
            {
                tapCorners[i].transform.localEulerAngles = new Vector3(0, i*angle, 0);
                tapCorners[i].gameObject.SetActive(true);
            }
        }

        public void PlayAnimation(float globalTime)
        {
            if (!tapped)
            {
                float value = (float)timeRange.GetProgress(globalTime - tapTime);
                float absValue = Mathf.Abs(1-Mathf.Abs(1 - value));// x=0~1 y=0~1 x=1~2 y=1~0
                FirstAnimation(absValue);
            }
            else
            {
                float value = (float)((globalTime - actualTapTime) / fadeTime);
                SecondAnimation(value);
            }
        }

        private void FirstAnimation(float value)
        { 
            Debug.Log("FirstAnimation");
            radius = beginRadius * radiusCurve.Evaluate(value);
            tapColor = colorGradient.Evaluate(value);
            propBlock.SetColor(colorID, tapColor);
            for (int i = 0; i < tapCorners.Length; i++)
            {
                float selfAngle = meshAngle-(i * angle);
                float angleRad = selfAngle * Mathf.Deg2Rad;
                tapCorners[i].transform.localPosition = new Vector3(radius * Mathf.Cos(angleRad), 0f, radius * Mathf.Sin(angleRad));
                tapCorners[i].SetPropertyBlock(propBlock);
            }
            tapColor.a = 0;
            propBlock.SetColor(colorID, tapColor);
            tapEffect.SetPropertyBlock(propBlock);
        }

        private void SecondAnimation(float value)
        {
            tapColor.a = circleFadeCurve.Evaluate(value)/255;
            propBlock.SetColor(colorID, tapColor);
            for (int i = 0; i < tapCorners.Length; i++)
            {
                tapCorners[i].SetPropertyBlock(propBlock);
            }
            tapColor.a = effectFadeCurve.Evaluate(value)/255;
            propBlock.SetColor(colorID, tapColor);
            tapEffect.SetPropertyBlock(propBlock);
        }

        public void Preview(float value)
        {
            Debug.Log("Preview");
            if (value <= 1)
                FirstAnimation(value);
            else
            {
                FirstAnimation(1);
                SecondAnimation(value-1);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            Awake();
            if (previewTime != lastPreviewTime)
            {
                StartAnimation(0);
                Preview(previewTime);
            }
        }
#endif
    }
}

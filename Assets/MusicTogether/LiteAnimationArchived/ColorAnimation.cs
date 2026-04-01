using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.LiteAnimation
{
    [Serializable]
    public class ColorAnimation : BaseFixAnimation<MeshRenderer>
    {
        public Gradient colorGradient;
        
        private MaterialPropertyBlock propBlock;
        private int colorID;
        
        public override void Init()
        {
            base.Init();
            propBlock = new MaterialPropertyBlock();
            target.GetPropertyBlock(propBlock);
            colorID = Shader.PropertyToID("_Color");
        }
        
        public override void PlayAnimation(double time)
        {
            if (!enabled) return;
            float t = Evaluate(time);
            Color color = colorGradient.Evaluate(t);
            propBlock.SetColor(colorID, color);
        }
    }
}
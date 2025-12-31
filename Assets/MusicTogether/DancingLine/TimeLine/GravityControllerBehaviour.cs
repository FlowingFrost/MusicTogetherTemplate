using System.Collections;
using System.Collections.Generic;
using MusicTogether.DancingLine.Basic;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.TimeLine
{
    public class GravityControllerBehaviour : PlayableBehaviour
    {
        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        private bool applied = false;
        
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            applied = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            LineComponent line = playerData as LineComponent;
            if (line != null && !applied)
            {
                line.OnGravityChanged(gravity);
                applied = true;
            }
        }
    }
}
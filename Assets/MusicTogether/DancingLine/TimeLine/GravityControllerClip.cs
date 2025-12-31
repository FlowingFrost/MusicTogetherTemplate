using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.TimeLine
{
    public class GravityControllerClip : PlayableAsset
    {
        public Vector3 gravity = new Vector3(0, -9.81f, 0);
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<GravityControllerBehaviour>.Create(graph);
            GravityControllerBehaviour behaviour = playable.GetBehaviour();
            behaviour.gravity = gravity;
            return playable;
        }
    }
}
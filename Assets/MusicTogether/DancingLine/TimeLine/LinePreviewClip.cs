using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.TimeLine
{
    public class LinePreviewClip : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LinePreviewBehaviour>.Create(graph);
            return playable;
        }
    }
}
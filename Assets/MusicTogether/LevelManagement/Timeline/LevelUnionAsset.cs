using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MusicTogether.LevelManagement.Timeline
{
    [Serializable]
    public class LevelUnionAsset : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] public ExposedReference<MonoBehaviour> levelUnion;

        public ClipCaps clipCaps => ClipCaps.Blending;
        [HideInInspector]
        public double clipStart;
        [HideInInspector]
        public double clipEnd;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LevelUnionBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.levelUnion = levelUnion.Resolve(graph.GetResolver());
            Debug.Log($"setting clipStart: {clipStart}, clipEnd: {clipEnd}");
            behaviour.clipStart = clipStart;
            behaviour.clipEnd = clipEnd;
            return playable;
        }
    }
}

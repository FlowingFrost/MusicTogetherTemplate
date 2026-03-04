using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MusicTogether.LevelManagement.Timeline
{
    [TrackColor(0.2f, 0.6f, 0.9f)]
    [TrackClipType(typeof(LevelUnionAsset))]
    public class LevelUnionTrack : TrackAsset
    {
        public UnionUpdateMode updateMode;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // 遍历所有 Clip 并注入时间信息
            foreach (var clip in GetClips())
            {
                var unionAsset = clip.asset as LevelUnionAsset;
                if (unionAsset != null)
                {
                    unionAsset.clipStart = clip.start;
                    unionAsset.clipEnd = clip.end;
                }
            }
            
            var mixer = ScriptPlayable<LevelUnionMixerBehaviour>.Create(graph, inputCount);
            var behaviour = mixer.GetBehaviour();
            behaviour.Mode = updateMode;
            return mixer;
        }
    }

    public class LevelUnionMixerBehaviour : PlayableBehaviour
    {
        public UnionUpdateMode Mode { get; set; }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();

            for (int i = 0; i < inputCount; i++)
            {
                if (playable.GetInputWeight(i) > 0)
                {
                    var inputPlayable = (ScriptPlayable<LevelUnionBehaviour>)playable.GetInput(i);
                    var inputBehaviour = inputPlayable.GetBehaviour();
                    inputBehaviour.Mode = Mode;
                }
            }
        }
    }
}

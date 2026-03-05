using UnityEngine.Playables;

namespace MusicTogether.LevelManagement.Timeline
{
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
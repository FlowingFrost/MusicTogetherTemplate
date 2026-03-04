using MusicTogether.DancingLine.Interfaces;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.TimeLine
{
    public class LinePreviewBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            ILineComponent line = playerData as ILineComponent;
            if (line != null)
            {
                line.Move();
            }
        }
    }
}
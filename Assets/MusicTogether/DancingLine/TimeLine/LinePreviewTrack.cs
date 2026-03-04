using MusicTogether.DancingLine.Interfaces;
using UnityEngine.Timeline;

namespace MusicTogether.DancingLine.TimeLine
{
    [TrackColor(0.2f, 0.6f, 1f)]
    [TrackClipType(typeof(LinePreviewClip))]
    [TrackBindingType(typeof(ILineComponent))]
    public class LinePreviewTtack : TrackAsset
    {
    }
}
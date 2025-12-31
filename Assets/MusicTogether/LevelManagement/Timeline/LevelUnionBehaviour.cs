using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.LevelManagement.Timeline
{
    public class LevelUnionBehaviour : PlayableBehaviour
    {
        public MonoBehaviour levelUnion;
        public double clipStart;
        public double clipEnd;
        private ILevelUnion union;
        private bool isInitialized;

        public UnionUpdateMode Mode { get; set; }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (levelUnion == null) return;
            union = levelUnion as ILevelUnion;
            if (union == null) return;

            union.AwakeUnion();
            isInitialized = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            switch (Mode)
            {
                case UnionUpdateMode.PlayMode:
                    if (Application.isPlaying)
                        Update(info);
                    break;
                case UnionUpdateMode.EditorMode:
                    if (!Application.isPlaying)
                        Update(info);
                    break;
                case UnionUpdateMode.Both:
                    Update(info);
                    break;
            }
        }
        
        void Update(FrameData info)
        {
            //Debug.Log($"LevelUnionBehaviour ProcessFrame - Mode: {Mode}, IsPlaying: {Application.isPlaying}");
            if (union == null) return;
            if (!isInitialized)
            {
                union.StartUnion(clipStart);
                isInitialized = true;
                return;
            }
            //Debug.Log("LevelUnionBehaviour UpdateUnion called");
            union.UpdateUnion();
        }
    }
}

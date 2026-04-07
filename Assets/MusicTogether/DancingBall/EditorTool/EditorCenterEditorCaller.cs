using MusicTogether.DancingBall.Player;
using MusicTogether.DancingBall.Scene;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    [ExecuteAlways]
    public class EditorCenterEditorCaller : SerializedMonoBehaviour
    {
        [SerializeField]private IMap map;
        [SerializeField]private BallPlayer player;
        private EditorCenter editorCenter;
        [Button("Setup EditorCenter")]
        void OnEnable()
        {
            editorCenter ??= new EditorCenter();
            editorCenter.Setup(map, player,0, 0);
        }

        void OnDisable()
        {
            editorCenter.Cleanup();
        }
    }
}
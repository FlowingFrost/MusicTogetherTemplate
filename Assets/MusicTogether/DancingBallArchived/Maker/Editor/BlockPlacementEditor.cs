using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Maker.Editor
{
    public static class BlockPlacementEditor
    {
        private const string EditKey = "MT.DB.BlockPlacementEditEnabled";
        private const string JumpKey = "MT.DB.AutoCornerJumpEnabled";

        private static bool _detectionEnabled;
        private static bool _autoJumpEnabled;
        private static bool _isUserOperation;
        [InitializeOnLoadMethod]
        private static void Init()
        {
            _detectionEnabled = EditorPrefs.GetBool(EditKey, false);
            _autoJumpEnabled = EditorPrefs.GetBool(JumpKey, true);
            ToggleDetection(_detectionEnabled, _autoJumpEnabled);
        }
        public static void ToggleDetection(bool enableEdit,bool enableJump)
        {
            // 清除现有事件
            EditorApplication.update -= OnEditorUpdate;
            Undo.postprocessModifications -= OnPostprocessModifications;
        
            if(enableEdit)
            {
                // 注册事件
                EditorApplication.update += OnEditorUpdate;
                Undo.postprocessModifications += OnPostprocessModifications;
            }
        
            _detectionEnabled = enableEdit;
            _autoJumpEnabled = enableJump;
        }

        private static void OnEditorUpdate()
        {
            if(!_detectionEnabled) return;
            
            var transform = Selection.transforms[0];
            if (transform.hasChanged)
            {
                if (_isUserOperation)
                {
                    BlockMaker script = transform.GetComponent<BlockMaker>();
                    if (script != null)
                    {
                        script.CheckModifications();
                    }
                }
                transform.hasChanged = false;
                _isUserOperation = false;
                return;
            }
            
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown)
            {
                ClassicPlacementType type = MakerWindow.GetClassicPlacementType(currentEvent.keyCode);
                BlockMaker script = Selection.transforms[0].GetComponent<BlockMaker>();
                if (script != null)
                {
                    script.ClassicInput(type);
                    if (_autoJumpEnabled)
                    {
                        SceneView sceneView = SceneView.lastActiveSceneView;
                        if (sceneView != null)
                        {
                            Vector3 newPosition = script.NextCorner(out var result).transform.position;
                            if (result)
                            {
                                sceneView.pivot = newPosition;
                                sceneView.Repaint();
                            }
                        }
                    }
                }
                return;    
            }
            
            
        }

        private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            _isUserOperation = true;
            return modifications;
        }
    }
}

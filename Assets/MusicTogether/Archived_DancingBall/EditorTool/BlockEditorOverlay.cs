#if UNITY_EDITOR
using MusicTogether.Archived_DancingBall.DancingBall;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.Archived_DancingBall.EditorTool
{
    [Overlay(typeof(SceneView), "Block Editor", defaultDisplay: true)]
    public class BlockEditorOverlay : Overlay
    {
        private BlockEditor _editor;
        private int _controlId = -1;

        private BlockEditor FindEditor() =>
            Object.FindFirstObjectByType<BlockEditor>();

        public override void OnCreated()
        {
            base.OnCreated();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnWillBeDestroyed()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            base.OnWillBeDestroyed();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            _editor = FindEditor();
            if (_editor == null || !_editor.enableEditorTool) return;

            if (Event.current.type == EventType.Layout)
            {
                _controlId = GUIUtility.GetControlID(FocusType.Keyboard);
                HandleUtility.AddDefaultControl(_controlId);
            }

            Event e = Event.current;
            if (e == null) return;

            _editor.SprintHeld = ResolveSprintHeld(_editor.sprint, e);

            if (e.type != EventType.KeyDown) return;
            Debug.Log($"KeyDown: {e.keyCode}, sprint held: {_editor.SprintHeld}");
            if (e.keyCode == _editor.nextBlock)
            {
                _editor.NavigateBlock(forward: true);
                e.Use();
                sceneView.Repaint();
            }
            else if (e.keyCode == _editor.previousBlock)
            {
                _editor.NavigateBlock(forward: false);
                e.Use();
                sceneView.Repaint();
            }
            else if (e.keyCode == _editor.setTurnTypeForward)
            {
                _editor.ApplyTurnType(TurnType.Forward);
                e.Use();
            }
            else if (e.keyCode == _editor.setTurnTypeRight)
            {
                _editor.ApplyTurnType(TurnType.Right);
                e.Use();
            }
            else if (e.keyCode == _editor.setTurnTypeLeft)
            {
                _editor.ApplyTurnType(TurnType.Left);
                e.Use();
            }
            else if (e.keyCode == _editor.setTurnTypeJump)
            {
                _editor.ApplyTurnType(TurnType.Jump);
                e.Use();
            }
            else if (e.keyCode == _editor.setTurnTypeNone)
            {
                _editor.ApplyTurnType(TurnType.None);
                e.Use();
            }
            else if (e.keyCode == _editor.setDisplacementTypeNone)
            {
                _editor.ApplyDisplacementType(DisplacementType.None);
                e.Use();
            }
            else if (e.keyCode == _editor.setDisplacementTypeUp)
            {
                _editor.ApplyDisplacementType(DisplacementType.Up);
                e.Use();
            }
            else if (e.keyCode == _editor.setDisplacementTypeDown)
            {
                _editor.ApplyDisplacementType(DisplacementType.Down);
                e.Use();
            }
            else if (e.keyCode == _editor.setDisplacementTypeForwardUp)
            {
                _editor.ApplyDisplacementType(DisplacementType.ForwardUp);
                e.Use();
            }
            else if (e.keyCode == _editor.setDisplacementTypeForwardDown)
            {
                _editor.ApplyDisplacementType(DisplacementType.ForwardDown);
                e.Use();
            }
        }

        private bool ResolveSprintHeld(KeyCode sprintKey, Event e)
        {
            if (sprintKey == KeyCode.LeftControl || sprintKey == KeyCode.RightControl)
                return e.control;
            if (sprintKey == KeyCode.LeftShift || sprintKey == KeyCode.RightShift)
                return e.shift;
            if (sprintKey == KeyCode.LeftAlt || sprintKey == KeyCode.RightAlt)
                return e.alt;
            if (e.keyCode == sprintKey)
            {
                if (e.type == EventType.KeyDown) return true;
                if (e.type == EventType.KeyUp)   return false;
            }
            return _editor != null && _editor.SprintHeld;
        }

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.minWidth = 230;
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;

            var titleLabel = new Label("Block Editor: 未连接");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 4;
            root.Add(titleLabel);

            var enableToggle = new Toggle("启用");
            enableToggle.style.marginBottom = 4;
            enableToggle.RegisterValueChangedCallback(evt =>
            {
                _editor = FindEditor();
                if (_editor == null) return;
                _editor.enableEditorTool = evt.newValue;
                EditorUtility.SetDirty(_editor);
            });
            root.Add(enableToggle);

            var infoLabel = new Label("");
            infoLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(infoLabel);

            var hintLabel = new Label("");
            hintLabel.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            hintLabel.style.marginTop = 4;
            hintLabel.style.whiteSpace = WhiteSpace.Normal;
            root.Add(hintLabel);

            // 跳转输入框
            var jumpLabel = new Label("跳转到 (Road | Block)");
            jumpLabel.style.marginTop = 6;
            jumpLabel.style.marginBottom = 2;
            root.Add(jumpLabel);

            var jumpRow = new VisualElement();
            jumpRow.style.flexDirection = FlexDirection.Row;
            jumpRow.style.alignItems = Align.Center;
            root.Add(jumpRow);

            var jumpRoadField = new IntegerField();
            jumpRoadField.style.width = 50;
            jumpRoadField.style.marginRight = 2;
            jumpRow.Add(jumpRoadField);

            var jumpBlockField = new IntegerField();
            jumpBlockField.style.width = 50;
            jumpBlockField.style.marginRight = 4;
            jumpRow.Add(jumpBlockField);

            var jumpButton = new Button(() =>
            {
                _editor = FindEditor();
                if (_editor == null) return;
                _editor.JumpToBlock(jumpRoadField.value, jumpBlockField.value);
                SceneView.RepaintAll();
            }) { text = "Go" };
            jumpButton.style.width = 40;
            jumpRow.Add(jumpButton);

            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = () =>
            {
                if (root.panel == null)
                {
                    EditorApplication.update -= updateCallback;
                    return;
                }
                RefreshLabels(titleLabel, infoLabel, hintLabel, enableToggle, jumpRoadField, jumpBlockField);
            };
            EditorApplication.update += updateCallback;

            return root;
        }

        private void RefreshLabels(Label titleLabel, Label infoLabel, Label hintLabel, Toggle enableToggle, IntegerField rField, IntegerField bField)
        {
            _editor = FindEditor();

            if (_editor == null)
            {
                titleLabel.text = "Block Editor: 场景中未找到组件";
                infoLabel.text  = "";
                hintLabel.text  = "";
                enableToggle.SetValueWithoutNotify(false);
                enableToggle.SetEnabled(false);
                return;
            }

            enableToggle.SetEnabled(true);
            enableToggle.SetValueWithoutNotify(_editor.enableEditorTool);

            if (!_editor.enableEditorTool)
            {
                titleLabel.text = $"Block Editor: 已禁用";
                infoLabel.text  = "";
                hintLabel.text  = "";
                return;
            }

            if (_editor.targetMap == null || _editor.targetMap.SceneData == null)
            {
                titleLabel.text = "Block Editor: 未绑定 Map";
                infoLabel.text  = "请设置 Target Map";
                return;
            }

            var sceneData = _editor.targetMap.SceneData;
            int rIdx = _editor.CurrentRoadIndex;
            int bIdx = _editor.CurrentBlockLocalIndex;
            
            // Sync fields if needed? No, let user type unless they want to sync.
            // Actually, maybe sync placeholders or values if not focused?
            // Simple: just show current in label.

            string blockLine = $"Current: R {rIdx} : B {bIdx}";
            
            if (sceneData.IsValidRoadIndex(rIdx))
            {
                sceneData.GetBlockData(rIdx, bIdx, out var bd);
                string tags = "";
                if (sceneData.HasTap(rIdx, bIdx)) tags += " [TAP]"; // SceneData needs HasTap
                if (bd != null)
                {
                    if (bd.HasTurn)         tags += $" [{bd.turnType}]";
                    if (bd.HasDisplacement) tags += $" [{bd.displacementType}]";
                }
                if (tags == "") tags = " [Normal]";
                blockLine += tags;
            }

            titleLabel.text = $"Block Editor Active";
            infoLabel.text  = blockLine;

            string sprintStat = _editor.SprintHeld ? "ON" : "off";
            hintLabel.text =
                $"{ShortKeyName(_editor.previousBlock)} / {ShortKeyName(_editor.nextBlock)}  导航\n" +
                $"{ShortKeyName(_editor.sprint)}  快速跳转（{sprintStat}）";
        }

        private static string ShortKeyName(KeyCode kc) =>
            kc.ToString()
              .Replace("LeftArrow",  "←")
              .Replace("RightArrow", "→")
              .Replace("UpArrow",    "↑")
              .Replace("DownArrow",  "↓")
              .Replace("Left",       "L-")
              .Replace("Right",      "R-");
    }
}
#endif

#if UNITY_EDITOR
using MusicTogether.DancingBall.SceneMap;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool
{
    /// <summary>
    /// SceneView Overlay，负责：
    ///   1. 通过 duringSceneGui + HandleUtility.AddDefaultControl 抢占输入控制权，
    ///      在 SceneView 消费方向键之前捕获键盘事件
    ///   2. 将按键事件转发给 BlockEditor 的导航逻辑
    ///   3. 在 SceneView 中显示当前方块状态面板
    /// 使用：SceneView 右上角 Overlays 菜单 → 勾选 "Block Editor"。
    /// </summary>
    [Overlay(typeof(SceneView), "Block Editor", defaultDisplay: true)]
    public class BlockEditorOverlay : Overlay
    {
        private BlockEditor _editor;
        private int _controlId = -1;

        private BlockEditor FindEditor() =>
            Object.FindFirstObjectByType<BlockEditor>();

        // ── Overlay 生命周期 ─────────────────────────────────────────

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

        // ── SceneGUI：抢占控制权 + 捕获键盘 ──────────────────────────

        private void OnSceneGUI(SceneView sceneView)
        {
            _editor = FindEditor();
            if (_editor == null || !_editor.enableEditorTool) return;

            // Layout 阶段注册 controlId 并抢占默认控制权。
            // AddDefaultControl 让 SceneView 自身工具在本 controlId 持有控制权期间
            // 不消费键盘事件，从而方向键等可以到达本回调。
            if (Event.current.type == EventType.Layout)
            {
                _controlId = GUIUtility.GetControlID(FocusType.Keyboard);
                HandleUtility.AddDefaultControl(_controlId);
            }

            Event e = Event.current;
            if (e == null) return;

            // sprint 修饰键：修饰符属性在任意事件类型都可读，无需焦点
            _editor.SprintHeld = ResolveSprintHeld(_editor.sprint, e);

            if (e.type != EventType.KeyDown) return;

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
            // ── TurnType ─────────────────────────────────────────────
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
            // ── DisplacementType ─────────────────────────────────────
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

        // ── Overlay 面板构建 ─────────────────────────────────────────

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

            // ── 手动跳转输入框 ────────────────────────────────────────
            var jumpLabel = new Label("跳转到 Block");
            jumpLabel.style.marginTop = 6;
            jumpLabel.style.marginBottom = 2;
            root.Add(jumpLabel);

            var jumpRow = new VisualElement();
            jumpRow.style.flexDirection = FlexDirection.Row;
            jumpRow.style.alignItems = Align.Center;
            root.Add(jumpRow);

            // 不传 label 文字，避免 IntegerField 内置 label 撑宽
            var jumpField = new IntegerField();
            jumpField.style.flexGrow = 1;
            jumpField.style.flexShrink = 1;
            jumpField.style.minWidth = 0;
            jumpField.style.marginRight = 4;
            jumpRow.Add(jumpField);

            var jumpButton = new Button(() =>
            {
                _editor = FindEditor();
                if (_editor == null) return;
                if (_editor.targetMap == null || _editor.targetMap.mapData == null) return;
                int total = _editor.targetMap.mapData.totalBlockCount;
                int target = Mathf.Clamp(jumpField.value, 0, total - 1);
                jumpField.SetValueWithoutNotify(target);
                _editor.JumpToBlock(target);
                SceneView.RepaintAll();
            }) { text = "Go" };
            jumpButton.style.width = 40;
            jumpButton.style.flexShrink = 0;
            jumpRow.Add(jumpButton);

            // EditorApplication.update 定期刷新标签
            EditorApplication.CallbackFunction updateCallback = null;
            updateCallback = () =>
            {
                if (root.panel == null)
                {
                    EditorApplication.update -= updateCallback;
                    return;
                }
                RefreshLabels(titleLabel, infoLabel, hintLabel, enableToggle);
            };
            EditorApplication.update += updateCallback;

            return root;
        }

        // ── 标签刷新 ────────────────────────────────────────────────

        private void RefreshLabels(Label titleLabel, Label infoLabel, Label hintLabel, Toggle enableToggle)
        {
            _editor = FindEditor();

            if (_editor == null)
            {
                titleLabel.text = "Block Editor: 场景中未找到 BlockEditor 组件";
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
                titleLabel.text = $"Block Editor: 已禁用（{_editor.gameObject.name}）";
                infoLabel.text  = "";
                hintLabel.text  = "";
                return;
            }

            if (_editor.targetMap == null || _editor.targetMap.mapData == null)
            {
                titleLabel.text = "Block Editor: 未绑定 Map";
                infoLabel.text  = "请在 BlockEditor 组件上设置 Target Map";
                hintLabel.text  = "";
                return;
            }

            MapData mapData = _editor.targetMap.mapData;
            int idx   = _editor.TargetBlockIndexForDisplay;
            int roadIdx     = _editor.targetMap.TryGetRoad_ByBlockGlobalIndex(idx, out var road) ? road.roadGlobalIndex : -1;
            int total = mapData.totalBlockCount;

            string blockLine = $"Block  {idx} / {total - 1}";
            if (idx >= 0 && mapData.InRange_ByBlockGlobalIndex(idx))
            {
                mapData.GetBlockData_ByBlockGlobalIndex(idx, out var bd);
                string tags = "";
                if (mapData.HasTap_ByBlockGlobalIndex(roadIdx,idx)) tags += " [TAP]";
                if (bd.HasTurn)               tags += $" [{bd.turnType}]";
                if (bd.HasDisplacement)       tags += $" [{bd.displacementType}]";
                if (tags == "")               tags  = " [Normal]";
                blockLine += tags;
            }

            
            string roadLine = roadIdx >= 0 ? $"Road   {roadIdx}" : "Road   —";

            titleLabel.text = $"Block Editor：{_editor.gameObject.name}";
            infoLabel.text  = $"{blockLine}\n{roadLine}";

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

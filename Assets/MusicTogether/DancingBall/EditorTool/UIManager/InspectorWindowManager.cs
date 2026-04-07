using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.UIManager
{
    public class InspectorWindowManager : UIManagerBase
    {
        private VisualElement _mapCommonContainer;
        private VisualElement _mapDerivedContainer;
        private VisualElement _mapMissBindingContainer;

        private VisualElement _roadCommonContainer;
        private VisualElement _roadDerivedContainer;
        private VisualElement _roadMissBindingContainer;

        private VisualElement _blockCommonContainer;
        private VisualElement _blockDerivedContainer;
        private VisualElement _blockMissBindingContainer;

        private Button _mapRebuildRoadsButton;
        private Button _mapRefreshAllRoadsButton;

        private Button _roadRefreshRoadBlocksButton;
        private Button _roadUpdateBlockTransformButton;
        private Button _roadRefreshBlockDisplayButton;
        private IntegerField _roadNoteBeginField;
        private Button _roadModifyNoteBeginButton;
        private IntegerField _roadNoteEndField;
        private Button _roadModifyNoteEndButton;
        private TextField _roadTargetDataNameField;
        private Button _roadModifyTargetDataNameButton;

        private EnumField _blockDisplacementDataTypeField;
        private EnumField _classicBlockTurnTypeField;
        private Button _classicBlockApplyTurnTypeButton;
        private EnumField _classicBlockDisplacementTypeField;
        private Button _classicBlockApplyDisplacementTypeButton;

        private EnumField _settingsPrevBlockField;
        private EnumField _settingsNextBlockField;
        private EnumField _settingsSprintField;
        private EnumField _settingsTurnNoneField;
        private EnumField _settingsTurnForwardField;
        private EnumField _settingsTurnRightField;
        private EnumField _settingsTurnLeftField;
        private EnumField _settingsTurnJumpField;
        private EnumField _settingsDisplacementNoneField;
        private EnumField _settingsDisplacementUpField;
        private EnumField _settingsDisplacementDownField;
        private EnumField _settingsDisplacementForwardUpField;
        private EnumField _settingsDisplacementForwardDownField;

        private VisualElement _bindedView;
        private VisualElement _unbindedView;
        private Button _retryButton;

        public Action MapRebuildRoadsRequested { get; set; }
        public Action MapRefreshAllRoadsRequested { get; set; }

        public Action RoadRefreshBlocksRequested { get; set; }
        public Action RoadUpdateBlockTransformRequested { get; set; }
        public Action RoadRefreshBlockDisplayRequested { get; set; }
        public Action<int> RoadModifyNoteBeginRequested { get; set; }
        public Action<int> RoadModifyNoteEndRequested { get; set; }
        public Action<string> RoadModifyTargetDataNameRequested { get; set; }

        public Action<Enum> BlockDisplacementDataTypeChanged { get; set; }
        public Action<Enum> ClassicBlockApplyTurnTypeRequested { get; set; }
        public Action<Enum> ClassicBlockApplyDisplacementTypeRequested { get; set; }

        public Action RetryBind { get; set; }

        public Action ShortcutSettingsSaved { get; set; }

        public InspectorWindowManager(VisualElement root) : base(root)
        {
            BindElements();
        }

        public void SetMapContainersVisibility(bool commonVisible, bool derivedVisible, bool missBindingVisible)
        {
            if (_mapCommonContainer != null) _mapCommonContainer.style.display = commonVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_mapDerivedContainer != null) _mapDerivedContainer.style.display = derivedVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_mapMissBindingContainer != null) _mapMissBindingContainer.style.display = missBindingVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetRoadContainersVisibility(bool commonVisible, bool derivedVisible, bool missBindingVisible)
        {
            if (_roadCommonContainer != null) _roadCommonContainer.style.display = commonVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_roadDerivedContainer != null) _roadDerivedContainer.style.display = derivedVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_roadMissBindingContainer != null) _roadMissBindingContainer.style.display = missBindingVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetBlockContainersVisibility(bool commonVisible, bool derivedVisible, bool missBindingVisible)
        {
            if (_blockCommonContainer != null) _blockCommonContainer.style.display = commonVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_blockDerivedContainer != null) _blockDerivedContainer.style.display = derivedVisible ? DisplayStyle.Flex : DisplayStyle.None;
            if (_blockMissBindingContainer != null) _blockMissBindingContainer.style.display = missBindingVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetBindedViewVisible(bool isBinded)
        {
            if (_bindedView != null)
            {
                _bindedView.style.display = isBinded ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_unbindedView != null)
            {
                _unbindedView.style.display = isBinded ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        public void LoadShortcutSettings(EditorShortcutConfig config = null)
        {
            if (config == null)
            {
                return;
            }

            _settingsPrevBlockField?.SetValueWithoutNotify(config.previousBlock);
            _settingsNextBlockField?.SetValueWithoutNotify(config.nextBlock);
            _settingsSprintField?.SetValueWithoutNotify(config.sprint);

            _settingsTurnNoneField?.SetValueWithoutNotify(config.setTurnTypeNone);
            _settingsTurnForwardField?.SetValueWithoutNotify(config.setTurnTypeForward);
            _settingsTurnRightField?.SetValueWithoutNotify(config.setTurnTypeRight);
            _settingsTurnLeftField?.SetValueWithoutNotify(config.setTurnTypeLeft);
            _settingsTurnJumpField?.SetValueWithoutNotify(config.setTurnTypeJump);

            _settingsDisplacementNoneField?.SetValueWithoutNotify(config.setDisplacementTypeNone);
            _settingsDisplacementUpField?.SetValueWithoutNotify(config.setDisplacementTypeUp);
            _settingsDisplacementDownField?.SetValueWithoutNotify(config.setDisplacementTypeDown);
            _settingsDisplacementForwardUpField?.SetValueWithoutNotify(config.setDisplacementTypeForwardUp);
            _settingsDisplacementForwardDownField?.SetValueWithoutNotify(config.setDisplacementTypeForwardDown);
        }

        public void SaveShortcutSettings(EditorShortcutConfig config = null)
        {
            config ??= EditorShortcutConfig.Config;
            if (config == null)
            {
                return;
            }

            config.previousBlock = ResolveKeyCode(_settingsPrevBlockField?.value);
            config.nextBlock = ResolveKeyCode(_settingsNextBlockField?.value);
            config.sprint = ResolveKeyCode(_settingsSprintField?.value);

            config.setTurnTypeNone = ResolveKeyCode(_settingsTurnNoneField?.value);
            config.setTurnTypeForward = ResolveKeyCode(_settingsTurnForwardField?.value);
            config.setTurnTypeRight = ResolveKeyCode(_settingsTurnRightField?.value);
            config.setTurnTypeLeft = ResolveKeyCode(_settingsTurnLeftField?.value);
            config.setTurnTypeJump = ResolveKeyCode(_settingsTurnJumpField?.value);

            config.setDisplacementTypeNone = ResolveKeyCode(_settingsDisplacementNoneField?.value);
            config.setDisplacementTypeUp = ResolveKeyCode(_settingsDisplacementUpField?.value);
            config.setDisplacementTypeDown = ResolveKeyCode(_settingsDisplacementDownField?.value);
            config.setDisplacementTypeForwardUp = ResolveKeyCode(_settingsDisplacementForwardUpField?.value);
            config.setDisplacementTypeForwardDown = ResolveKeyCode(_settingsDisplacementForwardDownField?.value);

            ShortcutSettingsSaved?.Invoke();
        }

        public void SetRoadNoteRange(int begin, int end)
        {
            _roadNoteBeginField?.SetValueWithoutNotify(begin);
            _roadNoteEndField?.SetValueWithoutNotify(end);
        }

        public void SetRoadTargetDataName(string dataName)
        {
            _roadTargetDataNameField?.SetValueWithoutNotify(dataName ?? string.Empty);
        }

        public void SetClassicBlockTurnType(Enum type)
        {
            if (type == null)
            {
                return;
            }

            _classicBlockTurnTypeField?.SetValueWithoutNotify(type);
        }

        public void SetClassicBlockDisplacementType(Enum type)
        {
            if (type == null)
            {
                return;
            }

            _classicBlockDisplacementTypeField?.SetValueWithoutNotify(type);
        }

        private void BindElements()
        {
            if (Root == null)
            {
                Debug.LogWarning("[EditorWindowManager] Root is null.");
                return;
            }

            _mapCommonContainer = Root.Q<VisualElement>("map-common-container");
            _mapDerivedContainer = Root.Q<VisualElement>("map-derived-container");
            _mapMissBindingContainer = Root.Q<VisualElement>("map-missbinding-container");

            _roadCommonContainer = Root.Q<VisualElement>("road-common-container");
            _roadDerivedContainer = Root.Q<VisualElement>("road-derived-container");
            _roadMissBindingContainer = Root.Q<VisualElement>("road-missbinding-container");

            _blockCommonContainer = Root.Q<VisualElement>("block-common-container");
            _blockDerivedContainer = Root.Q<VisualElement>("block-derived-container");
            _blockMissBindingContainer = Root.Q<VisualElement>("block-missbinding-container");

            _bindedView = Root.Q<VisualElement>("binded-view");
            _unbindedView = Root.Q<VisualElement>("unbinded-view");
            _retryButton = Root.Q<Button>("editor-retry");

            _mapRebuildRoadsButton = Root.Q<Button>("map-rebuild-roads");
            _mapRefreshAllRoadsButton = Root.Q<Button>("map-refresh-all-roads");

            _roadRefreshRoadBlocksButton = Root.Q<Button>("road-refresh-road-blocks");
            _roadUpdateBlockTransformButton = Root.Q<Button>("road-update-block-transform");
            _roadRefreshBlockDisplayButton = Root.Q<Button>("road-refresh-block-display");
            _roadNoteBeginField = Root.Q<IntegerField>("road-note-begin");
            _roadModifyNoteBeginButton = Root.Q<Button>("road-modify-note-begin");
            _roadNoteEndField = Root.Q<IntegerField>("road-note-end");
            _roadModifyNoteEndButton = Root.Q<Button>("road-modify-note-end");
            _roadTargetDataNameField = Root.Q<TextField>("road-target-data-name");
            _roadModifyTargetDataNameButton = Root.Q<Button>("road-modify-target-data-name");

            _blockDisplacementDataTypeField = Root.Q<EnumField>("block-displacement-data-type");
            _classicBlockTurnTypeField = Root.Q<EnumField>("classic-block-turn-type");
            _classicBlockApplyTurnTypeButton = Root.Q<Button>("classic-block-apply-turn-type");
            _classicBlockDisplacementTypeField = Root.Q<EnumField>("classic-block-displacement-type");
            _classicBlockApplyDisplacementTypeButton = Root.Q<Button>("classic-block-apply-displacement-type");

            _settingsPrevBlockField = Root.Q<EnumField>("settings-prev-block");
            _settingsNextBlockField = Root.Q<EnumField>("settings-next-block");
            _settingsSprintField = Root.Q<EnumField>("settings-sprint");
            _settingsTurnNoneField = Root.Q<EnumField>("settings-turn-none");
            _settingsTurnForwardField = Root.Q<EnumField>("settings-turn-forward");
            _settingsTurnRightField = Root.Q<EnumField>("settings-turn-right");
            _settingsTurnLeftField = Root.Q<EnumField>("settings-turn-left");
            _settingsTurnJumpField = Root.Q<EnumField>("settings-turn-jump");
            _settingsDisplacementNoneField = Root.Q<EnumField>("settings-displacement-none");
            _settingsDisplacementUpField = Root.Q<EnumField>("settings-displacement-up");
            _settingsDisplacementDownField = Root.Q<EnumField>("settings-displacement-down");
            _settingsDisplacementForwardUpField = Root.Q<EnumField>("settings-displacement-forward-up");
            _settingsDisplacementForwardDownField = Root.Q<EnumField>("settings-displacement-forward-down");

            ConfigureKeyCodeField(_settingsPrevBlockField);
            ConfigureKeyCodeField(_settingsNextBlockField);
            ConfigureKeyCodeField(_settingsSprintField);
            ConfigureKeyCodeField(_settingsTurnNoneField);
            ConfigureKeyCodeField(_settingsTurnForwardField);
            ConfigureKeyCodeField(_settingsTurnRightField);
            ConfigureKeyCodeField(_settingsTurnLeftField);
            ConfigureKeyCodeField(_settingsTurnJumpField);
            ConfigureKeyCodeField(_settingsDisplacementNoneField);
            ConfigureKeyCodeField(_settingsDisplacementUpField);
            ConfigureKeyCodeField(_settingsDisplacementDownField);
            ConfigureKeyCodeField(_settingsDisplacementForwardUpField);
            ConfigureKeyCodeField(_settingsDisplacementForwardDownField);

            if (_mapRebuildRoadsButton != null)
            {
                _mapRebuildRoadsButton.clicked += () => MapRebuildRoadsRequested?.Invoke();
            }

            if (_mapRefreshAllRoadsButton != null)
            {
                _mapRefreshAllRoadsButton.clicked += () => MapRefreshAllRoadsRequested?.Invoke();
            }

            if (_roadRefreshRoadBlocksButton != null)
            {
                _roadRefreshRoadBlocksButton.clicked += () => RoadRefreshBlocksRequested?.Invoke();
            }

            if (_roadUpdateBlockTransformButton != null)
            {
                _roadUpdateBlockTransformButton.clicked += () => RoadUpdateBlockTransformRequested?.Invoke();
            }

            if (_roadRefreshBlockDisplayButton != null)
            {
                _roadRefreshBlockDisplayButton.clicked += () => RoadRefreshBlockDisplayRequested?.Invoke();
            }

            if (_roadModifyNoteBeginButton != null)
            {
                _roadModifyNoteBeginButton.clicked += () => RoadModifyNoteBeginRequested?.Invoke(_roadNoteBeginField?.value ?? 0);
            }

            if (_roadModifyNoteEndButton != null)
            {
                _roadModifyNoteEndButton.clicked += () => RoadModifyNoteEndRequested?.Invoke(_roadNoteEndField?.value ?? 0);
            }

            if (_roadModifyTargetDataNameButton != null)
            {
                _roadModifyTargetDataNameButton.clicked += () => RoadModifyTargetDataNameRequested?.Invoke(_roadTargetDataNameField?.value ?? string.Empty);
            }

            if (_blockDisplacementDataTypeField != null)
            {
                _blockDisplacementDataTypeField.RegisterValueChangedCallback(evt => BlockDisplacementDataTypeChanged?.Invoke(evt.newValue));
            }

            if (_classicBlockApplyTurnTypeButton != null)
            {
                _classicBlockApplyTurnTypeButton.clicked += () => ClassicBlockApplyTurnTypeRequested?.Invoke(_classicBlockTurnTypeField?.value);
            }

            if (_classicBlockApplyDisplacementTypeButton != null)
            {
                _classicBlockApplyDisplacementTypeButton.clicked += () => ClassicBlockApplyDisplacementTypeRequested?.Invoke(_classicBlockDisplacementTypeField?.value);
            }

            if (_retryButton != null)
            {
                _retryButton.clicked += () => RetryBind?.Invoke();
            }

            RegisterShortcutSaveCallback(_settingsPrevBlockField);
            RegisterShortcutSaveCallback(_settingsNextBlockField);
            RegisterShortcutSaveCallback(_settingsSprintField);
            RegisterShortcutSaveCallback(_settingsTurnNoneField);
            RegisterShortcutSaveCallback(_settingsTurnForwardField);
            RegisterShortcutSaveCallback(_settingsTurnRightField);
            RegisterShortcutSaveCallback(_settingsTurnLeftField);
            RegisterShortcutSaveCallback(_settingsTurnJumpField);
            RegisterShortcutSaveCallback(_settingsDisplacementNoneField);
            RegisterShortcutSaveCallback(_settingsDisplacementUpField);
            RegisterShortcutSaveCallback(_settingsDisplacementDownField);
            RegisterShortcutSaveCallback(_settingsDisplacementForwardUpField);
            RegisterShortcutSaveCallback(_settingsDisplacementForwardDownField);
        }

        private void RegisterShortcutSaveCallback(EnumField field)
        {
            if (field == null)
            {
                return;
            }

            field.RegisterValueChangedCallback(_ => SaveShortcutSettings());
        }

        private void ConfigureKeyCodeField(EnumField field)
        {
            if (field == null)
            {
                return;
            }

            if (field.value == null || field.value.GetType() != typeof(KeyCode))
            {
                field.Init(KeyCode.None);
                field.SetValueWithoutNotify(KeyCode.None);
            }
        }

        private static KeyCode ResolveKeyCode(Enum value)
        {
            return value is KeyCode keyCode ? keyCode : KeyCode.None;
        }
    }
}

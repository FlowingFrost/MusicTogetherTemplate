using System;
using MusicTogether.DancingBall.EditorTool;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.UIManager
{
    public class SelectionWindowManager : UIManagerBase
    {
        
        private Label _titleLabel;
        private Toggle _enableToggle;
        private Label _infoLabel;
        private Label _hintLabel;
    private IntegerField _jumpRoadField;
        private IntegerField _jumpBlockField;
        private Button _jumpButton;
    private EnumField _defaultDisplacementTypeField;
        private VisualElement _bindedView;
        private VisualElement _unbindedView;
        private Button _retryButton;

        
        public Action<bool> EnableChanged { get; set; }
        public Action<int,int> JumpTo { get; set; }
    public Action RetryBind { get; set; }
    public Action<Enum> DefaultDisplacementTypeChanged { get; set; }
        
        public SelectionWindowManager(VisualElement root) : base(root)
        {
            BindElements();
        }

        public void UpdateSelectionInfo(int roadIndex, int blockIndex)
        {
            if (_infoLabel != null)
            {
                _infoLabel.text = $"Current: R {roadIndex} : B {blockIndex}";
            }

            if (_jumpRoadField != null)
            {
                _jumpRoadField.SetValueWithoutNotify(roadIndex);
            }

            if (_jumpBlockField != null)
            {
                _jumpBlockField.SetValueWithoutNotify(blockIndex);
            }
        }

        public void SetTitle(string title)
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = title;
            }
        }

        public void SetHint(string hint)
        {
            if (_hintLabel != null)
            {
                _hintLabel.text = hint;
            }
        }

        public void SetEnabledState(bool enabled)
        {
            if (_enableToggle != null)
            {
                _enableToggle.SetValueWithoutNotify(enabled);
                _enableToggle.MarkDirtyRepaint();
            }
        }

        public void SetToggleInteractable(bool enabled)
        {
            _enableToggle?.SetEnabled(enabled);
        }

        public void SetDefaultDisplacementType(BlockDisplacementDataType type)
        {
            if (_defaultDisplacementTypeField == null) return;
            _defaultDisplacementTypeField.SetValueWithoutNotify(type);
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

        private void BindElements()
        {
            if (Root == null)
            {
                Debug.LogWarning("[SelectionWindowView] Root is null.");
                return;
            }

            _titleLabel = Root.Q<Label>("selection-title");
            _enableToggle = Root.Q<Toggle>("selection-enable");
            _infoLabel = Root.Q<Label>("selection-info");
            _hintLabel = Root.Q<Label>("selection-hint");
            _jumpRoadField = Root.Q<IntegerField>("selection-jump-road");
            _jumpBlockField = Root.Q<IntegerField>("selection-jump-block");
            _jumpButton = Root.Q<Button>("selection-jump-go");
            _defaultDisplacementTypeField = Root.Q<EnumField>("selection-default-displacement-type");
            _bindedView = Root.Q<VisualElement>("binded-view");
            _unbindedView = Root.Q<VisualElement>("unbinded-view");
            _retryButton = Root.Q<Button>("selection-retry");

            if (_enableToggle != null)
            {
                _enableToggle.RegisterValueChangedCallback(evt => EnableChanged?.Invoke(evt.newValue));
            }

            if (_jumpButton != null)
            {
                _jumpButton.clicked += OnJumpClicked;
            }

            if (_defaultDisplacementTypeField != null)
            {
                _defaultDisplacementTypeField.Init(BlockDisplacementDataType.Classic);
                _defaultDisplacementTypeField.SetValueWithoutNotify(BlockDisplacementDataType.Classic);
                _defaultDisplacementTypeField.RegisterValueChangedCallback(evt => DefaultDisplacementTypeChanged?.Invoke(evt.newValue));
            }

            if (_retryButton != null)
            {
                _retryButton.clicked += OnRetryClicked;
            }
        }

        private void OnJumpClicked()
        {
            int roadIndex = _jumpRoadField?.value ?? 0;
            int blockIndex = _jumpBlockField?.value ?? 0;
            JumpTo?.Invoke(roadIndex, blockIndex);
        }

        private void OnRetryClicked()
        {
            RetryBind?.Invoke();
        }
    }
}

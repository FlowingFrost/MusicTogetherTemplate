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

        private Button _mapMissbindingButton;
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

        private VisualElement _bindedView;
        private VisualElement _unbindedView;
        private Button _retryButton;

        public Action MapMissBindingRetryRequested { get; set; }
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

            _mapMissbindingButton = Root.Q<Button>("map-missbinding-retry");
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

            if (_mapMissbindingButton != null)
            {
                _mapMissbindingButton.clicked += () => MapMissBindingRetryRequested?.Invoke();
            }
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

        }
    }
}

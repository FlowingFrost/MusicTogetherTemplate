using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MusicTogether.DancingBall.EditorTool.UIManager
{
    public class RoadCreateWindowManager : UIManagerBase
    {
        private TextField _roadNameField;
    private DropdownField _segmentField;
        private IntegerField _noteBeginField;
        private IntegerField _noteEndField;
        private Button _confirmButton;
        private Button _cancelButton;

    private readonly List<int> _segmentOptionIndices = new List<int>();

        public Action<string, int, int, int> CreateRequested { get; set; }
        public Action CancelRequested { get; set; }

        public RoadCreateWindowManager(VisualElement root) : base(root)
        {
            BindElements();
        }

        public void SetDefaults(string roadName, int segmentIndex, int noteBegin, int noteEnd)
        {
            _roadNameField?.SetValueWithoutNotify(roadName ?? string.Empty);
            SetSelectedSegment(segmentIndex);
            _noteBeginField?.SetValueWithoutNotify(noteBegin);
            _noteEndField?.SetValueWithoutNotify(noteEnd);
        }

        public void SetSegmentOptions(IReadOnlyList<string> displayNames, IReadOnlyList<int> segmentIndices)
        {
            if (_segmentField == null) return;
            _segmentOptionIndices.Clear();

            if (displayNames != null && segmentIndices != null)
            {
                int count = Mathf.Min(displayNames.Count, segmentIndices.Count);
                for (int i = 0; i < count; i++)
                {
                    _segmentOptionIndices.Add(segmentIndices[i]);
                }
                _segmentField.choices = new List<string>(displayNames);
            }
            else
            {
                _segmentField.choices = new List<string>();
            }

            if (_segmentField.index < 0 && _segmentField.choices.Count > 0)
            {
                _segmentField.index = 0;
                _segmentField.SetValueWithoutNotify(_segmentField.choices[0]);
            }
        }

        private int GetSelectedSegmentIndex()
        {
            int selectedIndex = _segmentField?.index ?? -1;
            if (selectedIndex >= 0 && selectedIndex < _segmentOptionIndices.Count)
            {
                return _segmentOptionIndices[selectedIndex];
            }
            return 0;
        }

        private void SetSelectedSegment(int segmentIndex)
        {
            if (_segmentField == null || _segmentField.choices == null) return;
            int optionIndex = _segmentOptionIndices.IndexOf(segmentIndex);
            if (optionIndex >= 0 && optionIndex < _segmentField.choices.Count)
            {
                _segmentField.index = optionIndex;
                _segmentField.SetValueWithoutNotify(_segmentField.choices[optionIndex]);
            }
            else if (_segmentField.choices.Count > 0)
            {
                _segmentField.index = 0;
                _segmentField.SetValueWithoutNotify(_segmentField.choices[0]);
            }
        }

        private void BindElements()
        {
            if (Root == null)
            {
                Debug.LogWarning("[RoadCreateWindowManager] Root is null.");
                return;
            }

            _roadNameField = Root.Q<TextField>("road-create-name");
            _segmentField = Root.Q<DropdownField>("road-create-segment");
            _noteBeginField = Root.Q<IntegerField>("road-create-note-begin");
            _noteEndField = Root.Q<IntegerField>("road-create-note-end");
            _confirmButton = Root.Q<Button>("road-create-confirm");
            _cancelButton = Root.Q<Button>("road-create-cancel");

            if (_confirmButton != null)
            {
                _confirmButton.clicked += () =>
                {
                    CreateRequested?.Invoke(
                        _roadNameField?.value ?? string.Empty,
                        GetSelectedSegmentIndex(),
                        _noteBeginField?.value ?? 0,
                        _noteEndField?.value ?? 0);
                };
            }

            if (_cancelButton != null)
            {
                _cancelButton.clicked += () => CancelRequested?.Invoke();
            }
        }
    }
}

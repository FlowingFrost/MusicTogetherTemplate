using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace LightGameFrame.InputDetector
{
    public class InputDetectorDebugUI : InputDetector
    {
        [Header("UI Toolkit")]
        public UIDocument uiDocument;
        private const string PanelTitle = "Input Detector Debug";
        private bool startOnEnable;

        private const string ContainerName = "input-detector-debug";
        private const string TitleLabelName = "input-detector-debug-title";
        private const string StatusLabelName = "input-detector-debug-status";
        private const string ConditionContainerName = "input-detector-debug-conditions";
        private const string StartButtonName = "input-detector-debug-start";
        private const string DetectedKeysContainerName = "DetectedKeys";

        private VisualElement _container;
        private Label _titleLabel;
        private Label _statusLabel;
        private VisualElement _conditionsRoot;
        private VisualElement _detectedKeysRoot;
        private Button _startButton;
        private bool _startButtonHooked;
        private readonly List<Label> _conditionLabels = new();
        private readonly Dictionary<KeyCode, Label> _detectedKeyLabels = new();
        private float? _startTime;

        private static readonly KeyCode[] PhysicalKeyboardKeys =
        {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M,
            KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.Tab, KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt,
            KeyCode.Backspace, KeyCode.Delete, KeyCode.Insert, KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown,
            KeyCode.Space, KeyCode.Return, KeyCode.Escape
        };

        void OnEnable()
        {
            EnsureUI();
            if (startOnEnable)
            {
                StartDebugDetection();
            }
        }

        void Update()
        {
            EnsureUI();
            RefreshUI();
        }

        public void StartDebugDetection()
        {
            _startTime = Time.time;
            BeginDetection();
        }

        private void EnsureUI()
        {
            if (uiDocument == null) return;
            var root = uiDocument.rootVisualElement;
            if (root == null) return;

            _container = root.Q<VisualElement>(ContainerName);
            if (_container == null) return;

            _titleLabel = _container.Q<Label>(TitleLabelName);
            _statusLabel = _container.Q<Label>(StatusLabelName);
            _conditionsRoot = _container.Q<VisualElement>(ConditionContainerName);
            _detectedKeysRoot = _container.Q<VisualElement>(DetectedKeysContainerName);
            _startButton = _container.Q<Button>(StartButtonName);

            if (_titleLabel != null)
            {
                _titleLabel.text = PanelTitle;
                _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            if (_statusLabel != null)
            {
                _statusLabel.text = "状态：未开始";
            }

            if (_startButton != null && !_startButtonHooked)
            {
                _startButton.clicked += StartDebugDetection;
                _startButtonHooked = true;
            }
        }

        private void RefreshUI()
        {
            if (_container == null || _statusLabel == null || _conditionsRoot == null) return;

            if (inputConditions == null || inputConditions.Count == 0)
            {
                _statusLabel.text = "状态：未配置条件";
                ClearConditionLabels();
                return;
            }

            if (!_startTime.HasValue && inputConditions.Exists(c => c.Meeted.HasValue))
            {
                _startTime = Time.time;
            }

            bool anyFailed = inputConditions.Exists(c => c.Meeted.HasValue && !c.Meeted.Value);
            bool allMet = inputConditions.All(c => c.Meeted.HasValue && c.Meeted.Value);
            bool anyUnset = inputConditions.Exists(c => !c.Meeted.HasValue);

            string status;
            if (anyFailed)
            {
                status = "失败";
            }
            else if (allMet)
            {
                status = "成功";
            }
            else if (anyUnset)
            {
                if (_startTime.HasValue && duration > 0f && Time.time - _startTime.Value > duration)
                {
                    status = "超时";
                }
                else if (_startTime.HasValue)
                {
                    status = "检测中";
                }
                else
                {
                    status = "未开始";
                }
            }
            else
            {
                status = "未开始";
            }

            if (_startTime.HasValue && duration > 0f && status == "检测中")
            {
                float remaining = Mathf.Max(0f, duration - (Time.time - _startTime.Value));
                _statusLabel.text = $"状态：{status}（剩余 {remaining:0.00}s）";
            }
            else
            {
                _statusLabel.text = $"状态：{status}";
            }

            EnsureConditionLabelCount(inputConditions.Count);
            for (int i = 0; i < inputConditions.Count; i++)
            {
                _conditionLabels[i].text = BuildConditionText(inputConditions[i]);
            }

            RefreshDetectedKeys();
        }

        private void RefreshDetectedKeys()
        {
            if (_detectedKeysRoot == null) return;

            var observedKeys = GetObservedKeys();
            var pressedKeys = new HashSet<KeyCode>();
            foreach (var key in observedKeys)
            {
                if (Input.GetKey(key))
                {
                    pressedKeys.Add(key);
                    if (!_detectedKeyLabels.TryGetValue(key, out var label))
                    {
                        label = new Label($"已按下：{key}")
                        {
                            name = $"detected-key-{key}"
                        };
                        _detectedKeysRoot.Add(label);
                        _detectedKeyLabels[key] = label;
                    }
                }
            }

            var keysToRemove = _detectedKeyLabels.Keys.Where(key => !pressedKeys.Contains(key)).ToList();
            foreach (var key in keysToRemove)
            {
                _detectedKeyLabels[key].RemoveFromHierarchy();
                _detectedKeyLabels.Remove(key);
            }
        }

        private HashSet<KeyCode> GetObservedKeys()
        {
            var keys = new HashSet<KeyCode>(PhysicalKeyboardKeys);
            if (inputConditions == null) return keys;

            foreach (var condition in inputConditions)
            {
                if (condition is SpecificKeyDetection specificKey)
                {
                    keys.Add(specificKey.targetKey);
                }
            }

            return keys;
        }

        private void EnsureConditionLabelCount(int count)
        {
            while (_conditionLabels.Count < count)
            {
                var label = new Label();
                _conditionsRoot.Add(label);
                _conditionLabels.Add(label);
            }

            while (_conditionLabels.Count > count)
            {
                var lastIndex = _conditionLabels.Count - 1;
                _conditionLabels[lastIndex].RemoveFromHierarchy();
                _conditionLabels.RemoveAt(lastIndex);
            }
        }

        private void ClearConditionLabels()
        {
            foreach (var label in _conditionLabels)
            {
                label.RemoveFromHierarchy();
            }
            _conditionLabels.Clear();
        }

        private string BuildConditionText(InputCondition condition)
        {
            string state = condition.Meeted switch
            {
                true => "满足",
                false => "失败",
                _ => "未检出"
            };

            if (condition is SpecificKeyDetection specificKey)
            {
                string required = specificKey.conditionType == SpecificKeyDetection.ConditionType.Required ? "必须有" : "不能有";
                return $"特定按键 {specificKey.targetKey} {required} ({specificKey.detectionType}) -> {state}";
            }

            if (condition is PhysicalKeyCountDetection keyCount)
            {
                return $"按键数量 ≥ {keyCount.neededCount} ({keyCount.detectionType}) -> {state}";
            }

            return $"未知条件 ({condition.GetType().Name}) -> {state}";
        }
    }
}

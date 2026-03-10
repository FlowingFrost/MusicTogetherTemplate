using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace MusicTogether.MusicSampling.Editor
{
    /// <summary>
    /// 音频采样 Editor 窗口
    /// 提供音频播放、波形可视化、音符标记等功能
    /// </summary>
    public class MusicSamplingWindow : EditorWindow
    {
        // 数据和播放器
        private AudioSamplingData _samplingData;
        private EditorAudioPlayer _audioPlayer;
        private float[] _audioSamples;

        // UI 元素
        private ObjectField _dataField;
        private Button _playButton;
        private Button _stopButton;
        private Button _markCurrentButton;
        private Slider _timelineSlider;
        private Label _timeLabel;
        private Label _bpmLabel;
        private Label _noteIndexLabel;
        private ScrollView _waveformContainer;
        private VisualElement _playhead;
        private WaveformVisualElement _waveformElement;

        // 状态
        private bool _isDraggingTimeline = false;
        private int _currentNoteIndex = -1;
        private int _highlightedNoteIndex = -1;

        // 平滑滚动状态
        private float _targetScrollX = 0f;       // 目标滚动位置（由音频时间驱动）
        private float _currentScrollX = 0f;      // 当前插值滚动位置
        private bool _scrollInitialized = false; // 是否已初始化滚动位置

        // 常量
        private const float PLAYHEAD_UPDATE_INTERVAL = 0.00f;
        private const float SCROLL_SMOOTH_SPEED = 8f; // 平滑追踪速度（越大越快跟上）
        private double _lastUpdateTime = 0;

        // 是否启用指数平滑滚动追踪（false = 直接跳到目标位置）
        private bool _enableSmoothScroll = true;

        [MenuItem("Window/MusicTogether/Music Sampling Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<MusicSamplingWindow>();
            window.titleContent = new GUIContent("Music Sampling");
            window.minSize = new Vector2(800, 600);
        }

        private void OnEnable()
        {
            // 初始化音频播放器
            _audioPlayer = new EditorAudioPlayer();
            _audioPlayer.Initialize();
            _audioPlayer.OnTimeChanged += OnAudioTimeChanged;
            _audioPlayer.OnStateChanged += OnAudioStateChanged;
            
            // 注册更新回调用于平滑滚动
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            // 清理资源
            EditorApplication.update -= OnEditorUpdate;
            
            if (_audioPlayer != null)
            {
                _audioPlayer.OnTimeChanged -= OnAudioTimeChanged;
                _audioPlayer.OnStateChanged -= OnAudioStateChanged;
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
        }

        private void CreateGUI()
        {
            // 加载 UXML（USS 已在 UXML 中绑定）
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/MusicTogether/MusicSampling/Editor/MusicSamplingWindow.uxml");
            
            visualTree.CloneTree(rootVisualElement);

            // 查询 UI 元素
            QueryUIElements();

            // 绑定事件
            BindEvents();
            
            // 全局鼠标抬起事件（安全措施）
            rootVisualElement.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (_isDraggingTimeline)
                {
                    _isDraggingTimeline = false;
                    if (_audioPlayer != null)
                    {
                        _audioPlayer.SetDragging(false);
                    }
                    Debug.Log($"[Timeline] 结束拖拽 (Global) - _isDraggingTimeline = false");
                }
            }, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// 查询 UI 元素
        /// </summary>
        private void QueryUIElements()
        {
            _dataField = rootVisualElement.Q<ObjectField>("data-field");
            _playButton = rootVisualElement.Q<Button>("play-button");
            _stopButton = rootVisualElement.Q<Button>("stop-button");
            _markCurrentButton = rootVisualElement.Q<Button>("mark-current-button");
            _timelineSlider = rootVisualElement.Q<Slider>("timeline-slider");
            _timeLabel = rootVisualElement.Q<Label>("time-label");
            _bpmLabel = rootVisualElement.Q<Label>("bpm-label");
            _noteIndexLabel = rootVisualElement.Q<Label>("note-index-label");
            _waveformContainer = rootVisualElement.Q<ScrollView>("waveform-container");

            // 设置 ObjectField 类型
            if (_dataField != null)
            {
                _dataField.objectType = typeof(AudioSamplingData);
            }
            
            // 初始状态下禁用标记按钮
            if (_markCurrentButton != null)
            {
                _markCurrentButton.SetEnabled(false);
            }
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvents()
        {
            if (_dataField != null)
                _dataField.RegisterValueChangedCallback(OnDataChanged);

            if (_playButton != null)
                _playButton.clicked += OnPlayButtonClicked;

            if (_stopButton != null)
                _stopButton.clicked += OnStopButtonClicked;
            
            if (_markCurrentButton != null)
                _markCurrentButton.clicked += OnMarkCurrentButtonClicked;

            if (_timelineSlider != null)
            {
                _timelineSlider.RegisterValueChangedCallback(OnTimelineValueChanged);
                
                // 鼠标按下时开始拖拽
                _timelineSlider.RegisterCallback<MouseDownEvent>(evt =>
                {
                    _isDraggingTimeline = true;
                    _audioPlayer.SetDragging(true);
                    Debug.Log($"[Timeline] 开始拖拽 - _isDraggingTimeline = true");
                });
                
                // 鼠标抬起时结束拖拽（捕获阶段，确保一定能收到）
                _timelineSlider.RegisterCallback<MouseUpEvent>(evt =>
                {
                    _isDraggingTimeline = false;
                    _audioPlayer.SetDragging(false);
                    Debug.Log($"[Timeline] 结束拖拽 (Slider) - _isDraggingTimeline = false");
                }, TrickleDown.TrickleDown);
                
                // 额外的安全措施：鼠标离开编辑器窗口时也结束拖拽
                _timelineSlider.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    if (_isDraggingTimeline)
                    {
                        _isDraggingTimeline = false;
                        _audioPlayer.SetDragging(false);
                        Debug.Log($"[Timeline] 结束拖拽 (MouseLeave) - _isDraggingTimeline = false");
                    }
                });
            }
        }

        /// <summary>
        /// 数据文件改变事件
        /// </summary>
        private void OnDataChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            _samplingData = evt.newValue as AudioSamplingData;
            
            if (_samplingData != null && _samplingData.audioClip != null)
            {
                LoadAudioData();
                UpdateWaveformDisplay();
                UpdateInfoLabels();
                
                // 启用标记按钮
                if (_markCurrentButton != null)
                {
                    _markCurrentButton.SetEnabled(true);
                }
            }
            else
            {
                ClearWaveformDisplay();
                
                // 禁用标记按钮
                if (_markCurrentButton != null)
                {
                    _markCurrentButton.SetEnabled(false);
                }
            }
        }

        /// <summary>
        /// 加载音频数据
        /// </summary>
        private void LoadAudioData()
        {
            if (_samplingData == null || _samplingData.audioClip == null)
                return;

            // 加载到播放器
            if (!_audioPlayer.LoadClip(_samplingData.audioClip))
                return;

            // 更新时间轴范围
            _timelineSlider.lowValue = 0;
            _timelineSlider.highValue = (float)_audioPlayer.Duration;
            _timelineSlider.value = 0;

            // 提取音频采样数据
            ExtractAudioSamples();
        }

        /// <summary>
        /// 提取音频采样数据
        /// </summary>
        private void ExtractAudioSamples()
        {
            var clip = _samplingData.audioClip;
            if (clip == null) return;

            try
            {
                int totalSamples = clip.samples * clip.channels;
                _audioSamples = new float[totalSamples];
                
                if (!clip.GetData(_audioSamples, 0))
                {
                    Debug.LogError("音频数据读取失败！");
                    _audioSamples = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"音频采样异常: {e.Message}");
                _audioSamples = null;
            }
        }

        /// <summary>
        /// 更新波形显示
        /// </summary>
        private void UpdateWaveformDisplay()
        {
            _waveformContainer.Clear();

            if (_samplingData == null || _audioSamples == null)
                return;

            // 创建波形可视化元素
            _waveformElement = new WaveformVisualElement(_samplingData, _audioSamples);
            _waveformElement.OnNoteClicked += OnNoteClicked;
            _waveformContainer.Add(_waveformElement);

            // 创建播放头
            _playhead = new VisualElement();
            _playhead.AddToClassList("playhead");
            _waveformContainer.Add(_playhead);

            // 添加点击留白区域选中当前音符的功能
            _waveformContainer.RegisterCallback<MouseDownEvent>(OnWaveformContainerClicked, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// 清除波形显示
        /// </summary>
        private void ClearWaveformDisplay()
        {
            _waveformContainer.Clear();
            var hintLabel = new Label("请选择 AudioSamplingData 资源文件");
            hintLabel.name = "hint-label";
            hintLabel.AddToClassList("hint-label");
            _waveformContainer.Add(hintLabel);
        }

        /// <summary>
        /// 播放按钮点击
        /// </summary>
        private void OnPlayButtonClicked()
        {
            if (_audioPlayer == null || _samplingData == null)
                return;

            _audioPlayer.TogglePlayPause();
        }

        /// <summary>
        /// 停止按钮点击
        /// </summary>
        private void OnStopButtonClicked()
        {
            if (_audioPlayer == null)
                return;

            _audioPlayer.Stop();
            // 重置平滑滚动状态，让下次播放从头开始
            _targetScrollX = 0f;
            _currentScrollX = 0f;
            _scrollInitialized = false;
        }

        /// <summary>
        /// 标记当前音符按钮点击
        /// </summary>
        private void OnMarkCurrentButtonClicked()
        {
            if (_samplingData == null || _audioPlayer == null)
                return;

            int currentNoteIndex = _samplingData.GetNoteIndexAtTime(_audioPlayer.CurrentTime);
            OnNoteClicked(currentNoteIndex);
        }

        /// <summary>
        /// 时间轴值改变
        /// </summary>
        private void OnTimelineValueChanged(ChangeEvent<float> evt)
        {
            if (_audioPlayer == null || _isDraggingTimeline == false)
                return;

            // 拖拽时实现音频擦洗
            _audioPlayer.Scrub(evt.newValue);
        }

        /// <summary>
        /// 音频时间改变回调
        /// </summary>
        private void OnAudioTimeChanged(double time)
        {
            // 节流更新，避免过于频繁
            if (EditorApplication.timeSinceStartup - _lastUpdateTime < PLAYHEAD_UPDATE_INTERVAL)
                return;

            _lastUpdateTime = EditorApplication.timeSinceStartup;

            // 更新时间轴（非拖拽状态下）
            if (!_isDraggingTimeline)
            {
                _timelineSlider.SetValueWithoutNotify((float)time);
            }

            // 更新播放头位置和滚动目标
            if (_playhead != null && _samplingData != null)
            {
                float noteWidth = _samplingData.noteWidth;
                // 用连续浮点时间换算像素位置，避免跳格
                float exactPixelX = (float)(time / _samplingData.SecondsPerNote) * noteWidth;
                _playhead.style.left = exactPixelX;

                // 高亮仍然用整数音符索引
                int noteIndex = _samplingData.GetNoteIndexAtTime(time);
                UpdateHighlightedNote(noteIndex);

                // 只写入目标值，实际滚动由 OnEditorUpdate 平滑完成
                if (_waveformContainer != null)
                {
                    float raw = exactPixelX - _waveformContainer.contentRect.width / 2;
                    _targetScrollX = Mathf.Max(0, raw);
                    if (!_scrollInitialized)
                    {
                        _currentScrollX = _targetScrollX;
                        _scrollInitialized = true;
                    }
                }
            }

            // 更新信息标签
            UpdateInfoLabels();

            // 检查当前音符
            CheckCurrentNote(time);
        }

        /// <summary>
        /// Editor 更新回调（用于平滑滚动）
        /// </summary>
        private void OnEditorUpdate()
        {
            if (_waveformContainer == null || _isDraggingTimeline)
                return;

            if (_enableSmoothScroll)
            {
                // 指数平滑追踪：1 - e^(-k * dt)
                float dt = (float)(EditorApplication.timeSinceStartup - _lastUpdateTime);
                dt = Mathf.Clamp(dt, 0.001f, 0.05f);
                _currentScrollX = Mathf.Lerp(_currentScrollX, _targetScrollX, 1f - Mathf.Exp(-SCROLL_SMOOTH_SPEED * dt));

                // 只有差异超过 0.5px 才写入，避免无意义的脏帧
                if (Mathf.Abs(_currentScrollX - _waveformContainer.scrollOffset.x) > 0.5f)
                {
                    _waveformContainer.scrollOffset = new Vector2(_currentScrollX, 0);
                }
            }
            else
            {
                // 直接跳到目标位置（无平滑）
                _currentScrollX = _targetScrollX;
                _waveformContainer.scrollOffset = new Vector2(_currentScrollX, 0);
            }
        }

        /// <summary>
        /// 更新高亮的音符
        /// </summary>
        private void UpdateHighlightedNote(int noteIndex)
        {
            if (_highlightedNoteIndex != noteIndex && _waveformElement != null)
            {
                // 移除旧的高亮
                if (_highlightedNoteIndex >= 0)
                {
                    _waveformElement.SetNoteHighlight(_highlightedNoteIndex, false);
                }

                // 添加新的高亮
                _highlightedNoteIndex = noteIndex;
                if (_highlightedNoteIndex >= 0)
                {
                    _waveformElement.SetNoteHighlight(_highlightedNoteIndex, true);
                }
            }
        }

        /// <summary>
        /// 音频状态改变回调
        /// </summary>
        private void OnAudioStateChanged(EditorAudioPlayer.PlayState state)
        {
            switch (state)
            {
                case EditorAudioPlayer.PlayState.Playing:
                    _playButton.text = "⏸ Pause";
                    break;
                case EditorAudioPlayer.PlayState.Paused:
                case EditorAudioPlayer.PlayState.Stopped:
                    _playButton.text = "▶ Play";
                    break;
            }
        }

        /// <summary>
        /// 更新信息标签
        /// </summary>
        private void UpdateInfoLabels()
        {
            if (_audioPlayer == null || _samplingData == null)
                return;

            double currentTime = _audioPlayer.CurrentTime;
            double duration = _audioPlayer.Duration;

            _timeLabel.text = $"{FormatTime(currentTime)} / {FormatTime(duration)}";
            
            int noteIndex = _samplingData.GetNoteIndexAtTime(currentTime);
            int barIndex = _samplingData.GetBarIndexAtNote(noteIndex);
            int noteInBar = noteIndex % _samplingData.NotesPerBar;
            
            _bpmLabel.text = $"BPM: {_samplingData.bpm} ({_samplingData.beatsPerBar}/4拍)";
            _noteIndexLabel.text = $"小节: {barIndex + 1} | 音符: {noteIndex} ({noteInBar + 1}/{_samplingData.NotesPerBar})";
        }

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        private string FormatTime(double time)
        {
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);
            int milliseconds = (int)((time % 1) * 1000);
            return $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }

        /// <summary>
        /// 检查当前音符并触发标记
        /// </summary>
        private void CheckCurrentNote(double time)
        {
            if (_samplingData == null)
                return;

            int noteIndex = _samplingData.GetNoteIndexAtTime(time);
            
            if (noteIndex != _currentNoteIndex)
            {
                _currentNoteIndex = noteIndex;
                
                // 这里可以添加音效提示
                // 如果当前音符被标记，播放提示音
            }
        }

        /// <summary>
        /// 音符点击事件
        /// </summary>
        private void OnNoteClicked(int noteIndex)
        {
            if (_samplingData == null)
                return;

            _samplingData.ToggleMarkedNote(noteIndex);
            EditorUtility.SetDirty(_samplingData);
            
            // 只更新对应的音符元素，而不是重建整个波形
            if (_waveformElement != null)
            {
                _waveformElement.RefreshNoteMarkedState(noteIndex);
            }
        }

        /// <summary>
        /// 波形容器点击事件（点击留白区域）
        /// </summary>
        private void OnWaveformContainerClicked(MouseDownEvent evt)
        {
            // 如果点击的是容器本身（留白区域），标记当前播放位置的音符
            if (evt.target == _waveformContainer)
            {
                if (_samplingData != null && _audioPlayer != null)
                {
                    int currentNoteIndex = _samplingData.GetNoteIndexAtTime(_audioPlayer.CurrentTime);
                    OnNoteClicked(currentNoteIndex);
                }
            }
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void Update()
        {
            if (_samplingData == null || _audioPlayer == null)
                return;

            // 空格键：标记当前音符
            if (Event.current != null && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Space)
                {
                    int noteIndex = _samplingData.GetNoteIndexAtTime(_audioPlayer.CurrentTime);
                    OnNoteClicked(noteIndex);
                    Event.current.Use();
                }
            }
        }
    }
}

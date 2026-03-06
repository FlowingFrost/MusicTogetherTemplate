using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace MusicTogether.MusicSampling.Editor
{
    /// <summary>
    /// 波形可视化 VisualElement
    /// 显示音频波形和音符标记
    /// </summary>
    public class WaveformVisualElement : VisualElement
    {
        private AudioSamplingData _data;
        private float[] _audioSamples;
        private List<NoteVisualElement> _noteElements = new List<NoteVisualElement>();
        private List<VisualElement> _barMarkers = new List<VisualElement>();

        // 事件
        public event Action<int> OnNoteClicked;

        public WaveformVisualElement(AudioSamplingData data, float[] audioSamples)
        {
            _data = data;
            _audioSamples = audioSamples;

            AddToClassList("waveform-element");
            style.flexDirection = FlexDirection.Row;
            style.height = 300;
            style.minHeight = 300;

            BuildWaveform();
        }

        /// <summary>
        /// 构建波形显示
        /// </summary>
        private void BuildWaveform()
        {
            if (_data == null || _audioSamples == null || _data.audioClip == null)
                return;

            // 计算总音符数
            int totalNotes = Mathf.CeilToInt((float)(_data.audioClip.length * _data.bpm * _data.beatDivision / 60.0));

            // 计算总小节数
            int notesPerBar = _data.NotesPerBar;
            int totalBars = Mathf.CeilToInt((float)totalNotes / notesPerBar);

            // 创建小节单元
            for (int barIndex = 0; barIndex < totalBars; barIndex++)
            {
                var barElement = CreateBarElement(barIndex, notesPerBar);
                Add(barElement);
            }
        }

        /// <summary>
        /// 创建小节单元
        /// </summary>
        private VisualElement CreateBarElement(int barIndex, int notesPerBar)
        {
            var barContainer = new VisualElement();
            barContainer.AddToClassList("bar-container");

            // 添加小节标记（左边框 + 小节号）
            var barMarker = new VisualElement();
            barMarker.AddToClassList("bar-marker");
            
            var barLabel = new Label($"{barIndex + 1}");
            barLabel.AddToClassList("bar-label");
            barMarker.Add(barLabel);
            
            barContainer.Add(barMarker);
            _barMarkers.Add(barMarker);

            // 创建拍容器
            var beatsContainer = new VisualElement();
            beatsContainer.AddToClassList("beats-container");

            int notesPerBeat = _data.beatDivision;
            int beatsPerBar = _data.beatsPerBar;

            // 创建该小节下的所有拍
            for (int beatIndex = 0; beatIndex < beatsPerBar; beatIndex++)
            {
                var beatElement = CreateBeatElement(barIndex, beatIndex, notesPerBeat);
                beatsContainer.Add(beatElement);
            }

            barContainer.Add(beatsContainer);

            return barContainer;
        }

        /// <summary>
        /// 创建节拍单元
        /// </summary>
        private VisualElement CreateBeatElement(int barIndex, int beatInBar, int notesPerBeat)
        {
            var beatContainer = new VisualElement();
            beatContainer.AddToClassList("beat-container");

            // 创建该节拍下的所有音符
            for (int i = 0; i < notesPerBeat; i++)
            {
                int noteIndex = barIndex * _data.NotesPerBar + beatInBar * notesPerBeat + i;
                var noteElement = CreateNoteElement(noteIndex);
                beatContainer.Add(noteElement);
                _noteElements.Add(noteElement);
            }

            return beatContainer;
        }

        /// <summary>
        /// 创建音符元素
        /// </summary>
        private NoteVisualElement CreateNoteElement(int noteIndex)
        {
            var noteElement = new NoteVisualElement(_data, _audioSamples, noteIndex);
            noteElement.OnClicked += () => OnNoteClicked?.Invoke(noteIndex);
            return noteElement;
        }

        /// <summary>
        /// 刷新指定音符的标记状态（性能优化：只更新单个音符）
        /// </summary>
        public void RefreshNoteMarkedState(int noteIndex)
        {
            if (noteIndex >= 0 && noteIndex < _noteElements.Count)
            {
                var noteElement = _noteElements[noteIndex];
                bool isMarked = _data.IsNoteMarked(noteIndex);
                noteElement.UpdateMarkedState(isMarked);
            }
        }

        /// <summary>
        /// 设置音符高亮状态（当前播放位置）
        /// </summary>
        public void SetNoteHighlight(int noteIndex, bool isHighlighted)
        {
            if (noteIndex >= 0 && noteIndex < _noteElements.Count)
            {
                var noteElement = _noteElements[noteIndex];
                noteElement.SetHighlight(isHighlighted);
            }
        }
    }

    /// <summary>
    /// 单个音符的可视化元素
    /// </summary>
    public class NoteVisualElement : VisualElement
    {
        private AudioSamplingData _data;
        private float[] _audioSamples;
        private int _noteIndex;
        private bool _isMarked;
        private bool _isHighlighted;

        // 颜色定义
        private static readonly Color NormalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color MarkedColor = new Color(0.2f, 0.6f, 1f, 1f);
        private static readonly Color WaveformColor = new Color(0.6f, 0.8f, 1f, 1f);

        public event Action OnClicked;

        public NoteVisualElement(AudioSamplingData data, float[] audioSamples, int noteIndex)
        {
            _data = data;
            _audioSamples = audioSamples;
            _noteIndex = noteIndex;
            _isMarked = data.IsNoteMarked(noteIndex);

            // 设置宽度（动态值，无法在 USS 中设置）
            style.width = data.noteWidth;
            
            // 使用样式类
            AddToClassList("note-element");
            AddToClassList(_isMarked ? "note-marked" : "note-normal");

            // 添加点击事件
            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0) // 左键点击
                {
                    OnClicked?.Invoke();
                    evt.StopPropagation();
                }
            });

            // 添加悬停效果
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                AddToClassList("note-hover");
            });

            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                RemoveFromClassList("note-hover");
            });

            // 生成波形图
            generateVisualContent += GenerateWaveform;
        }

        /// <summary>
        /// 绘制波形
        /// </summary>
        private void GenerateWaveform(MeshGenerationContext ctx)
        {
            if (_audioSamples == null || _data == null || _data.audioClip == null)
                return;

            // 计算该音符对应的采样范围
            int sampleRate = _data.audioClip.frequency;
            int channels = _data.audioClip.channels;
            
            double noteStartTime = _data.GetTimeAtNoteIndex(_noteIndex);
            double noteEndTime = _data.GetTimeAtNoteIndex(_noteIndex + 1);

            int sampleStart = Mathf.Clamp((int)(noteStartTime * sampleRate * channels), 0, _audioSamples.Length - 1);
            int sampleEnd = Mathf.Clamp((int)(noteEndTime * sampleRate * channels), 0, _audioSamples.Length - 1);

            if (sampleStart >= sampleEnd)
                return;

            // 分段采样
            int samplesPerSegment = _data.samplesPerNote;
            float segmentWidth = contentRect.width / samplesPerSegment;
            int samplesPerDivision = Mathf.Max(1, (sampleEnd - sampleStart) / samplesPerSegment);

            var painter = ctx.painter2D;
            painter.strokeColor = WaveformColor;
            painter.lineWidth = segmentWidth * 0.8f;
            painter.lineCap = LineCap.Round;

            // 绘制每个采样段
            for (int i = 0; i < samplesPerSegment; i++)
            {
                int segmentStart = sampleStart + i * samplesPerDivision;
                int segmentEnd = Mathf.Min(segmentStart + samplesPerDivision, sampleEnd);

                // 计算该段的平均振幅
                float sum = 0;
                for (int j = segmentStart; j < segmentEnd; j++)
                {
                    sum += Mathf.Abs(_audioSamples[j]);
                }
                float amplitude = (sum / (segmentEnd - segmentStart)) * _data.waveformZoom;
                amplitude = Mathf.Clamp01(amplitude);

                // 绘制垂直线条
                float x = i * segmentWidth + segmentWidth / 2;
                float centerY = contentRect.height / 2;
                float halfHeight = amplitude * contentRect.height / 2;

                painter.BeginPath();
                painter.MoveTo(new Vector2(x, centerY - halfHeight));
                painter.LineTo(new Vector2(x, centerY + halfHeight));
                painter.Stroke();
            }
        }

        /// <summary>
        /// 更新标记状态
        /// </summary>
        public void UpdateMarkedState(bool isMarked)
        {
            _isMarked = isMarked;
            UpdateVisualState();
        }

        /// <summary>
        /// 设置高亮状态
        /// </summary>
        public void SetHighlight(bool isHighlighted)
        {
            _isHighlighted = isHighlighted;
            UpdateVisualState();
        }

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState()
        {
            // 移除所有状态类
            RemoveFromClassList("note-normal");
            RemoveFromClassList("note-marked");
            RemoveFromClassList("note-current");

            // 高亮状态优先级最高
            if (_isHighlighted)
            {
                AddToClassList("note-current");
            }
            else if (_isMarked)
            {
                AddToClassList("note-marked");
            }
            else
            {
                AddToClassList("note-normal");
            }
        }
    }
}

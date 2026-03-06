using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusicTogether.MusicSampling
{
    /// <summary>
    /// 音频采样数据，存储音频配置和标记的音符信息
    /// </summary>
    [CreateAssetMenu(fileName = "AudioSamplingData", menuName = "MusicTogether/Audio Sampling Data")]
    public class AudioSamplingData : ScriptableObject
    {
        [Header("音频资源")]
        [Tooltip("要分析的音频文件")]
        public AudioClip audioClip;

        [Header("节拍配置")]
        [Tooltip("每分钟节拍数")]
        [Range(60, 300)]
        public int bpm = 120;

        [Tooltip("拍型：每小节的拍数")]
        [Range(2, 16)]
        public int beatsPerBar = 4;

        [Tooltip("节拍细分（每拍包含的音符数，如16分音符则为4）")]
        [Range(1, 16)]
        public int beatDivision = 4;

        [Header("已标记的音符")]
        [Tooltip("用户标记的音符索引列表")]
        public List<int> markedNoteIndices = new List<int>();

        [Header("可视化配置")]
        [Tooltip("音符宽度（像素）")]
        [Range(10, 100)]
        public float noteWidth = 40f;

        [Tooltip("波形缩放系数")]
        [Range(0.1f, 10f)]
        public float waveformZoom = 1.0f;

        [Tooltip("每个音符的波形采样条数")]
        [Range(1, 20)]
        public int samplesPerNote = 10;

        // 计算属性
        /// <summary>
        /// 每个音符的时长（秒）
        /// </summary>
        public double SecondsPerNote => 60.0 / (bpm * beatDivision);

        /// <summary>
        /// 每个节拍的时长（秒）
        /// </summary>
        public double SecondsPerBeat => 60.0 / bpm;

        /// <summary>
        /// 每小节的时长（秒）
        /// </summary>
        public double SecondsPerBar => (60.0 / bpm) * beatsPerBar;

        /// <summary>
        /// 每小节包含的音符数
        /// </summary>
        public int NotesPerBar => beatsPerBar * beatDivision;

        /// <summary>
        /// 根据时间获取音符索引
        /// </summary>
        public int GetNoteIndexAtTime(double time)
        {
            return Mathf.RoundToInt((float)(time * bpm * beatDivision / 60.0));
        }

        /// <summary>
        /// 根据音符索引获取时间
        /// </summary>
        public double GetTimeAtNoteIndex(int noteIndex)
        {
            return noteIndex * SecondsPerNote;
        }

        /// <summary>
        /// 根据音符索引获取所在小节索引
        /// </summary>
        public int GetBarIndexAtNote(int noteIndex)
        {
            return noteIndex / NotesPerBar;
        }

        /// <summary>
        /// 根据小节索引获取起始音符索引
        /// </summary>
        public int GetNoteIndexAtBar(int barIndex)
        {
            return barIndex * NotesPerBar;
        }

        /// <summary>
        /// 添加标记的音符
        /// </summary>
        public void AddMarkedNote(int noteIndex)
        {
            if (!markedNoteIndices.Contains(noteIndex))
            {
                markedNoteIndices.Add(noteIndex);
                markedNoteIndices.Sort();
            }
        }

        /// <summary>
        /// 移除标记的音符
        /// </summary>
        public void RemoveMarkedNote(int noteIndex)
        {
            markedNoteIndices.Remove(noteIndex);
        }

        /// <summary>
        /// 切换音符的标记状态
        /// </summary>
        public void ToggleMarkedNote(int noteIndex)
        {
            if (markedNoteIndices.Contains(noteIndex))
                RemoveMarkedNote(noteIndex);
            else
                AddMarkedNote(noteIndex);
        }

        /// <summary>
        /// 清除所有标记
        /// </summary>
        public void ClearAllMarkedNotes()
        {
            markedNoteIndices.Clear();
        }

        /// <summary>
        /// 检查音符是否被标记
        /// </summary>
        public bool IsNoteMarked(int noteIndex)
        {
            return markedNoteIndices.Contains(noteIndex);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MusicTogether.General
{
    /// <summary>
    /// 音符输入数据结构 - 存储特定BPM和节奏下的音符位置
    /// </summary>
    [Serializable]
    public struct InputNotes
    {
        [Tooltip("每分钟节拍数(Beats Per Minute)")]
        [Range(1, 999)]
        public int bpm;
        
        [Tooltip("音符类型(决定每个音符的时值)")]
        public NoteType noteType;
        
        [Tooltip("音符位置索引列表(将自动排序)")]
        public List<int> notes;
        
        private bool _isSorted;
        
        /// <summary>
        /// 确保音符列表按升序排列(惰性排序)
        /// </summary>
        private void EnsureSorted()
        {
            if (_isSorted) return;
            
            if (notes == null)
            {
                notes = new List<int>();
                _isSorted = true;
                return;
            }
            
            // 移除无效的负数音符索引
            notes.RemoveAll(n => n < 0);
            
            // 按升序排序
            notes.Sort();
            _isSorted = true;
        }
        
        /// <summary>
        /// 添加单个音符位置
        /// </summary>
        /// <param name="noteIndex">音符索引位置</param>
        public void AddNote(int noteIndex)
        {
            if (noteIndex < 0)
            {
                Debug.LogWarning($"[InputNotes] 尝试添加无效的音符索引: {noteIndex}");
                return;
            }
            
            if (notes == null) notes = new List<int>();
            
            notes.Add(noteIndex);
            _isSorted = false;
        }
        
        /// <summary>
        /// 批量添加音符位置
        /// </summary>
        public void AddNotes(IEnumerable<int> noteIndices)
        {
            if (notes == null) notes = new List<int>();
            
            foreach (var index in noteIndices)
            {
                if (index >= 0)
                    notes.Add(index);
            }
            _isSorted = false;
        }
        
        /// <summary>
        /// 获取所有音符的时间点列表(秒)
        /// </summary>
        /// <returns>已排序的时间点列表</returns>
        public List<double> GetNoteTimes()
        {
            EnsureSorted();
            
            if (notes == null || notes.Count == 0)
                return new List<double>();
            
            if (!NoteConverter.IsValidBPM(bpm))
            {
                Debug.LogError($"[InputNotes] 无效的BPM值: {bpm}");
                return new List<double>();
            }
            
            List<double> noteTimes = new List<double>(notes.Count);
            foreach (var noteIndex in notes)
            {
                double time = NoteConverter.GetNoteTime(bpm, noteType, noteIndex);
                noteTimes.Add(time);
            }
            
            return noteTimes;
        }
        
        /// <summary>
        /// 获取指定音符索引的时间点
        /// </summary>
        public double GetNoteTimeAt(int noteIndex)
        {
            return NoteConverter.GetNoteTime(bpm, noteType, noteIndex);
        }
        
        /// <summary>
        /// 检查指定位置是否有音符
        /// </summary>
        public bool HasNoteAt(int noteIndex)
        {
            EnsureSorted();
            return notes != null && notes.BinarySearch(noteIndex) >= 0;
        }
        
        /// <summary>
        /// 获取音符数量
        /// </summary>
        public int NoteCount => notes?.Count ?? 0;
        
        /// <summary>
        /// 获取只读的音符列表
        /// </summary>
        public IReadOnlyList<int> GetNotes()
        {
            EnsureSorted();
            return notes?.AsReadOnly();
        }
        
        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public bool IsValid()
        {
            return NoteConverter.IsValidBPM(bpm) && notes != null;
        }
    }
    
    /// <summary>
    /// 音符数据ScriptableObject - 存储多个音符段落的数据
    /// 用于在Unity编辑器中创建和管理音符配置
    /// </summary>
    [CreateAssetMenu(menuName = "MusicTogether/NoteData", fileName = "NewNoteData")]
    public class InputNoteData : ScriptableObject
    {
        [Header("采音数据转换")]
        [Tooltip("从AudioSamplingData转换音符数据")]
        public MusicTogether.MusicSampling.AudioSamplingData audioSamplingData;
        
        [Tooltip("音符类型(默认16分音符)")]
        public NoteType targetNoteType = NoteType.Semi;
        
        [Header("音符数据")]
        [Tooltip("多个音符列表,可以包含不同BPM的段落")]
        public List<InputNotes> noteLists = new List<InputNotes>();
        
        // 缓存机制
        private List<double> _cachedNoteTimes;
        private bool _isDirty = true;
        
        /// <summary>
        /// 获取所有音符段落的时间点合集(带缓存优化)
        /// </summary>
        /// <param name="forceRecalculate">是否强制重新计算</param>
        /// <returns>所有音符的时间点列表(已排序)</returns>
        public List<double> GetNoteTimes(bool forceRecalculate = false)
        {
            if (forceRecalculate || _isDirty)
            {
                _cachedNoteTimes = CalculateAllNoteTimes();
                _isDirty = false;
            }
            
            return new List<double>(_cachedNoteTimes); // 返回副本防止外部修改
        }
        
        /// <summary>
        /// 计算所有音符时间(内部方法)
        /// </summary>
        private List<double> CalculateAllNoteTimes()
        {
            if (noteLists == null || noteLists.Count == 0)
                return new List<double>();
            
            List<double> allNoteTimes = new List<double>();
            
            for (int i = 0; i < noteLists.Count; i++)
            {
                var noteList = noteLists[i];
                if (!noteList.IsValid())
                {
                    Debug.LogWarning($"[InputNoteData] 第 {i} 个音符列表无效,已跳过");
                    continue;
                }
                
                allNoteTimes.AddRange(noteList.GetNoteTimes());
            }
            
            // 最终排序
            allNoteTimes.Sort();
            return allNoteTimes;
        }
        
        /// <summary>
        /// 获取指定段落的音符时间
        /// </summary>
        public List<double> GetNoteTimesForSegment(int segmentIndex)
        {
            if (segmentIndex < 0 || segmentIndex >= noteLists.Count)
            {
                Debug.LogError($"[InputNoteData] 段落索引超出范围: {segmentIndex}");
                return new List<double>();
            }
            
            return noteLists[segmentIndex].GetNoteTimes();
        }
        
        /// <summary>
        /// 添加新的音符段落
        /// </summary>
        public void AddSegment(InputNotes notes)
        {
            if (noteLists == null)
                noteLists = new List<InputNotes>();
            
            noteLists.Add(notes);
            MarkDirty();
        }
        
        /// <summary>
        /// 标记数据已修改,需要重新计算
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// 获取总音符数量
        /// </summary>
        public int TotalNoteCount
        {
            get
            {
                int count = 0;
                if (noteLists != null)
                {
                    foreach (var noteList in noteLists)
                        count += noteList.NoteCount;
                }
                return count;
            }
        }
        
        /// <summary>
        /// 获取段落数量
        /// </summary>
        public int SegmentCount => noteLists?.Count ?? 0;
        
        /// <summary>
        /// 验证所有数据
        /// </summary>
        public bool ValidateData()
        {
            if (noteLists == null || noteLists.Count == 0)
            {
                Debug.LogWarning($"[InputNoteData] {name}: 没有音符数据");
                return false;
            }
            
            bool allValid = true;
            for (int i = 0; i < noteLists.Count; i++)
            {
                if (!noteLists[i].IsValid())
                {
                    Debug.LogError($"[InputNoteData] {name}: 第 {i} 个段落数据无效");
                    allValid = false;
                }
            }
            
            return allValid;
        }
        
        /// <summary>
        /// Unity编辑器数据变更回调
        /// </summary>
        private void OnValidate()
        {
            MarkDirty();
        }
        
        /// <summary>
        /// 从AudioSamplingData转换音符数据
        /// </summary>
        public void ConvertFromAudioSamplingData()
        {
            if (audioSamplingData == null)
            {
                Debug.LogError($"[InputNoteData] {name}: AudioSamplingData未设置!");
                return;
            }
            
            if (audioSamplingData.markedNoteIndices == null || audioSamplingData.markedNoteIndices.Count == 0)
            {
                Debug.LogWarning($"[InputNoteData] {name}: AudioSamplingData中没有标记的音符!");
                return;
            }
            
            // 清空现有数据
            noteLists.Clear();
            
            // 创建新的音符段落
            InputNotes newNotes = new InputNotes
            {
                bpm = audioSamplingData.bpm,
                noteType = targetNoteType,
                notes = new List<int>()
            };
            
            // AudioSamplingData使用的是beatDivision细分的音符索引
            // 需要根据beatDivision和目标NoteType进行转换
            
            // 计算转换比例
            // AudioSamplingData的beatDivision表示每拍有多少个音符
            // 例如：beatDivision=4表示16分音符，beatDivision=2表示8分音符
            
            // 目标NoteType对应的每拍音符数
            int targetNotesPerBeat = GetNotesPerBeatForNoteType(targetNoteType);
            
            // 转换比例 = 目标音符数 / 源音符数
            float conversionRatio = (float)targetNotesPerBeat / audioSamplingData.beatDivision;
            
            // 转换每个标记的音符索引
            foreach (int sourceIndex in audioSamplingData.markedNoteIndices)
            {
                int targetIndex = Mathf.RoundToInt(sourceIndex * conversionRatio);
                newNotes.AddNote(targetIndex);
            }
            
            // 添加到列表
            noteLists.Add(newNotes);
            MarkDirty();
            
            Debug.Log($"[InputNoteData] 成功从 {audioSamplingData.name} 转换音符数据!");
            Debug.Log($"BPM: {newNotes.bpm}, 音符类型: {newNotes.noteType}, 音符数量: {newNotes.NoteCount}");
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        /// <summary>
        /// 获取指定音符类型每拍的音符数
        /// </summary>
        private int GetNotesPerBeatForNoteType(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Quarter:
                    return 1;  // 四分音符：每拍1个
                case NoteType.Eighth:
                    return 2;  // 八分音符：每拍2个
                case NoteType.Semi:
                    return 4;  // 十六分音符：每拍4个
                case NoteType.ThirtySecond:
                    return 8;  // 三十二分音符：每拍8个
                default:
                    return 4;
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 从AudioSamplingData转换（右键菜单）
        /// </summary>
        [ContextMenu("Convert From Audio Sampling Data")]
        private void ConvertFromAudioSamplingDataMenu()
        {
            ConvertFromAudioSamplingData();
        }
        
        /// <summary>
        /// 调试信息
        /// </summary>
        [ContextMenu("Print Debug Info")]
        private void PrintDebugInfo()
        {
            Debug.Log($"=== {name} Debug Info ===");
            Debug.Log($"段落数量: {SegmentCount}");
            Debug.Log($"总音符数: {TotalNoteCount}");
            
            for (int i = 0; i < noteLists.Count; i++)
            {
                var segment = noteLists[i];
                Debug.Log($"段落 {i}: BPM={segment.bpm}, 类型={segment.noteType}, 音符数={segment.NoteCount}");
            }
            
            var times = GetNoteTimes(true);
            Debug.Log($"时间点数量: {times.Count}");
            if (times.Count > 0)
            {
                Debug.Log($"时间范围: {times[0]:F3}s - {times[times.Count - 1]:F3}s");
            }
        }
#endif
    }
}


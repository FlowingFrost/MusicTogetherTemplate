using System;
using UnityEngine;

namespace MusicTogether.General
{
    /// <summary>
    /// 音符类型枚举 - 表示音符的时值
    /// </summary>
    public enum NoteType 
    { 
        Quarter,        // 四分音符
        Eighth,         // 八分音符
        Semi,           // 十六分音符
        ThirtySecond    // 三十二分音符
    }
    
    /// <summary>
    /// 音符时间转换工具类
    /// 用于计算音符在时间轴上的位置
    /// </summary>
    public static class NoteConverter
    {
        /// <summary>
        /// 将音符索引从一种音符类型转换到另一种音符类型
        /// </summary>
        /// <param name="originalIndex">原始音符索引</param>
        /// <param name="fromType">源音符类型</param>
        /// <param name="toType">目标音符类型</param>
        /// <returns>转换后的音符索引</returns>
        public static int ConvertNoteIndex(int originalIndex, NoteType fromType, NoteType toType) 
        {
            if (originalIndex < 0)
            {
                Debug.LogWarning($"[NoteConverter] 无效的音符索引: {originalIndex}");
                return 0;
            }
            
            if (fromType == toType) return originalIndex;
            
            double ratio = GetRatio(fromType) / GetRatio(toType);
            double convertedIndex = originalIndex * ratio;
            return Mathf.FloorToInt((float)convertedIndex);
        }

        /// <summary>
        /// 计算指定音符在时间轴上的时间点(秒)
        /// </summary>
        /// <param name="bpm">每分钟节拍数</param>
        /// <param name="noteType">音符类型</param>
        /// <param name="noteIndex">音符索引位置</param>
        /// <returns>音符时间(秒)</returns>
        public static double GetNoteTime(int bpm, NoteType noteType, float noteIndex)
        {
            if (bpm <= 0)
            {
                Debug.LogError($"[NoteConverter] 无效的BPM值: {bpm}");
                return 0;
            }
            
            if (noteIndex < 0)
            {
                Debug.LogWarning($"[NoteConverter] 负数音符索引: {noteIndex}");
            }

            // 计算公式: 时间 = (音符索引 * 音符比例 * 60) / BPM
            double noteLengthInBeats = GetRatio(noteType);
            return (noteIndex * noteLengthInBeats * 60.0) / bpm;
        }

        /// <summary>
        /// 获取音符类型占一拍的比例
        /// </summary>
        /// <param name="type">音符类型</param>
        /// <returns>占一拍的比例(四分音符为1)</returns>
        private static double GetRatio(NoteType type) 
        {
            switch (type)
            {
                case NoteType.Quarter:      return 1.0;    // 四分音符 = 1拍
                case NoteType.Eighth:       return 0.5;    // 八分音符 = 1/2拍
                case NoteType.Semi:         return 0.25;   // 十六分音符 = 1/4拍
                case NoteType.ThirtySecond: return 0.125;  // 三十二分音符 = 1/8拍
                default: 
                    throw new ArgumentException($"未知的音符类型: {type}");
            }
        }
        
        /// <summary>
        /// 验证BPM值是否有效
        /// </summary>
        public static bool IsValidBPM(int bpm) => bpm > 0 && bpm <= 999;
        
        /// <summary>
        /// 验证音符索引是否有效
        /// </summary>
        public static bool IsValidNoteIndex(int noteIndex) => noteIndex >= 0;
    }
}
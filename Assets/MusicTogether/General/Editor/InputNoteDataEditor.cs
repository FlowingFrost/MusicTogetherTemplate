using UnityEditor;
using UnityEngine;

namespace MusicTogether.General.Editor
{
    /// <summary>
    /// InputNoteData的自定义编辑器
    /// 添加从AudioSamplingData转换的按钮
    /// </summary>
    [CustomEditor(typeof(InputNoteData))]
    public class InputNoteDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            InputNoteData noteData = (InputNoteData)target;
            
            // 绘制默认的Inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            // 添加转换按钮
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("采音数据转换工具", EditorStyles.boldLabel);
            
            if (noteData.audioSamplingData == null)
            {
                EditorGUILayout.HelpBox("请先设置 Audio Sampling Data 才能进行转换", MessageType.Warning);
            }
            else
            {
                var asd = noteData.audioSamplingData;
                int segmentCount = asd.segments?.Count ?? 0;
                int markedTotal = 0;
                if (asd.segments != null)
                {
                    foreach (var seg in asd.segments)
                        markedTotal += seg?.markedNoteIndices?.Count ?? 0;
                }
                else
                {
                    markedTotal = asd.markedNoteIndices?.Count ?? 0;
                }

                EditorGUILayout.HelpBox(
                    segmentCount > 0
                        ? $"将从 {asd.name} 转换音符数据 (多段)\n" +
                          $"段落数: {segmentCount}\n" +
                          $"总标记音符数: {markedTotal}\n" +
                          $"目标音符类型: {noteData.targetNoteType}"
                        : $"将从 {asd.name} 转换音符数据 (单段兼容)\n" +
                          $"源BPM: {asd.bpm}\n" +
                          $"源细分: 每拍{asd.beatDivision}个音符\n" +
                          $"标记音符数: {asd.markedNoteIndices?.Count ?? 0}\n" +
                          $"目标音符类型: {noteData.targetNoteType}",
                    MessageType.Info);
            }
            
            EditorGUI.BeginDisabledGroup(noteData.audioSamplingData == null);
            
            if (GUILayout.Button("从采音数据转换", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "确认转换",
                    "这将清空当前的所有音符数据并从采音数据重新生成。\n是否继续?",
                    "确定",
                    "取消"))
                {
                    Undo.RecordObject(noteData, "Convert From Audio Sampling Data");
                    noteData.ConvertFromAudioSamplingData();
                }
            }
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }
    }
}

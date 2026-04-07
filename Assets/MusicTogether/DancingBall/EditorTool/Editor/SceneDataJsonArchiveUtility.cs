using System;
using System.IO;
using MusicTogether.DancingBall.Data;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool.Editor
{
    public static class SceneDataJsonArchiveUtility
    {
        [MenuItem("MusicTogether/DancingBall/SceneData JSON Archive/Export Selected SceneData...")]
        public static void ExportSelectedSceneData()
        {
            var sceneData = Selection.activeObject as SceneData;
            if (sceneData == null)
            {
                EditorUtility.DisplayDialog("SceneData JSON Archive", "请选择一个 SceneData 资源。", "确定");
                return;
            }

            var defaultName = $"{sceneData.name}.json";
            var path = EditorUtility.SaveFilePanel("导出 SceneData JSON", Application.dataPath, defaultName, "json");
            if (string.IsNullOrWhiteSpace(path)) return;

            var json = SceneDataJsonArchive.ToJson(sceneData, true);
            File.WriteAllText(path, json);
            Debug.Log($"[SceneDataJsonArchive] 导出完成: {path}", sceneData);
        }

        [MenuItem("MusicTogether/DancingBall/SceneData JSON Archive/Import To Selected SceneData...")]
        public static void ImportToSelectedSceneData()
        {
            var sceneData = Selection.activeObject as SceneData;
            if (sceneData == null)
            {
                EditorUtility.DisplayDialog("SceneData JSON Archive", "请选择一个 SceneData 资源。", "确定");
                return;
            }

            var path = EditorUtility.OpenFilePanel("导入 SceneData JSON", Application.dataPath, "json");
            if (string.IsNullOrWhiteSpace(path)) return;

            var json = File.ReadAllText(path);
            var archive = SceneDataJsonArchive.FromJson(json);
            archive.ApplyToSceneData(sceneData, true);

            EditorUtility.SetDirty(sceneData);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneDataJsonArchive] 导入完成: {path}", sceneData);
        }

        [MenuItem("MusicTogether/DancingBall/SceneData JSON Archive/Validate Round Trip (Selected)")]
        public static void ValidateRoundTrip()
        {
            var sceneData = Selection.activeObject as SceneData;
            if (sceneData == null)
            {
                EditorUtility.DisplayDialog("SceneData JSON Archive", "请选择一个 SceneData 资源。", "确定");
                return;
            }

            try
            {
                var json = SceneDataJsonArchive.ToJson(sceneData, false);
                var archive = SceneDataJsonArchive.FromJson(json);
                var tempSceneData = ScriptableObject.CreateInstance<SceneData>();
                archive.ApplyToSceneData(tempSceneData, true);

                bool roadCountMatch = tempSceneData.roadDataList.Count == sceneData.roadDataList.Count;
                Debug.Log($"[SceneDataJsonArchive] RoundTrip 验证完成：RoadCountMatch={roadCountMatch}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneDataJsonArchive] RoundTrip 验证失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
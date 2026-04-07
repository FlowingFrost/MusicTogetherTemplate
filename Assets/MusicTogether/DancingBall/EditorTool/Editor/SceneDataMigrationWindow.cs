using MusicTogether.DancingBall.Data;
using UnityEditor;
using UnityEngine;
using OldSceneData = MusicTogether.Archived_DancingBall.DancingBall.SceneData;
using OldRoadData = MusicTogether.Archived_DancingBall.DancingBall.RoadData;
using OldBlockData = MusicTogether.Archived_DancingBall.DancingBall.BlockData;
using OldTurnType = MusicTogether.Archived_DancingBall.DancingBall.TurnType;
using OldDisplacementType = MusicTogether.Archived_DancingBall.DancingBall.DisplacementType;

namespace MusicTogether.DancingBall.EditorTool.Editor
{
    public class SceneDataMigrationWindow : UnityEditor.EditorWindow
    {
        private OldSceneData sourceSceneData;
        private SceneData targetSceneData;
        private bool copyInputNoteData = true;
        private bool overwriteTarget = true;

        [MenuItem("MusicTogether/DancingBall/SceneData Migration")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneDataMigrationWindow>("SceneData Migration");
            window.minSize = new Vector2(420, 260);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("旧版 SceneData → 新版 SceneData", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            sourceSceneData = (OldSceneData)EditorGUILayout.ObjectField("旧 SceneData", sourceSceneData, typeof(OldSceneData), false);
            targetSceneData = (SceneData)EditorGUILayout.ObjectField("新 SceneData", targetSceneData, typeof(SceneData), false);

            EditorGUILayout.Space(6);
            copyInputNoteData = EditorGUILayout.Toggle("复制 InputNoteData", copyInputNoteData);
            overwriteTarget = EditorGUILayout.Toggle("覆盖目标数据", overwriteTarget);

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(sourceSceneData == null || targetSceneData == null))
            {
                if (GUILayout.Button("开始迁移"))
                {
                    Migrate();
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("迁移会把旧 Road/Block 规则转为新 RoadData + ClassicBlockDisplacementData。\n" +
                                    "如目标数据不为空且未勾选覆盖，将追加到列表末尾。", MessageType.Info);
        }

        private void Migrate()
        {
            if (sourceSceneData == null || targetSceneData == null) return;

            if (overwriteTarget)
            {
                targetSceneData.roadDataList.Clear();
            }

            if (copyInputNoteData)
            {
                targetSceneData.inputNoteData = sourceSceneData.inputNoteData;
            }

            foreach (OldRoadData oldRoad in sourceSceneData.roadDataList)
            {
                var newRoad = new RoadData(oldRoad.RoadGlobalIndex, oldRoad.TargetSegmentIndex, oldRoad.NoteBeginIndex, oldRoad.NoteEndIndex)
                {
                    roadName = BuildRoadName(oldRoad)
                };

                foreach (OldBlockData oldBlock in oldRoad.blockDataList)
                {
                    var newBlock = new ClassicBlockDisplacementData(oldBlock.blockLocalIndex)
                    {
                        turnType = MapTurnType(oldBlock.turnType),
                        displacementType = MapDisplacementType(oldBlock.displacementType)
                    };
                    newRoad.blockDisplacementDataList.Add(newBlock);
                }

                targetSceneData.Set_RoadData(newRoad);
            }

            EditorUtility.SetDirty(targetSceneData);
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneDataMigration] 迁移完成。", targetSceneData);
        }

        private static string BuildRoadName(OldRoadData oldRoad)
        {
            return $"Road_{oldRoad.TargetSegmentIndex}_{oldRoad.NoteBeginIndex}_{oldRoad.RoadGlobalIndex}";
        }

        private static ClassicBlockDisplacementData.TurnType MapTurnType(OldTurnType oldType)
        {
            return oldType switch
            {
                OldTurnType.Left => ClassicBlockDisplacementData.TurnType.Left,
                OldTurnType.Right => ClassicBlockDisplacementData.TurnType.Right,
                OldTurnType.Jump => ClassicBlockDisplacementData.TurnType.Jump,
                OldTurnType.Forward => ClassicBlockDisplacementData.TurnType.Jump,
                _ => ClassicBlockDisplacementData.TurnType.None
            };
        }

        private static ClassicBlockDisplacementData.DisplacementType MapDisplacementType(OldDisplacementType oldType)
        {
            return oldType switch
            {
                OldDisplacementType.Up => ClassicBlockDisplacementData.DisplacementType.Up,
                OldDisplacementType.Down => ClassicBlockDisplacementData.DisplacementType.Down,
                OldDisplacementType.ForwardUp => ClassicBlockDisplacementData.DisplacementType.ForwardUp,
                OldDisplacementType.ForwardDown => ClassicBlockDisplacementData.DisplacementType.ForwardDown,
                _ => ClassicBlockDisplacementData.DisplacementType.None
            };
        }
    }
}

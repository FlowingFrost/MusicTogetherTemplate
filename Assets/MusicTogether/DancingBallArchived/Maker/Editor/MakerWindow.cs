using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Maker.Editor
{
    public class MakerWindow : OdinEditorWindow
    {
        [OnValueChanged(nameof(ToggleValueUpdate))]
        [SerializeField] [ToggleLeft]
        private bool enableBlockPlacementEdit,enableAutoJumpToNextCorner;
        [OnValueChanged(nameof(SaveKeys))]
        [SerializeField][ListDrawerSettings(ShowIndexLabels = false)] [LabelText("@Labels[$index]")]
        private KeyCode[] keys = new KeyCode[8];
        private static readonly string[] Labels = { "Forward","BackWard","Left","Right","Up45","Up90","Down45","Down90" };
        
        [MenuItem("Tools/DB Maker Settings")]
        public static void ShowWindow()
        {
            GetWindow<MakerWindow>("Dancing Ball Maker Settings").Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            enableBlockPlacementEdit = EditorPrefs.GetBool("MT.DB.BlockPlacementEditEnabled", false);
            enableAutoJumpToNextCorner = EditorPrefs.GetBool("MT.DB.AutoCornerJumpEnabled", true);
        }
        private void ToggleValueUpdate()
        {
            EditorPrefs.SetBool("MT.DB.BlockPlacementEditEnabled", enableBlockPlacementEdit);
            EditorPrefs.SetBool("MT.DB.AutoCornerJumpEnabled", enableAutoJumpToNextCorner);
            BlockPlacementEditor.ToggleDetection(enableBlockPlacementEdit, enableAutoJumpToNextCorner);
        }
        [OnInspectorInit]
        private void LoadKeys()
        {
            string data = EditorPrefs.GetString("MT.DB.PlacementKeyCode", "");
            keys = data.Split(',')
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .Cast<KeyCode>().ToArray();
        }
        private void SaveKeys()
        {
            string data = string.Join(",", keys.Select(k => ((int)k).ToString()));
            EditorPrefs.SetString("MT.DB.PlacementKeyCode", data);
        }

        
        public static ClassicPlacementType GetClassicPlacementType(KeyCode key)
        {
            var win = GetWindow<MakerWindow>();
            return win.keys.Length == 0 ? ClassicPlacementType.Forward :  (ClassicPlacementType)win.keys.First(k => k == key);
        }
    }
}

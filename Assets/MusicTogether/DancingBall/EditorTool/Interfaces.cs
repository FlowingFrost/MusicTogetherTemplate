using System;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    public class Interfaces
    {
        public interface ISelectionWindow
        {
            public void Init(EditorCenter editorCenter);
            public void OnEnabledChanged(bool enabled);
            public void UpdateSelectionInfo(int roadIndex, int blockIndex);
            //public Action<int,int> JumpTo { get; set; }
        }
        public interface ISelector
        {
            public void LookAt(GameObject go);
        }
        
        public interface IMessageReceiver
        {
            void ShowMessage(string msg);
        }
        public interface IKeyboardListener
        {
            
        }
    }
}
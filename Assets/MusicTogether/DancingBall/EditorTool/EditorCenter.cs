using System;
using MusicTogether.DancingBall.Data;
using MusicTogether.DancingBall.Player;
using MusicTogether.DancingBall.Scene;
using Unity.VisualScripting;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    public class EditorCenter
    {
        public static EditorCenter Instance { get; private set; }
        public IMap targetMap;
        public int SelectedRoadIndex { get; private set; }
        public int SelectedBlockIndex { get; private set; }
        public IRoad selectedRoad;
        public IBlock selectedBlock;
        public BallPlayer player;

        private bool IsRoadIndexOutOfRange => targetMap == null || SelectedRoadIndex < 0 || SelectedRoadIndex >= targetMap.Roads.Count;
        private bool IsBlockIndexOutOfRange => selectedRoad == null || SelectedBlockIndex < 0 || SelectedBlockIndex >= selectedRoad.Blocks.Count;

        public Action<string> SendMessage = Debug.Log;
        public Action<int, int> OnSelectionChanged;
        public Action<IRoad> OnRoadSelectionChanged;
        public Action<IBlock, IBlockDisplacementData> OnBlockSelectionChanged;
        public Action<GameObject> LookAtObject;
        
        public void Setup(IMap targetMap, BallPlayer player, int selectedRoadIndex, int selectedBlockIndex)
        {
            this.targetMap = targetMap;
            this.player = player;
            SelectedRoadIndex = selectedRoadIndex;
            SelectedBlockIndex = selectedBlockIndex;
            Instance = this;
            RefreshSelection();
        }
        public void Cleanup()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PreviousBlock()
        {
            SelectedBlockIndex--;
            if (IsBlockIndexOutOfRange) PreviousRoad();
            else RefreshSelection();
        }
        public void NextBlock()
        {
            SelectedBlockIndex++;
            if (IsBlockIndexOutOfRange) NextRoad();
            else RefreshSelection();
        }
        public void PreviousRoad()
        {
            SelectedRoadIndex--;
            SelectedBlockIndex = int.MaxValue;
            RefreshSelection();
        }
        public void NextRoad()
        {
            SelectedRoadIndex++;
            SelectedBlockIndex = 0;
            RefreshSelection();
        }

        public void JumpTo(int roadIndex, int blockIndex = -1)
        {
            SelectedRoadIndex = roadIndex;
            SelectedBlockIndex = blockIndex;
            RefreshSelection();
        }

        public void RefreshSelection()
        {
            OnSelectionChanged?.Invoke(SelectedRoadIndex, SelectedBlockIndex);
            if (targetMap == null)
            {
                SendMessage("Target map is not set.");
                return;
            }

            if (targetMap.Roads == null || targetMap.Roads.Count == 0)
            {
                SendMessage("Target map has no roads.");
                return;
            }

            if (IsRoadIndexOutOfRange)
            {
                SendMessage("Selected road index is out of range.");
                SelectedRoadIndex = SelectedRoadIndex < 0 ? 0 : targetMap.Roads.Count - 1;
            }

            selectedRoad = targetMap.Roads[SelectedRoadIndex];
            OnRoadSelectionChanged?.Invoke(selectedRoad);
            if (selectedRoad == null || selectedRoad.Blocks == null || selectedRoad.Blocks.Count == 0)
            {
                SendMessage("Selected road has no blocks.");
                return;
            }

            if (IsBlockIndexOutOfRange)
            {
                SendMessage("Selected block index is out of range.");
                SelectedBlockIndex = SelectedBlockIndex < 0 ? 0 : selectedRoad.Blocks.Count - 1;
            }

            selectedBlock = selectedRoad.Blocks[SelectedBlockIndex];
            selectedRoad.RoadData.Get_BlockData(selectedBlock.BlockLocalIndex, out var blockData);
            OnBlockSelectionChanged?.Invoke(selectedBlock, blockData);
            if (selectedBlock == null)
            {
                SendMessage("Selected block is null.");
                return;
            }
            
            OnSelectionChanged?.Invoke(SelectedRoadIndex, SelectedBlockIndex);
            LookAtObject?.Invoke(selectedBlock.Transform.gameObject);
        }
    }
}
using System.Collections.Generic;
using MusicTogether.DancingBallArchived.Map;
using MusicTogether.General;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallArchived.Maker
{
    public class BlockMaker : MonoBehaviour
    {
        [Title("Resources")]
        public RoadMaker roadMaker;
        public BlockHolder blockHolder;
        public Block Block => blockHolder.block;
        public List<BlockMaker> BlockMakers => roadMaker.blockMakers;
        public int BlockIndex=> blockHolder.blockIndex;
        //{ get => blockHolder.blockIndex; set => blockHolder.blockIndex = value; }
        public int GlobalIndex=> blockHolder.globalIndex;
        //{ get => blockHolder.globalIndex; set => blockHolder.globalIndex = value; }
        [Title("Data")]
        [SerializeField]private bool customBlockSize;
        [ShowIf("@customBlockSize==true")]
        [SerializeField]private float blockSize;
        public float BlockSize => customBlockSize ? blockSize : roadMaker.RoadDefaultBlockSize;
        
        public bool IsCorner => (blockHolder.isClickPoint&&!blockHolder.jump) || blockHolder.isTurnPoint;
        [SerializeField][ShowIf("@IsCorner==true")]private BlockPlacementData placementData = BlockPlacementData.Default;
        public ref BlockPlacementData PlacementData=>ref CurrentCorner().placementData;

        [Title("Runtime Data")]
        private Vector3 lastPosition,lastEulerAngles;
        private bool editingPosition,editingRotation;


        
        //Pre-Editing Function
        public void Init(RoadMaker _roadMaker,RoadHolder _roadHolder,BlockHolder _blockHolder,GameObject _blockPrefab,int _blockIndex,float _noteIndex,bool _isClickPoint)
        {
            roadMaker = _roadMaker;
            blockHolder = _blockHolder;
            blockHolder.roadHolder = _roadHolder;
            blockHolder.blockIndex = _blockIndex;
            blockHolder.block = ResetBlock(_blockPrefab);
            blockHolder.noteIndex = _noteIndex;
            blockHolder.isClickPoint = _isClickPoint;
        }

        public Block ResetBlock(GameObject blockPrefab)
        {
            return Instantiate(blockPrefab, transform).GetComponent<Block>();
        }

        //Editing Function

        public void CheckModifications()
        {
            if (roadMaker.enablePlacementEdit)
            {
                
                if(BlockIndex==0) return;
                if(transform.position != lastPosition)
                {
                    editingPosition = true; return;
                }
                if (transform.eulerAngles != lastEulerAngles)
                {
                    if(!IsCorner) blockHolder.isTurnPoint = true;
                    editingRotation = true; return;
                }
                if (editingPosition) CurrentCorner().ModifyPlacementByPosition(transform.localPosition);
                if (editingRotation) ModifyPlacementByRotation(transform.localEulerAngles);
            }
            lastPosition = transform.position;
            lastEulerAngles = transform.eulerAngles;
        }
        public void ModifyPlacementByPosition(Vector3 nextPosition)
        {
            PlacementData.ModifyEulerAngles((nextPosition - transform.localPosition).ToEulerAngles());
            if (PlacementData.style == BlockPlacementStyle.Classic)
            {
                PlacementData.CheckClassicType(GetLastMaker(out _).PlacementData);
            }
            roadMaker.UpdateBlockPlacement(BlockIndex);
        }
        public void ModifyPlacementByRotation(Vector3 nextEulerAngles)
        {
            PlacementData.ModifyEulerAngles(nextEulerAngles);
            if (PlacementData.style == BlockPlacementStyle.Classic)
            {
                PlacementData.CheckClassicType(GetLastMaker(out _).PlacementData);
            }
            roadMaker.UpdateBlockPlacement(BlockIndex);
        }

        public void ClassicInput(ClassicPlacementType placementType)
        {
            if(!IsCorner) {CurrentCorner().ClassicInput(placementType); return;}
            BlockPlacementData lastPlacementData = LastCorner(out _).PlacementData;
            Vector3 lastForwardDirection = lastPlacementData.forwardDirection;
            Vector3 lastUpDirection = lastPlacementData.upDirection;
            Vector3 targetPosition = transform.localPosition;
            switch (placementType)
            {
                case ClassicPlacementType.Forward:
                    targetPosition += lastForwardDirection;
                    break;
                case ClassicPlacementType.BackWard:
                    targetPosition -= lastForwardDirection;
                    break;
                case ClassicPlacementType.Left:
                    targetPosition += Vector3.Cross(lastUpDirection, lastForwardDirection);
                    break;
                case ClassicPlacementType.Right:
                    targetPosition += Vector3.Cross(lastForwardDirection,lastUpDirection);
                    break;
                case ClassicPlacementType.Up90:
                    targetPosition += lastUpDirection;
                    break;
                case ClassicPlacementType.Down90:
                    targetPosition -= lastUpDirection;
                    break;
                case ClassicPlacementType.Up45:
                    targetPosition += lastForwardDirection + lastUpDirection;
                    break;
                case ClassicPlacementType.Down45:
                    targetPosition += lastForwardDirection - lastUpDirection;
                    break;
            }
            ModifyPlacementByPosition(targetPosition);
        }
        
        public void ResetTile()
        {
            Block.HideAll();
            Block.downNode.gameObject.SetActive(true);
            if (PlacementData.style == BlockPlacementStyle.Classic)
            {
                switch (PlacementData.classicPlacementType)
                {
                    case ClassicPlacementType.Up45:
                        Block.forwardNode.gameObject.SetActive(true);
                        break;
                    case ClassicPlacementType.Down45:
                        Block.backNode.gameObject.SetActive(true);
                        break;
                }
            }

            if (IsCorner)
            {
                switch (PlacementData.classicPlacementType)
                {
                    case ClassicPlacementType.Up90:
                        Block.backNode.gameObject.SetActive(true);
                        break;
                    case ClassicPlacementType.Down90:
                        Block.forwardNode.gameObject.SetActive(true);
                        break;
                }
            }
        }
        //Editing Tool
        private BlockMaker GetNextMaker(out bool result)
        {
            if(BlockIndex == BlockMakers.Count - 1){result = false; return this;}
            result = true; return BlockMakers[BlockIndex + 1];
        }
        private BlockMaker GetLastMaker(out bool result)
        {
            if (BlockIndex == 0) {result = false; return this; }
            result = true; return BlockMakers[BlockIndex-1];
        }

        private BlockMaker CurrentCorner()
        {
            var maker = this; bool hasLastMaker = true;
            while (!maker.IsCorner && hasLastMaker)
            { maker = maker.GetLastMaker(out hasLastMaker); }
            return maker;
        }

        public BlockMaker LastCorner(out bool result)
        {
            var maker = CurrentCorner();
            maker = maker.GetLastMaker(out result);
            if(!result) return maker;
            return maker.CurrentCorner();
        }

        public BlockMaker NextCorner(out bool result)
        {
            var maker = this;
            do
            {
                maker = maker.GetLastMaker(out result);
            } while (result&&!maker.IsCorner);
            return maker;
        }
    }
}


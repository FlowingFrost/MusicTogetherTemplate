using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Interfaces;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace MusicTogether.DancingBall
{
    public class EditManager : SerializedBehaviour
    {
        //绑定信息
        [SerializeField] protected IMap map;
        [SerializeField] protected IDisplacementApplyer displacementApplyer;
        [SerializeField] protected IFactory Factory;
        //API
        public static EditManager Instance { get; private set; }
        
        public IRoad GetRoadByBlock(IBlock block) => map.GetRoadByBlock(block);

        public void IncreaseBlockCount(IRoadMaker targetRoadMaker, int oldCount, int newCount)
        {
            var targetRoad = targetRoadMaker.Road;
            var newBlocks = Factory.CreateBlocks(targetRoad, oldCount, newCount - 1);
            targetRoad.Blocks.AddRange(newBlocks);
            if (targetRoad.Blocks.Count != newCount)
            {
                FixBlockList(targetRoadMaker, newCount);
            }
        }

        
        /// <summary>
        /// 列表中间存在丢失的项时，使用此操作补齐。
        /// </summary>
        /// <param name="targetRoadMaker"></param>
        /// <param name="targetCount"></param>
        public void FixBlockList(IRoadMaker targetRoadMaker, int targetCount)
        {
            
        }
        public void OnDisplacementChanged(IBlock targetBlock)
        {
            var targetRoad = map.GetRoadByBlock(targetBlock);
            if (targetRoad == null) return;
            //List<IBlock> blocksBehind = targetRoad.BlocksBehind(targetBlock).ToList();
            if (targetBlock.IndexInRoad == 0) 
            { targetBlock.Transform.localPosition = Vector3.zero; targetBlock.Transform.localRotation = Quaternion.identity; }
            var blocksToBeProcessed = targetRoad.BlockBehindSplitByCorner(targetBlock);
            blocksToBeProcessed.ForEach(bs=> ApplyDisplacement(bs.ToList(), targetRoad));
        }
        public void ApplyDisplacement(List<IBlock> blocksToBeProcessed, IRoad targetRoad)
        {
            blocksToBeProcessed.Sort((a,b)=>a.IndexInRoad.CompareTo(b.IndexInRoad));
            var root = blocksToBeProcessed.First();
            var previousBlock = targetRoad.PreviousBlock(root);
            displacementApplyer.ApplyDisplacement(blocksToBeProcessed, previousBlock);
        }
    }
}
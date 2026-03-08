using System;
using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Interfaces;
using UnityEngine;

namespace MusicTogether.DancingBall.Classic
{
    public class ClassicDisplacementApplyer : IDisplacementApplyer
    {
        /// <summary>
        /// 应用位移规则到目标方块上，从自己算起。
        /// </summary>
        /// <param name="targetBlocks"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ApplyDisplacement(List<IBlock> targetBlocks, IBlock previousBlock)
        {
            if (targetBlocks is null) return;
            var rootBlock = targetBlocks.First();
            var rootData = rootBlock.BlockMaker;
            if (rootData.HasTap)
            {
                Quaternion prevRotation = previousBlock == null? Quaternion.identity : previousBlock.Transform.localRotation;
                Quaternion newRotation = prevRotation * rootData.TurnType switch
                {
                    TurnType.Left => Quaternion.Euler(0, -90, 0),
                    TurnType.Right => Quaternion.Euler(0, 90, 0),
                    _ => Quaternion.identity
                };
                rootBlock.Transform.localRotation = newRotation;
                var direction = rootBlock.Transform.forward;
                var startPosition = rootBlock.Transform.localPosition;

                foreach (var targetBlock in targetBlocks)
                {
                    int indexDiff = targetBlock.IndexInRoad - rootBlock.IndexInRoad;
                    targetBlock.Transform.localPosition = startPosition + direction * indexDiff;
                    targetBlock.Transform.localRotation = newRotation;
                }
            }
            else
            {
                var direction = rootBlock.Transform.forward + rootData.DisplacementType switch
                {
                    DisplacementType.Down => -rootBlock.Transform.up,
                    DisplacementType.Up => rootBlock.Transform.up,
                    _ => Vector3.zero
                };
                
                var startPosition = rootBlock.Transform.localPosition;
                var rotation = rootBlock.Transform.localRotation;

                foreach (var targetBlock in targetBlocks)
                {
                    int indexDiff = targetBlock.IndexInRoad - rootBlock.IndexInRoad;
                    targetBlock.Transform.localPosition = startPosition + direction * indexDiff;
                    targetBlock.Transform.localRotation = rotation;
                }
            }
        }

        public void ApplyTileStyle(IEnumerable<IBlock> targetBlocks)
        {
            
        }
    }
}
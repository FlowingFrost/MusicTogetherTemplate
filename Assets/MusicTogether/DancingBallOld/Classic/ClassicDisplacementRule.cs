using System.Collections.Generic;
using MusicTogether.DancingBallOld.Interfaces;
using UnityEngine;

namespace MusicTogether.DancingBallOld.Classic
{
    /// <summary>
    /// 经典模式位移规则。
    /// 从 Road.PlacementData 中读取放置数据，不依赖 Block 上的序列化属性。
    /// </summary>
    public class ClassicDisplacementRule : IDisplacementApplyer
    {
        /// <summary>
        /// 对一段连续方块应用位移规则。
        /// 段首 Block 的 BlockEntry 决定本段的转弯方向或竖向位移。
        /// </summary>
        public void ApplyDisplacement(List<IBlock> targetBlocks, IBlock previousBlock, IRoad road)
        {
            if (targetBlocks == null || targetBlocks.Count == 0) return;

            var rootBlock = targetBlocks[0];
            var rootEntry = road.GetBlockEntry(rootBlock);

            if (rootEntry.HasTap)
            {
                // ── 转弯规则 ──────────────────────────────────────────
                Quaternion prevRotation = previousBlock != null
                    ? previousBlock.Transform.localRotation
                    : Quaternion.identity;

                Quaternion turnDelta = rootEntry.turnType switch
                {
                    TurnType.Left  => Quaternion.Euler(0, -90, 0),
                    TurnType.Right => Quaternion.Euler(0,  90, 0),
                    _              => Quaternion.identity
                };

                Quaternion newRotation = prevRotation * turnDelta;
                Vector3 direction = newRotation * Vector3.forward;
                Vector3 startPosition = rootBlock.Transform.localPosition;

                foreach (var block in targetBlocks)
                {
                    int diff = block.IndexInRoad - rootBlock.IndexInRoad;
                    block.Transform.localPosition = startPosition + direction * diff;
                    block.Transform.localRotation = newRotation;
                }
            }
            else
            {
                // ── 竖向位移规则 ──────────────────────────────────────
                Quaternion rotation = rootBlock.Transform.localRotation;
                Vector3 forward = rotation * Vector3.forward;
                Vector3 verticalDelta = rootEntry.displacementType switch
                {
                    DisplacementType.Up   =>  rootBlock.Transform.up,
                    DisplacementType.Down => -rootBlock.Transform.up,
                    _                     =>  Vector3.zero
                };

                Vector3 direction = forward + verticalDelta;
                Vector3 startPosition = rootBlock.Transform.localPosition;

                foreach (var block in targetBlocks)
                {
                    int diff = block.IndexInRoad - rootBlock.IndexInRoad;
                    block.Transform.localPosition = startPosition + direction * diff;
                    block.Transform.localRotation = rotation;
                }
            }
        }
    }
}

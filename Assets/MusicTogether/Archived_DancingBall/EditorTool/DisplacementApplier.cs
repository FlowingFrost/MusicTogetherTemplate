using System.Collections.Generic;
using System.Linq;
using MusicTogether.Archived_DancingBall.DancingBall;
using MusicTogether.Archived_DancingBall.Scene;
using UnityEngine;

namespace MusicTogether.Archived_DancingBall.EditorTool
{
    public class DisplacementApplier : MonoBehaviour
    {
        public void ApplyDisplacement(Map targetMap, List<Block> targetBlocks, Vector3 startPosition, Quaternion previousRotation)
        {
            if (targetBlocks == null || targetBlocks.Count == 0) return;
            if (targetMap == null || targetMap.SceneData == null) return;

            // 取根块数据（按本路局部索引排序，第一块作为根）
            targetBlocks.Sort((a, b) => a.blockLocalIndex.CompareTo(b.blockLocalIndex));
            var rootBlock = targetBlocks[0];
            int rootBlockLocalIndex = rootBlock.blockLocalIndex;
            int roadIndex = rootBlock.road.RoadIndex;

            targetMap.SceneData.GetBlockData(roadIndex, rootBlockLocalIndex, out var blockData);
            var rootTurnType = blockData.turnType;
            var rootDisplacementType = blockData.displacementType;

            int displacementID = (int)rootTurnType * 10 + (int)rootDisplacementType;
            Quaternion deltaRotation = rootTurnType switch
            {
                TurnType.Left => Quaternion.Euler(0, -90, 0),
                TurnType.Right => Quaternion.Euler(0, 90, 0),
                _ => Quaternion.identity
            };
            deltaRotation *= rootDisplacementType switch
            {
                DisplacementType.Up => Quaternion.Euler(-90, 0, 0),
                DisplacementType.Down => Quaternion.Euler(90, 0, 0),
                _ => Quaternion.identity
            };

            var transformList = targetBlocks.ConvertAll(b => b.transform);
            switch (displacementID)
            {
                case 20: goto BottomTileWithNormalDisplacement;
                case 30: goto BottomTileWithNormalDisplacement;
                case 21: goto Upward;
                case 31: goto Upward;
                case 40: goto Jump;
                case 01: goto Upward;
                case 02: goto Downward;
                case 03: goto UpwardStair;
                case 04: goto DownStair;

                Upward:
                    ApplyTile(new List<TileHolder> { rootBlock.tileHolder }, false, true, true);
                    goto SkipFirstThenBottomTileWithNormalDisplacement;
                Downward:
                    ApplyTile(new List<TileHolder> { rootBlock.tileHolder }, false, false, false);
                    goto SkipFirstThenBottomTileWithNormalDisplacement;
                Jump:
                    ApplyBottomTile(targetBlocks.Select(b => b.tileHolder).ToList());
                    var emptyHolders = targetBlocks
                        .Where(b => b.blockLocalIndex > rootBlockLocalIndex && b.blockLocalIndex <= rootBlockLocalIndex + 1)
                        .Select(b => b.tileHolder).ToList();
                    ApplyEmptyTile(emptyHolders);
                    ApplyLineDisplacement(transformList, startPosition, previousRotation);
                    break;
                UpwardStair:
                    var holdersUS = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyTile(holdersUS, true, false, true);
                    ApplyBottomTile(new List<TileHolder> { holdersUS.Last() });
                    ApplyStairDisplacement(transformList, startPosition, previousRotation, true);
                    break;
                DownStair:
                    var holdersDS = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyTile(holdersDS, false, true, true);
                    ApplyBottomTile(new List<TileHolder> { holdersDS.First() });
                    ApplyStairDisplacement(transformList, startPosition, previousRotation, false);
                    break;
                BottomTileWithNormalDisplacement:
                    ApplyBottomTile(targetBlocks.Select(b => b.tileHolder).ToList());
                    goto NormalDisplacement;
                SkipFirstThenBottomTileWithNormalDisplacement:
                    ApplyBottomTile(targetBlocks.Skip(1).Select(b => b.tileHolder).ToList());
                    goto NormalDisplacement;
                NormalDisplacement:
                    ApplyLineDisplacement(transformList, startPosition, previousRotation * deltaRotation);
                    break;
                default:
                    ApplyLineDisplacement(transformList, startPosition, previousRotation);
                    ApplyBottomTile(targetBlocks.Skip(1).Select(b => b.tileHolder).ToList());
                    break;
            }
        }

        private void ApplyLineDisplacement(List<Transform> blockTransforms, Vector3 startPosition, Quaternion rotation)
        {
            for (int i = 0; i < blockTransforms.Count; i++)
            {
                var t = blockTransforms[i];
                t.localPosition = startPosition + rotation * Vector3.forward * i;
                t.localRotation = rotation;
            }
        }

        private void ApplyStairDisplacement(List<Transform> blockTransforms, Vector3 startPosition, Quaternion rotation, bool isUpward)
        {
            for (int i = 0; i < blockTransforms.Count; i++)
            {
                var t = blockTransforms[i];
                t.localPosition = startPosition + rotation * (Vector3.forward + (isUpward ? Vector3.up : Vector3.down)) * i;
                t.localRotation = rotation;
            }
        }

        private void ApplyTile(List<TileHolder> tileHolders, bool forward, bool backward, bool bottom)
        {
            if (tileHolders == null || tileHolders.Count == 0) return;
            foreach (var tileHolder in tileHolders)
            {
                tileHolder.SetTileActive(forward, backward, bottom);
            }
        }

        private void ApplyBottomTile(List<TileHolder> tileHolders) => ApplyTile(tileHolders, false, false, true);
        private void ApplyEmptyTile(List<TileHolder> tileHolders) => ApplyTile(tileHolders, false, false, false);
    }
}

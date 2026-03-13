using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Archived_SceneMap;
using UnityEngine;

namespace MusicTogether.DancingBall.Archived_EditorTool
{
    public class DisplacementApplier : MonoBehaviour
    {
        public void ApplyDisplacement(Map targetMap,List<Block> targetBlocks,Vector3 startPosition, Quaternion previousRotation)
        {
            if (targetBlocks == null || targetBlocks.Count == 0) return;
            
            var mapData = targetMap.mapData;
            var rootBlock = targetBlocks[0];
            int rootBlockGlobalIndex = rootBlock.globalBlockIndex;
            mapData.GetBlockData_ByBlockGlobalIndex(rootBlockGlobalIndex, out var blockData);
            // GetBlockData 不存在时返回默认 BlockData（TurnType.None + DisplacementType.None），继续按普通 block 排列

            var rootTurnType = blockData.turnType;
            var rootDisplacementType = blockData.displacementType;
            // Block所有的拐弯模式：(TurnType + DisplacementType) 的组合
            // 1.需要点击转向：
            // Left/Right + None：           20/30/ 在当前水平面内分别向左/右转90度  RootTile:BTM
            // Left/right + Up：             21/31 左右转90°+上转90°             RootTile:BTM + backward
            // Jump + None：                 40    跳跃，继续向前                 RootTile:BTM 后1个：不显示任何方块
            // 2.不需要点击
            // None + Up：                   01    向上转90°                     RootTile:BTM + backward
            // None + Down：                 02    向下转90°                     RootTile:None
            // None + ForwardUp/ForwardDown：03/04 向上/下转45°(并不真正旋转45°)   RootTile:Up=>BTM+forward, Down=>BTM  All:BTM + Up=>forward, Down=>backward
            // 其余组合不合法，视为 None + None（普通方块）

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
            targetBlocks.Sort((a, b) => a.globalBlockIndex.CompareTo(b.globalBlockIndex));
            var transformList = targetBlocks.ConvertAll(b => b.transform);
            switch (displacementID)
            {
                //左右转，仍然只需要底部的地板
                case 20: goto BottomTileWithNormalDisplacement;
                case 30: goto BottomTileWithNormalDisplacement;
                //左右转+上转，除了底部地板之外，还需要rootBlock显示背面地板（backward）
                case 21: goto Upward;
                case 31: goto Upward;
                //向前跳跃，虽然不需要旋转，但需要特殊处理地板显示
                case 40: goto Jump;
                //向上转
                case 01: goto Upward;
                //向下转
                case 02: goto Downward;
                case 03: goto UpwardStair;
                case 04: goto DownStair;
                //上转，除了底部地板之外，还需要rootBlock显示背面地板（backward）
                Upward:
                    var rootBlockListU = new List<TileHolder>(){rootBlock.tileHolder};
                    ApplyTile(rootBlockListU, false, true, true);
                    goto SkipFirstThenBottomTileWithNormalDisplacement;
                Downward:
                    var rootBlockListD = new List<TileHolder>(){rootBlock.tileHolder};
                    ApplyEmptyTile(rootBlockListD);
                    goto SkipFirstThenBottomTileWithNormalDisplacement;
                Jump:
                    //向前跳跃
                    //先将所有block设置默认的地板，即底部地板
                    var tileHoldersToApplyBottom = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyBottomTile(tileHoldersToApplyBottom);
                    //然后把起跳点之后的1s个block设置为空地板
                    var tileHoldersToApplyEmpty = targetBlocks.Where(b => b.globalBlockIndex <= rootBlockGlobalIndex + 1 && b.globalBlockIndex > rootBlockGlobalIndex).Select(b => b.tileHolder).ToList();
                    ApplyEmptyTile(tileHoldersToApplyEmpty);
                    //最后再进行正常的位移
                    ApplyLineDisplacement(transformList, startPosition, previousRotation);
                    break;
                UpwardStair:
                    var tileHoldersUS = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyTile(tileHoldersUS, true, false, true);
                    var lsatTileHolderList = new List<TileHolder>(){tileHoldersUS.Last()};
                    ApplyBottomTile(lsatTileHolderList);
                    ApplyStairDisplacement(transformList, startPosition, previousRotation, true);
                    break;
                DownStair:
                    var tileHoldersDS = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyTile(tileHoldersDS, false, true, true);
                    var firstTileHolderList = new List<TileHolder>(){tileHoldersDS.First()};
                    ApplyBottomTile(firstTileHolderList);
                    ApplyStairDisplacement(transformList, startPosition, previousRotation, false);
                    break;
                BottomTileWithNormalDisplacement:
                    var tileHoldersBTWND = targetBlocks.Select(b => b.tileHolder).ToList();
                    ApplyBottomTile(tileHoldersBTWND);
                    goto NormalDisplacement;
                SkipFirstThenBottomTileWithNormalDisplacement:
                    var tileHoldersSFTBTWND = targetBlocks.Select(b => b.tileHolder).Skip(1).ToList();
                    ApplyBottomTile(tileHoldersSFTBTWND);
                    goto NormalDisplacement;
                NormalDisplacement:
                    ApplyLineDisplacement(transformList, startPosition, previousRotation * deltaRotation);
                    break;
                default: 
                    ApplyLineDisplacement(transformList, startPosition, previousRotation);
                    var tileHoldersD = targetBlocks.Skip(1).Select(b => b.tileHolder).ToList();
                    ApplyBottomTile(tileHoldersD);
                    break;
            }
        }

        private void ApplyLineDisplacement(List<Transform> blockTransforms, Vector3 startPosition, Quaternion rotation)
        {
            for (int i = 0; i < blockTransforms.Count; i++)
            {
                var blockTransform = blockTransforms[i];
                blockTransform.localPosition = startPosition + rotation * Vector3.forward * i;
                blockTransform.localRotation = rotation;
            }
        }
        private void ApplyStairDisplacement(List<Transform> blockTransforms, Vector3 startPosition, Quaternion rotation, bool isUpward)
        {
            for (int i = 0; i < blockTransforms.Count; i++)
            {
                var blockTransform = blockTransforms[i];
                blockTransform.localPosition = startPosition + rotation * (Vector3.forward + (isUpward ? Vector3.up : Vector3.down)) * i;
                blockTransform.localRotation = rotation;
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
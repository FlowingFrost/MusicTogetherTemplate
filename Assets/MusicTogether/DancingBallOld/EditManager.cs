using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBallOld.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MusicTogether.DancingBallOld
{
    /// <summary>
    /// 地图编辑管理器。
    /// 负责协调 Block 数量变更和位移计算，数据直接写入各 Road 的
    /// <see cref="BlockPlacementData"/>，场景中的 Block GameObject 只负责呈现。
    /// </summary>
    public class EditManager : SerializedBehaviour
    {
        // ── 绑定 ─────────────────────────────────────────────────────
        [SerializeField] protected IMap map;
        [SerializeField] protected IDisplacementApplyer displacementRule;
        [SerializeField] protected IFactory factory;

        public static EditManager Instance { get; private set; }

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[EditManager] 场景中存在多个 EditManager，将销毁多余实例。");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // ── 快捷查询 ─────────────────────────────────────────────────

        public IRoad GetRoadByBlock(IBlock block) => map.GetRoadByBlock(block);

        // ── Block 数量管理 ────────────────────────────────────────────

        /// <summary>
        /// 将 Road 的 Block 数量从 oldCount 扩展到 newCount。
        /// 同时在 PlacementData 中补足缺失的默认条目。
        /// </summary>
        public void IncreaseBlockCount(IRoadMaker targetRoadMaker, int oldCount, int newCount)
        {
            var road = targetRoadMaker.Road;
            // 1. 创建新的场景 Block 对象
            var newBlocks = factory.CreateBlocks(road, oldCount, newCount - 1).ToList();
            road.Blocks.AddRange(newBlocks);
            // 2. 补齐 PlacementData（不覆盖已有条目）
            road.PlacementData?.EnsureCapacity(newCount);
            // 3. 修复可能存在的数量不一致
            if (road.Blocks.Count != newCount)
                FixBlockList(targetRoadMaker, newCount);
        }

        /// <summary>
        /// 将 Road 的 Block 数量缩减到 newCount。
        /// 销毁多余的场景对象，并截断 PlacementData。
        /// </summary>
        public void DecreaseBlockCount(IRoadMaker targetRoadMaker, int newCount)
        {
            var road = targetRoadMaker.Road;
            while (road.Blocks.Count > newCount)
            {
                var last = road.Blocks[^1];
                road.Blocks.RemoveAt(road.Blocks.Count - 1);
                if (last.Transform != null)
                    Object.DestroyImmediate(last.Transform.gameObject);
            }
            road.PlacementData?.Truncate(newCount);
        }

        /// <summary>
        /// 列表中间存在丢失的项时，补齐至 targetCount。
        /// </summary>
        public void FixBlockList(IRoadMaker targetRoadMaker, int targetCount)
        {
            var road = targetRoadMaker.Road;
            for (int i = 0; i < targetCount; i++)
            {
                if (i >= road.Blocks.Count || road.Blocks[i] == null)
                {
                    var block = factory.CreateBlock(road, i);
                    if (i < road.Blocks.Count)
                        road.Blocks[i] = block;
                    else
                        road.Blocks.Add(block);
                }
                road.Blocks[i].IndexInRoad = i;
            }
            road.PlacementData?.EnsureCapacity(targetCount);
        }

        // ── 放置数据编辑 ──────────────────────────────────────────────

        /// <summary>
        /// 修改某个 Block 的放置数据（转弯/位移），并触发位置重算。
        /// </summary>
        public void SetBlockEntry(IBlock targetBlock, BlockEntry newEntry)
        {
            var road = map.GetRoadByBlock(targetBlock);
            if (road == null) return;
            road.PlacementData?.SetEntry(newEntry);
            OnDisplacementChanged(targetBlock);
        }

        // ── 位移计算 ──────────────────────────────────────────────────

        /// <summary>
        /// 当 targetBlock 的放置数据（转弯/位移）发生变化时调用。
        /// 重新计算从 targetBlock 起的所有后续 Block 的位置/旋转。
        /// </summary>
        public void OnDisplacementChanged(IBlock targetBlock)
        {
            var road = map.GetRoadByBlock(targetBlock);
            if (road == null) return;

            // 第一个 Block 始终在本地原点
            if (targetBlock.IndexInRoad == 0)
            {
                targetBlock.Transform.localPosition = Vector3.zero;
                targetBlock.Transform.localRotation = Quaternion.identity;
            }

            // 按 Corner 分段，对每段应用位移规则
            var segments = road.BlocksBehindSplitByCorner(targetBlock);
            foreach (var segment in segments)
            {
                var segList = segment.ToList();
                if (segList.Count == 0) continue;
                ApplyDisplacement(segList, road);
            }
        }

        /// <summary>
        /// 对一段连续 Block 应用位移规则（从段首的前一个 Block 作为参照）。
        /// </summary>
        public void ApplyDisplacement(List<IBlock> segment, IRoad road)
        {
            segment.Sort((a, b) => a.IndexInRoad.CompareTo(b.IndexInRoad));
            var root = segment[0];
            var previousBlock = road.PreviousBlock(root);
            displacementRule.ApplyDisplacement(segment, previousBlock, road);
        }

        /// <summary>Road Block 数量范围变化时的回调。</summary>
        public void OnBlockRangeChanged(IRoadMaker targetRoadMaker)
        {
            var road = targetRoadMaker.Road;
            int target = targetRoadMaker.BlockEndIndex - targetRoadMaker.BlockStartIndex + 1;
            int current = road.Blocks.Count;
            if (target > current)
                IncreaseBlockCount(targetRoadMaker, current, target);
            else if (target < current)
                DecreaseBlockCount(targetRoadMaker, target);
            // 重新计算整条 Road 的位移
            if (road.FirstBlock != null)
                OnDisplacementChanged(road.FirstBlock);
        }
    }
}

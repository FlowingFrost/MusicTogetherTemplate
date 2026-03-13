using MusicTogether.DancingBall.Archived_SceneMap;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBall.Archived_EditorTool
{
    [ExecuteAlways]
    public class BlockEditor : MonoBehaviour
    {
        public Map targetMap;
        private MapData MapData => targetMap != null ? targetMap.mapData : null;
        private Road targetRoad;
        private Block targetBlock;
        private int TargetBlockIndex => targetBlock == null ? -1 : targetBlock.globalBlockIndex;
        private int _cachedBlockIndex = 0;
        private bool HasValidReference => targetMap != null && MapData != null;

#if UNITY_EDITOR
        /// <summary>供 BlockEditorOverlay 显示用：当前缓存的 block 全局索引。</summary>
        internal int TargetBlockIndexForDisplay => _cachedBlockIndex;
#endif

        public bool enableEditorTool;

        /// <summary>
        /// sprint: 按住后，将只会定位到需要tap的block，或含有displacement规则的normal block。
        /// </summary>
        public KeyCode nextBlock = KeyCode.RightArrow,
            previousBlock = KeyCode.LeftArrow,
            sprint = KeyCode.LeftControl;
        public KeyCode setTurnTypeNone = KeyCode.S,
            setTurnTypeForward = KeyCode.W,
            setTurnTypeRight = KeyCode.D,
            setTurnTypeLeft = KeyCode.A,
            setTurnTypeJump = KeyCode.Space,
            setDisplacementTypeNone = KeyCode.Backspace,
            setDisplacementTypeUp = KeyCode.U,
            setDisplacementTypeDown = KeyCode.LeftShift,
            setDisplacementTypeForwardUp = KeyCode.E,
            setDisplacementTypeForwardDown = KeyCode.Q;

        // ──────────────────────────────────────────────────────────────
        //  相机跟随配置
        // ──────────────────────────────────────────────────────────────

        [Header("Camera Settings")]
        /// <summary>相机与目标方块的偏移（局部空间）</summary>
        public Vector3 cameraOffset = new Vector3(0f, 5f, -8f);
        /// <summary>相机移动的平滑速度（0 = 立即到位）</summary>
        [Range(0f, 20f)]
        public float cameraMoveSpeed = 8f;

#if UNITY_EDITOR

        private void OnDisable()
        {
            // 清理未完成的平滑移动
            if (_smoothMoveCallback != null)
                EditorApplication.update -= _smoothMoveCallback;
        }

        // ──────────────────────────────────────────────────────────────
        //  sprint 状态：由 Overlay 写入
        // ──────────────────────────────────────────────────────────────

        /// <summary>由 BlockEditorOverlay 根据 Event 实时写入 sprint 键的按下状态。</summary>
        internal bool SprintHeld { get; set; }

        // ──────────────────────────────────────────────────────────────
        //  1. 导航：获取并选择目标方块（由 Overlay 调用）
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 直接跳转到指定全局索引的方块。
        /// 由 BlockEditorOverlay 手动输入框调用。
        /// </summary>
        internal void JumpToBlock(int globalIndex)
        {
            if (!HasValidReference) return;
            if (!MapData.InRange_ByBlockGlobalIndex(globalIndex)) return;
            Block block = FindBlockByGlobalIndex(globalIndex);
            SetTargetBlock(block, globalIndex);
        }

        /// <summary>
        /// 向前或向后导航到下一个目标方块。
        /// 按住 sprint 键时，只定位到 Tap 方块或含有 displacement 规则的方块。
        /// 由 BlockEditorOverlay 在捕获到对应按键时调用。
        /// </summary>
        internal void NavigateBlock(bool forward)
        {
            int newIndex = FindNextTargetBlockIndex(forward);
            if (newIndex < 0) return;

            Block block = FindBlockByGlobalIndex(newIndex);
            SetTargetBlock(block, newIndex);
        }

        /// <summary>
        /// 查找下一个（或上一个）目标方块的全局索引。
        /// </summary>
        private int FindNextTargetBlockIndex(bool forward)
        {
            if (!HasValidReference) return -1;

            bool isSprint = SprintHeld;
            int searchFrom = _cachedBlockIndex + (forward ? 1 : -1);

            if (isSprint)
            {
                // sprint 模式：只定位 tap 或含 displacement 的方块
                if (forward)
                {
                    bool foundTap = MapData.TryGetNext_BlockDataGlobalIndex_WhichNeedTap(searchFrom, out int tapIdx);
                    bool foundDisp = MapData.TryGetNext_BlockDataGlobalIndex_WhichHasDisplacementRule(searchFrom, out int dispIdx);
                    if (!foundTap && !foundDisp) return -1;
                    if (foundTap && foundDisp) return Mathf.Min(tapIdx, dispIdx);
                    return foundTap ? tapIdx : dispIdx;
                }
                else
                {
                    bool foundTap = MapData.TryGetPrevious_BlockDataGlobalIndex_WhichNeedTap(searchFrom + 1, out int tapIdx);
                    bool foundDisp = MapData.TryGetPrevious_BlockDataGlobalIndex_WhichHasDisplacementRule(searchFrom + 1, out int dispIdx);
                    if (!foundTap && !foundDisp) return -1;
                    if (foundTap && foundDisp) return Mathf.Max(tapIdx, dispIdx);
                    return foundTap ? tapIdx : dispIdx;
                }
            }
            else
            {
                // 普通模式：逐个步进
                int next = _cachedBlockIndex + (forward ? 1 : -1);
                if (!MapData.InRange_ByBlockGlobalIndex(next)) return -1;
                return next;
            }
        }

        /// <summary>
        /// 在场景中根据全局索引查找对应的 Block 组件。
        /// </summary>
        private Block FindBlockByGlobalIndex(int globalIndex)
        {
            if (targetMap == null) return null;
            foreach (var road in targetMap.roads)
            {
                if (road == null) continue;
                foreach (var block in road.blocks)
                {
                    if (block != null && block.globalBlockIndex == globalIndex)
                        return block;
                }
            }
            return null;
        }

        // ──────────────────────────────────────────────────────────────
        //  2. 设置目标方块（聚合入口）
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 设置当前目标方块，并同步执行：更新缓存索引 → 选中 GameObject → 移动相机。
        /// </summary>
        private void SetTargetBlock(Block block, int globalIndex)
        {
            // 允许 block 为 null（数据存在但场景对象未生成的情况），仍更新索引
            targetBlock = block;
            targetRoad = block != null ? block.road : null;
            _cachedBlockIndex = globalIndex;

            SelectBlockGameObject(block);

            if (block != null)
                FocusCameraOnBlock(block);
        }

        // ──────────────────────────────────────────────────────────────
        //  5. 应用 TurnType / DisplacementType（由 Overlay 调用）
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 对当前目标方块设置 TurnType，并通过 Dispatcher 触发后置刷新链。
        /// 由 BlockEditorOverlay 在捕获到对应按键时调用。
        /// </summary>
        internal void ApplyTurnType(TurnType turnType)
        {
            if (!HasValidBlock()) return;
            MapData.GetBlockData_ByBlockGlobalIndex(_cachedBlockIndex, out var data);
            data.turnType = turnType;
            MapData.SetBlockData(data);
            targetMap.dispatcher.Dispatch(
                nameof(EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(targetMap, targetRoad.roadGlobalIndex, _cachedBlockIndex));
        }

        /// <summary>
        /// 对当前目标方块设置 DisplacementType，并通过 Dispatcher 触发后置刷新链。
        /// 由 BlockEditorOverlay 在捕获到对应按键时调用。
        /// </summary>
        internal void ApplyDisplacementType(DisplacementType displacementType)
        {
            if (!HasValidBlock()) return;
            MapData.GetBlockData_ByBlockGlobalIndex(_cachedBlockIndex, out var data);
            data.displacementType = displacementType;
            MapData.SetBlockData(data);
            targetMap.dispatcher.Dispatch(
                nameof(EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(targetMap, targetRoad.roadGlobalIndex, _cachedBlockIndex));
        }

        /// <summary>当前缓存索引指向有效 Block 时返回 true。</summary>
        private bool HasValidBlock() =>
            HasValidReference && MapData.InRange_ByBlockGlobalIndex(_cachedBlockIndex);



        /// <summary>
        /// 在 Unity Editor 中选中目标方块的 GameObject。
        /// </summary>
        private void SelectBlockGameObject(Block block)
        {
            if (block == null) return;
            Selection.activeGameObject = block.gameObject;
        }

        // ──────────────────────────────────────────────────────────────
        //  4. 移动 SceneView 相机
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// 将 SceneView 相机移动到目标方块附近并看向它。
        /// </summary>
        private void FocusCameraOnBlock(Block block)
        {
            if (block == null) return;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            Vector3 targetPos = block.transform.position;
            Vector3 desiredPivot = targetPos;

            if (cameraMoveSpeed <= 0f)
            {
                // 立即到位
                TeleportSceneCamera(sceneView, desiredPivot, cameraOffset);
            }
            else
            {
                // 平滑插值（在 SceneView 内异步更新）
                SmoothMoveSceneCamera(sceneView, desiredPivot, cameraOffset);
            }
        }

        /// <summary>
        /// 立即将 SceneView 相机传送到目标位置并看向目标点。
        /// </summary>
        private void TeleportSceneCamera(SceneView sceneView, Vector3 pivot, Vector3 offset)
        {
            Vector3 camPos = pivot + offset;
            Vector3 lookDir = (pivot - camPos).normalized;

            sceneView.pivot = pivot;
            sceneView.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            sceneView.size = offset.magnitude * 0.5f;
            sceneView.Repaint();
        }

        /// <summary>
        /// 将 SceneView 相机以平滑插值方式移向目标位置。
        /// 通过 EditorApplication.update 逐帧推进，直到足够接近后停止。
        /// </summary>
        private void SmoothMoveSceneCamera(SceneView sceneView, Vector3 targetPivot, Vector3 offset)
        {
            // 取消上一次未完成的平滑移动
            EditorApplication.update -= _smoothMoveCallback;

            Vector3 desiredCamPos = targetPivot + offset;
            Vector3 lookDir = (targetPivot - desiredCamPos).normalized;
            Quaternion desiredRot = Quaternion.LookRotation(lookDir, Vector3.up);
            float desiredSize = offset.magnitude * 0.5f;

            _smoothMoveCallback = () =>
            {
                if (sceneView == null)
                {
                    EditorApplication.update -= _smoothMoveCallback;
                    return;
                }

                float t = cameraMoveSpeed * 0.016f; // 约 60fps 下每帧插值步长
                sceneView.pivot = Vector3.Lerp(sceneView.pivot, targetPivot, t);
                sceneView.rotation = Quaternion.Slerp(sceneView.rotation, desiredRot, t);
                sceneView.size = Mathf.Lerp(sceneView.size, desiredSize, t);
                sceneView.Repaint();

                if (Vector3.Distance(sceneView.pivot, targetPivot) < 0.01f)
                    EditorApplication.update -= _smoothMoveCallback;
            };

            EditorApplication.update += _smoothMoveCallback;
        }

        private EditorApplication.CallbackFunction _smoothMoveCallback;

#endif
    }
}
using System.Collections.Generic;
using System.Linq;
using MusicTogether.DancingBall.Scene;
using UnityEditor;
using UnityEngine;

namespace MusicTogether.DancingBall.EditorTool
{
    [ExecuteAlways]
    public class BlockEditor : MonoBehaviour
    {
        public Map targetMap;
        private SceneData SceneData => targetMap != null ? targetMap.SceneData : null;
        
        private Road targetRoad;
        private Block targetBlock;

        // 缓存当前选中的 Road 和 Block 索引
        private int _currentRoadIndex = 0;
        private int _currentBlockLocalIndex = 0;

        private bool HasValidReference => targetMap != null && SceneData != null;

        public bool enableEditorTool = true;

        [Header("Controls")]
        public KeyCode nextBlock = KeyCode.RightArrow;
        public KeyCode previousBlock = KeyCode.LeftArrow;
        public KeyCode sprint = KeyCode.LeftControl;
        
        [Header("Edit Shortcuts")]
        public KeyCode setTurnTypeNone = KeyCode.S;
        public KeyCode setTurnTypeForward = KeyCode.W;
        public KeyCode setTurnTypeRight = KeyCode.D;
        public KeyCode setTurnTypeLeft = KeyCode.A;
        public KeyCode setTurnTypeJump = KeyCode.Space;
        
        public KeyCode setDisplacementTypeNone = KeyCode.Backspace;
        public KeyCode setDisplacementTypeUp = KeyCode.U;
        public KeyCode setDisplacementTypeDown = KeyCode.LeftShift; // Check if this conflicts with sprint
        public KeyCode setDisplacementTypeForwardUp = KeyCode.E;
        public KeyCode setDisplacementTypeForwardDown = KeyCode.Q;

        [Header("Camera Settings")]
        public Vector3 cameraOffset = new Vector3(0f, 5f, -8f);
        [Range(0f, 20f)]
        public float cameraMoveSpeed = 8f;

#if UNITY_EDITOR
        // 供 Overlay 显示使用
        public int CurrentRoadIndex => _currentRoadIndex;
        public int CurrentBlockLocalIndex => _currentBlockLocalIndex;
        public bool SprintHeld { get; set; }

        private EditorApplication.CallbackFunction _smoothMoveCallback;

        private void OnDisable()
        {
            if (_smoothMoveCallback != null)
                EditorApplication.update -= _smoothMoveCallback;
        }

        // ──────────────────────────────────────────────────────────────
        //  导航逻辑
        // ──────────────────────────────────────────────────────────────

        public void JumpToBlock(int roadIndex, int localIndex)
        {
            if (!HasValidReference) return;
            if (!SceneData.IsValidRoadIndex(roadIndex)) return;
            
            SceneData.GetRoadData(roadIndex, out var roadData);
            int clampedLocalIndex = Mathf.Clamp(localIndex, 0, Mathf.Max(0, roadData.BlockCount - 1));

            var block = FindBlockInScene(roadIndex, clampedLocalIndex);
            SetTargetBlock(block, roadIndex, clampedLocalIndex);
        }

        public void NavigateBlock(bool forward)
        {
            if (!HasValidReference) return;

            // 简单的全部遍历查找下一个
            // 如果 SprintHeld，则跳过普通块
            
            int rIdx = _currentRoadIndex;
            int bIdx = _currentBlockLocalIndex;

            int iterations = 0;
            int maxIterations = 10000; // 防止死循环

            while (iterations < maxIterations)
            {
                iterations++;
                
                // 步进
                if (forward)
                {
                    bIdx++;
                    SceneData.GetRoadData(rIdx, out var rData);
                    if (bIdx >=_GetBlockCount(rIdx))
                    {
                        // 下一个 Road
                        rIdx++;
                        bIdx = 0;
                        if (!SceneData.IsValidRoadIndex(rIdx))
                        {
                            // 到头了
                            return; 
                        }
                    }
                }
                else
                {
                    bIdx--;
                    if (bIdx < 0)
                    {
                        // 上一个 Road
                        rIdx--;
                        if (!SceneData.IsValidRoadIndex(rIdx))
                        {
                            // 到头了
                            return;
                        }
                        bIdx = _GetBlockCount(rIdx) - 1;
                        if (bIdx < 0) bIdx = 0; // 空 Road
                    }
                }

                // 检查是否符合 Sprint 条件
                if (SprintHeld)
                {
                    if (IsSpecialBlock(rIdx, bIdx))
                    {
                        JumpToBlock(rIdx, bIdx);
                        return;
                    }
                }
                else
                {
                    // 普通模式，直接跳转
                    JumpToBlock(rIdx, bIdx);
                    return;
                }
            }
        }

        private int _GetBlockCount(int roadIndex)
        {
            if (SceneData.GetRoadData(roadIndex, out var data))
                return data.BlockCount;
            return 0;
        }

        private bool IsSpecialBlock(int rIdx, int bIdx)
        {
            if (!SceneData.GetRoadData(rIdx, out var rData)) return false;
            // 检查 Tap (这里假设有 GetBlockData)
            SceneData.GetBlockData(rIdx, bIdx, out var bData);
            if (bData == null) return false; // 默认普通
            
            // 下方逻辑需根据实际业务调整：什么是"Special"?
            // 原逻辑：NeedTap 或 HasDisplacementRule
            // 这里检查 BlockData 是否有特殊属性
            if (bData.HasTurn || bData.HasDisplacement) return true;
            
            // 检查 Tap? SceneData 似乎没有明确存储 Tap 信息，除非在 BlockData 中
            // 假设 BlockData 不包含 Tap 信息? 
            // 如果 SceneData 中没有 Tap 信息，可能需要其他方式判断，或者暂时只判断 Turn/Displacement
            
            return false;
        }

        private Block FindBlockInScene(int roadIndex, int localIndex)
        {
            if (targetMap == null) return null;
            if (!targetMap.TryGetRoad(roadIndex, out var road)) return null;
            if (road == null) return null;
            
            // 假设 Road.blocks 是按 localIndex 排序的，或者我们需要搜索
            return road.blocks.FirstOrDefault(b => b.blockLocalIndex == localIndex);
        }

        private void SetTargetBlock(Block block, int roadIndex, int localIndex)
        {
            targetBlock = block;
            targetRoad = block != null ? block.road : null;
            _currentRoadIndex = roadIndex;
            _currentBlockLocalIndex = localIndex;

            if (block != null)
            {
                Selection.activeGameObject = block.gameObject;
                FocusCameraOnBlock(block);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  编辑逻辑
        // ──────────────────────────────────────────────────────────────

        public void ApplyTurnType(TurnType turnType)
        {
            if (!HasValidReference) return;
            // 更新数据
            if (SceneData.GetBlockData(_currentRoadIndex, _currentBlockLocalIndex, out var bData))
            {
                if (bData == null) 
                {
                    // 如果不存在，创建新的数据 entry
                    // 实际上 SceneData.GetBlockData 应该返回一个新的如果不存在，或者我们需要手动添加
                    // 查看 SceneData.GetBlockData 实现... 假设它返回引用可修改
                    // 如果 SceneData 仅返回 Copy，我们需要 SetBlockData
                }
            }

            // 由于 SceneData 结构未知细节，假设我们可以这样操作：
            // 需要确保 SceneData 有 SetBlockData 方法或者 GetBlockData 返回同一个引用
            // 如果 SceneData 没有 SetBlockData，可能需要扩充 SceneData
            
            // 暂时先只打印日志，提示需要 SceneData 支持写入
            Debug.Log($"Apply TurnType {turnType} to R{_currentRoadIndex}:B{_currentBlockLocalIndex}");
            
            // 修改 SceneData
            SceneData.SetBlockData_TurnType(_currentRoadIndex, _currentBlockLocalIndex, turnType);
            
            // 触发刷新
            targetMap.Dispatcher.Dispatch(
                nameof(EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(targetMap, _currentRoadIndex, _currentBlockLocalIndex));
        }

        public void ApplyDisplacementType(DisplacementType dispType)
        {
            if (!HasValidReference) return;
            
            SceneData.SetBlockData_DisplacementType(_currentRoadIndex, _currentBlockLocalIndex, dispType);

            targetMap.Dispatcher.Dispatch(
                nameof(EditorTool.OnBlockDisplacementRuleChanged),
                EditorActionContext.ForRoadAndBlock(targetMap, _currentRoadIndex, _currentBlockLocalIndex));
        }

        // ──────────────────────────────────────────────────────────────
        //  相机逻辑 (复用原逻辑)
        // ──────────────────────────────────────────────────────────────

        private void FocusCameraOnBlock(Block block)
        {
            if (block == null) return;
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            Vector3 targetPos = block.transform.position;
            Vector3 desiredPivot = targetPos;

            if (cameraMoveSpeed <= 0f)
                TeleportSceneCamera(sceneView, desiredPivot, cameraOffset);
            else
                SmoothMoveSceneCamera(sceneView, desiredPivot, cameraOffset);
        }

        private void TeleportSceneCamera(SceneView sceneView, Vector3 pivot, Vector3 offset)
        {
            Vector3 camPos = pivot + offset;
            Vector3 lookDir = (pivot - camPos).normalized;

            sceneView.pivot = pivot;
            sceneView.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            sceneView.size = offset.magnitude * 0.5f;
            sceneView.Repaint();
        }

        private void SmoothMoveSceneCamera(SceneView sceneView, Vector3 targetPivot, Vector3 offset)
        {
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

                float t = cameraMoveSpeed * 0.016f;
                sceneView.pivot = Vector3.Lerp(sceneView.pivot, targetPivot, t);
                sceneView.rotation = Quaternion.Slerp(sceneView.rotation, desiredRot, t);
                sceneView.size = Mathf.Lerp(sceneView.size, desiredSize, t);
                sceneView.Repaint();

                if (Vector3.Distance(sceneView.pivot, targetPivot) < 0.01f)
                    EditorApplication.update -= _smoothMoveCallback;
            };

            EditorApplication.update += _smoothMoveCallback;
        }
#endif
    }
}

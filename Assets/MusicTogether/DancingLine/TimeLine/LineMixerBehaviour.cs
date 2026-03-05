using System.Collections.Generic;
using MusicTogether.DancingLine.Interfaces;
using UnityEngine;
using UnityEngine.Playables;

namespace MusicTogether.DancingLine.TimeLine
{
    public class LineMixerBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// 缓存的 ILineComponent 引用，在 Track 创建时注入
        /// 避免每帧都进行 GetComponent 查找
        /// </summary>
        public ILineComponent cachedLineComponent;
        
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // 优先使用缓存的引用
            var lineComponent = cachedLineComponent;
            
            // 如果缓存为空，尝试从 playerData 获取（兼容性处理）
            if (lineComponent == null)
            {
                var gameObject = playerData as GameObject;
                if (gameObject != null)
                {
                    lineComponent = gameObject.GetComponent<ILineComponent>();
                    // 找到后缓存，避免下次再查找
                    cachedLineComponent = lineComponent;
                }
            }
            
            if (lineComponent == null) return;
        
            int inputCount = playable.GetInputCount();
            if (inputCount == 0) return;
            
            float totalWeight = 0f;
            var states = new List<(MotionState state, float weight, AnimationCurve curve)>();
            
            // 收集所有活动的 Pool 的 MotionState
            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight > 0.0001f)
                {
                    var inputPlayable = (ScriptPlayable<LineBehaviour>)playable.GetInput(i);
                    var inputBehaviour = inputPlayable.GetBehaviour();

                    if (inputBehaviour?.linePool != null && inputBehaviour.CurrentMotionState != null)
                    {
                        float curvedWeight = inputBehaviour.blendCurve?.Evaluate(inputWeight) ?? inputWeight;
                        states.Add((inputBehaviour.CurrentMotionState, curvedWeight, inputBehaviour.blendCurve));
                        totalWeight += curvedWeight;
                    }
                }
            }

            // 如果有活动的状态，进行混合并更新 LineComponent
            if (states.Count > 0 && totalWeight > 0.0001f)
            {
                MotionState finalMotionState = BlendMotionStates(states, totalWeight);
                lineComponent.UpdatePosition(finalMotionState);
            }
        }
        
        /// <summary>
        /// 混合多个 MotionState
        /// 注意：这里在 WorldSpace 进行混合，然后转换回 ParentSpace
        /// </summary>
        private MotionState BlendMotionStates(List<(MotionState state, float weight, AnimationCurve curve)> states, float totalWeight)
        {
            // 只有一个状态时直接返回
            if (states.Count == 1) return states[0].state;
            
            Vector3 blendedWorldPosition = Vector3.zero;
            Quaternion blendedWorldRotation = Quaternion.identity;
            Transform refTransform = states[0].state.SelfTransform;

            // 在世界空间中进行混合（更符合直觉）
            foreach (var (state, weight, _) in states)
            {
                float normalizedWeight = weight / totalWeight;
                blendedWorldPosition += state.WorldSpacePosition * normalizedWeight;
            }
            
            // 旋转混合：使用加权四元数混合（更精确）
            blendedWorldRotation = BlendQuaternions(states, totalWeight);

            // 创建新的 MotionState 并转换回 ParentSpace
            var result = new MotionState { SelfTransform = refTransform };
            result.ParentSpacePosition = result.WorldPosToParent(blendedWorldPosition);
            result.ParentSpaceRotation = result.WorldRotToParent(blendedWorldRotation);
            
            return result;
        }
        
        /// <summary>
        /// 加权混合四元数
        /// 使用累积归一化方法，比连续 Slerp 更精确
        /// </summary>
        private Quaternion BlendQuaternions(
            List<(MotionState state, float weight, AnimationCurve curve)> states,
            float totalWeight)
        {
            if (states.Count == 1)
                return states[0].state.WorldSpaceRotation;
            
            // 累积四元数向量
            Vector4 cumulative = Vector4.zero;
            
            foreach (var (state, weight, _) in states)
            {
                Quaternion q = state.WorldSpaceRotation;
                float normalizedWeight = weight / totalWeight;
                
                Vector4 qVec = new Vector4(q.x, q.y, q.z, q.w);
                
                // 确保所有四元数在同一半球（避免走远路）
                if (Vector4.Dot(qVec, cumulative) < 0)
                {
                    qVec = -qVec;
                }
                
                cumulative += qVec * normalizedWeight;
            }
            
            // 归一化得到最终四元数
            cumulative.Normalize();
            return new Quaternion(cumulative.x, cumulative.y, cumulative.z, cumulative.w);
        }
    }
}
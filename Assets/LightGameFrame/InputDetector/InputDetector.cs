//using MaxIceFlameTemplate.Basic;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace LightGameFrame.InputDetector
{
    public enum DetectionType { OnEnter, OnHeld, OnExit }

    [Serializable] [InlineProperty] [LabelWidth(80)] [LabelText("特定按键检测")]
    public class SpecificKeyDetection : InputCondition
    {
        public enum ConditionType { [LabelText("必须有")]Required, [LabelText("不能有")]Excluded }

    
        [HorizontalGroup] [LabelWidth(90)] public KeyCode targetKey;
        [HorizontalGroup(90)][HideLabel] public ConditionType conditionType;
        [HorizontalGroup(90)][HideLabel] public DetectionType detectionType;
    
        public bool? Meeted { get; set; }
    
        public void MeetCondition()
        {
            bool currentKeyDetected = false;
            switch (detectionType)
            {
                case DetectionType.OnEnter:
                    currentKeyDetected = Input.GetKeyDown(targetKey);
                    break;
                case DetectionType.OnHeld:
                    currentKeyDetected = Input.GetKey(targetKey);
                    break;
                case DetectionType.OnExit:
                    currentKeyDetected = Input.GetKeyUp(targetKey);
                    break;
                default:
                    currentKeyDetected = false;
                    break;
            }

            switch (conditionType)
            {
                case ConditionType.Required:
                    if (Meeted is true) return;
                    if (currentKeyDetected) Meeted = true;
                    break;
                case ConditionType.Excluded:
                    if (Meeted is false) return;
                    if (currentKeyDetected) Meeted = false;
                    else Meeted = true;
                    break;
            }
        }
    }

    [Serializable] [InlineProperty] [LabelWidth(80)] [LabelText("按键数量检测")]
    public class PhysicalKeyCountDetection : InputCondition
    {
        private HashSet<KeyCode> PhysicalKeyboardKeys = new HashSet<KeyCode>()
        {
            KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.K, KeyCode.L, KeyCode.M,
            KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
            KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
            KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
            KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
            KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.Tab, KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt,
            KeyCode.Backspace,KeyCode.Delete, KeyCode.Insert, KeyCode.Home, KeyCode.End, KeyCode.PageUp, KeyCode.PageDown
        };
        [HorizontalGroup] [LabelWidth(90)] public int neededCount;
        [HorizontalGroup(90)][HideLabel] public DetectionType detectionType;
        public bool? Meeted { get; set; }

        public void MeetCondition()
        {
            if (Meeted is true) return;
            int currentCount = detectionType switch
            {
                DetectionType.OnEnter => PhysicalKeyboardKeys.Count(Input.GetKeyDown),
                DetectionType.OnHeld  => PhysicalKeyboardKeys.Count(Input.GetKey),
                DetectionType.OnExit  => PhysicalKeyboardKeys.Count(Input.GetKeyUp),
                _ => 0
            };
            if (currentCount >= neededCount) Meeted = true;
        }
    }

    public interface InputCondition
    {
        public bool? Meeted { get; set; }
        /// <summary>
        /// 检测当前条件是否已完成。
        /// </summary>
        /// <returns>返回true说明检测成功，返回null说明未检出，返回false说明检测失败</returns>
        public void MeetCondition();
    }

    /// <summary>
    /// 按键检测，在一定的时间范围内检测是否满足所有条件，若满足则不再继续。
    /// </summary>
    public class InputDetector : MonoBehaviour
    {
        private enum ConditionOption { [LabelText("特定按键检测")] SpecificKey, [LabelText("按键数量检测")] PhysicalKeyCount }
    
    
        [PropertyOrder(0)] [LabelText("检测时长")]
        public float duration;
    
        [PropertyOrder(10)]
        [LabelText("检测条件列表")]
        [SerializeReference]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true, ShowIndexLabels = true)]
        public List<InputCondition> inputConditions = new List<InputCondition>();
    
        [PropertyOrder(20)] public UnityEvent OnInputDetected;

        private bool isDetecting = false;
    
        protected void BeginDetection()
        {
            inputConditions.ForEach(c => c.Meeted = null);
            StartCoroutine(Detection());
        }
    
        IEnumerator Detection()
        {
            isDetecting = true;
            float endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                foreach (var condition in inputConditions)
                {
                    condition.MeetCondition();
                }

                if (inputConditions.Exists(c => c.Meeted.HasValue && c.Meeted.Value == false))
                {
                    //存在检查失败的项目，停止检查
                    isDetecting = false;
                    yield break;
                }
                else if (inputConditions.Exists(c => !c.Meeted.HasValue))
                {
                    //存在还没有完成检查的项目，继续检查
                    yield return null;
                }
                else
                {
                    //所有的项目都满足了
                    OnInputDetected?.Invoke();
                    isDetecting = false;
                    yield break;
                }
            }

            isDetecting = false;
        }

        bool Detect()
        {
            bool hasUnmetCondition = false;
            foreach (var condition in inputConditions)
            {
                if (condition.Meeted.HasValue && condition.Meeted.Value) continue;
                condition.MeetCondition();
            }
            return !hasUnmetCondition;
        }



    }
}
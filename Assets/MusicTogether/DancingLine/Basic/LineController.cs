using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class LineController : BaseLineController
    {
        public override void DetectInput()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space)) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            OnInputDetected?.Invoke();
            /*CurrentDirectionIndex = Directions[CurrentDirectionIndex].NextDirectionID;
            OnDirectionChanged?.Invoke(Directions[CurrentDirectionIndex]);*/
        }
    }
}
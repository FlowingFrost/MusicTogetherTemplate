using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class LineController : BaseLineController
    {
        public override void DetectInput()
        {

            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space)) return;
            CurrentDirectionIndex = Directions[CurrentDirectionIndex].NextDirectionID;
            OnDirectionChanged?.Invoke(Directions[CurrentDirectionIndex]);
        }
    }
}
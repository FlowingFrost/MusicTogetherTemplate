using System;
using System.Collections.Generic;
using MusicTogether.DancingLine.Core;
using UnityEngine;

namespace MusicTogether.DancingLine.Basic
{
    [Serializable]
    public class Direction : BaseDirection
    {
        public Vector3 DirectionVector;
    }
    
    [Serializable]
    public class LineController : BaseLineController
    {
        /*public BasicLineController(List<BasicDirection> directions,Action<BasicDirection> callback) : base(callback)
        {
            Directions = directions;
        }*/

        public override void DetectInput()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space)) return;
            CurrentDirectionIndex = Directions[CurrentDirectionIndex].NextDirectionID;
            OnDirectionChanged?.Invoke(Directions[CurrentDirectionIndex]);
        }
    }
}
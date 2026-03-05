using System;
using UnityEngine.Events;

namespace MusicTogether.DancingLine.Interfaces
{
    /// <summary>
    /// 线条控制器接口
    /// 负责检测输入并触发方向改变
    /// </summary>
    public interface ILineController
    {
        //IDirection CurrentDirection { get; }
        //public event Action<int?> OnDirectionChanged;
        
        
        //public event Action OnInputDetected { get; }
        
        
        //public event Action<IDirection, bool, Vector3> OnGroundedChanged;
        //void Register(Action<IDirection> changeDirCallback, Action<bool, float> onGroundedChanged);
        /// <summary>检测输入（每帧调用）</summary>
        //void DetectInput(MotionType currentMotionType);
        //public void Register(Action onInputDetected);
    }
}
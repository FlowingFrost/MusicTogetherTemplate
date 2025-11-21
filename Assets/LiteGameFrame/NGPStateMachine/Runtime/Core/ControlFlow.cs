using System;

namespace LiteGameFrame.NGPStateMachine
{
    /// <summary>
    /// 控制流信号类型标识
    /// 用于在 Node Graph Processor 中标识控制流端口
    /// 这是一个空结构体，仅用于类型系统和端口连接验证
    /// </summary>
    [Serializable]
    public struct ControlFlow
    {
        // 空结构体，仅用于标识控制流连接
        // NGP 会根据类型来着色和验证端口连接
    }
}

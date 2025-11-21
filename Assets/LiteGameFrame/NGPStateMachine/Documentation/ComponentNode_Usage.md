# Component Node 使用指南

## 概述

Component Node 允许你在状态机中引用和操作场景中的组件（如 Transform、Animator、GameObject 等）。

## 核心特性

- **场景对象绑定**：类似 Timeline 的机制，Component 引用存储在 NGPStateMachine 组件中
- **类型安全**：通过泛型确保绑定的类型正确
- **易于扩展**：继承 `ComponentNode<T>` 即可创建自定义节点

## 工作流程

### 1. 打开图编辑器

- 菜单：`Window > State Machine Graph Editor`
- 窗口会保持打开状态

### 2. 选择要编辑的物体

- 在 Hierarchy 中选中带有 `NGPStateMachine` 组件的 GameObject
- 确保该组件的 `StateGraph` 字段已赋值
- 图编辑器会自动加载该物体的状态机图

### 3. 添加 Component Node

在图编辑器中右键：
- `State Machine > Component > Transform Move` - 移动物体
- `State Machine > Component > Animator Trigger` - 控制动画
- `State Machine > Component > GameObject SetActive` - 激活/禁用物体

### 4. 绑定场景对象

- 选中 Component Node
- 在节点上会显示 `Bind [ComponentType]` 字段
- 从 Hierarchy 拖拽对应的物体或组件到该字段
- 绑定信息会自动保存到 NGPStateMachine 组件

### 5. 运行测试

- 进入 Play 模式
- 状态机会自动使用绑定的组件

## 内置 Component Node

### TransformMoveNode

**功能**：移动物体到指定位置

**参数**：
- `Target Position` (Vector3): 目标位置
- `Move Mode` (Absolute/Relative): 绝对位置或相对偏移
- `Duration` (float): 移动时长（秒）
- `Use Local Space` (bool): 是否使用本地坐标

**示例**：
```
Entry → TransformMove(pos=(0,5,0), duration=2) → Log("Move completed")
```

### AnimatorTriggerNode

**功能**：控制 Animator 参数

**参数**：
- `Parameter Name` (string): 参数名称
- `Parameter Type` (Trigger/Bool/Int/Float): 参数类型
- 对应类型的值输入

**示例**：
```
Entry → AnimatorTrigger(paramName="Jump", type=Trigger) → Delay(1) → Log("Jump animation triggered")
```

### GameObjectSetActiveNode

**功能**：激活或禁用 GameObject

**参数**：
- `Active` (bool): 是否激活

**示例**：
```
Entry → Delay(2) → GameObjectSetActive(active=false) → Log("Object hidden")
```

## 创建自定义 Component Node

### 示例：创建 Audio Source 播放节点

```csharp
using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    [Serializable, NodeMenuItem("State Machine/Component/Audio Play")]
    public class AudioPlayNode : ComponentNode<AudioSource>
    {
        [Input("Audio Clip")]
        [Tooltip("要播放的音频")]
        public AudioClip clip;
        
        [Tooltip("是否循环播放")]
        public bool loop = false;
        
        public override string name => "Audio Play";
        public override Color color => new Color(0.2f, 0.8f, 0.8f);
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            // 尝试获取绑定的 AudioSource
            if (!TryGetBoundComponent(out var audioSource))
                return;
            
            // 设置音频
            if (clip != null)
            {
                audioSource.clip = clip;
            }
            
            audioSource.loop = loop;
            audioSource.Play();
            
            Debug.Log($"[AudioPlayNode] Playing audio on {audioSource.gameObject.name}");
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            // 可以在这里停止播放
            if (TryGetBoundComponent(out var audioSource))
            {
                audioSource.Stop();
            }
            StopRunning();
        }
    }
}
```

## 多个绑定字段

如果一个节点需要绑定多个 Component，可以重写 `BindingFieldName`：

```csharp
public class MultiBindingNode : BaseStateNode
{
    protected Transform GetSourceTransform()
    {
        if (_stateMachine is NGPStateMachine sm)
            return sm.GetComponentBinding<Transform>(GUID, "source");
        return null;
    }
    
    protected Transform GetTargetTransform()
    {
        if (_stateMachine is NGPStateMachine sm)
            return sm.GetComponentBinding<Transform>(GUID, "target");
        return null;
    }
}
```

## 注意事项

1. **场景引用限制**：Component Binding 只能引用同一场景中的对象
2. **运行时修改**：运行时修改绑定不会生效，需要在编辑模式下绑定
3. **Prefab 支持**：绑定数据存储在场景中的 NGPStateMachine 实例上，不会影响 Prefab
4. **序列化**：Unity 会自动处理场景引用的序列化，不需要手动处理

## 与 Timeline 的对比

| 特性 | Timeline | NGPStateMachine |
|------|----------|-----------------|
| 绑定存储位置 | PlayableDirector | NGPStateMachine |
| 编辑器打开方式 | 选中物体自动加载 | 选中物体自动加载 |
| 资源文件 | TimelineAsset | StateMachineGraph |
| 绑定机制 | ExposedReference | ComponentBinding |
| 运行时访问 | Resolve() | GetComponentBinding() |

## 故障排除

**问题**：节点显示"Component binding is missing"错误

**解决**：
1. 确保在图编辑器中选中了正确的 GameObject
2. 检查节点的绑定字段是否已赋值
3. 检查绑定的对象是否在同一场景中

**问题**：修改绑定后不生效

**解决**：
1. 保存场景（Ctrl+S）
2. 重新进入 Play 模式
3. 检查 Console 是否有错误信息

**问题**：图编辑器没有自动加载

**解决**：
1. 确保选中的 GameObject 有 NGPStateMachine 组件
2. 确保 StateGraph 字段已赋值
3. 重新打开图编辑器窗口


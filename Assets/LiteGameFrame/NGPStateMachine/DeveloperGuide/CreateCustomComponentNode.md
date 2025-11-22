# 创建自定义 ComponentNode

> 扩展 ComponentNode 实现操作场景组件的节点

---

## 概述

`ComponentNode<T>` 是一个泛型基类，用于创建操作场景组件的节点。当你需要：

- 直接控制场景中的组件（Transform、Animator、Rigidbody 等）
- 封装可复用的组件操作逻辑
- 提供给非程序员使用的预制功能

可以创建自定义 ComponentNode。

---

## 基础示例

### 1. 创建简单的 ComponentNode

```csharp
using System;
using UnityEngine;
using GraphProcessor;
using LiteGameFrame.NGPStateMachine;

[Serializable]
[NodeMenuItem("My Nodes/Apply Force")] // 在右键菜单中的位置
public class ApplyForceNode : ComponentNode<Rigidbody>
{
    [Input("Force")]
    public Vector3 force = Vector3.up * 10f;
    
    public ForceMode forceMode = ForceMode.Impulse;
    
    public override void OnEnterSignal(string sourceId)
    {
        // 尝试获取绑定的 Rigidbody
        if (TryGetBoundComponent(out var rb))
        {
            rb.AddForce(force, forceMode);
            Debug.Log($"对 {rb.name} 施加力: {force}");
        }
        else
        {
            Debug.LogWarning("未绑定 Rigidbody！");
        }
        
        // 立即触发信号并停止
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        // 收到强制停止信号
        StopRunning();
    }
}
```

### 2. 在图中使用

1. 打开状态机图编辑器
2. 右键选择 `My Nodes > Apply Force`
3. 在节点上找到 `🔗 Rigidbody` 字段
4. 从 Hierarchy 拖拽带有 Rigidbody 的物体到该字段
5. 配置 `force` 和 `forceMode` 参数
6. 连接控制流并运行

---

## 进阶示例

### 持续运行的节点

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Rotate Object")]
public class RotateObjectNode : ComponentNode<Transform>
{
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);
    public Space space = Space.World;
    
    private bool isRunning = false;
    
    public override void OnEnterSignal(string sourceId)
    {
        isRunning = true;
        TriggerSignal(); // 立即触发信号，但继续运行
    }
    
    public override void OnUpdate()
    {
        if (!isRunning) return;
        
        if (TryGetBoundComponent(out var target))
        {
            target.Rotate(rotationSpeed * Time.deltaTime, space);
        }
    }
    
    public override void OnExitSignal(string sourceId)
    {
        isRunning = false;
        StopRunning();
    }
}
```

### 带输入端口的节点

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Set Position")]
public class SetPositionNode : ComponentNode<Transform>
{
    [Input("Target Position")]
    public Vector3 targetPosition;
    
    [Input("Is Local")]
    public bool isLocal = false;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetBoundComponent(out var target))
        {
            if (isLocal)
                target.localPosition = targetPosition;
            else
                target.position = targetPosition;
                
            Debug.Log($"设置 {target.name} 位置为 {targetPosition}");
        }
        
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

### 带输出端口的节点

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Get Transform Info")]
public class GetTransformInfoNode : ComponentNode<Transform>
{
    [Output("Position")]
    public Vector3 outputPosition;
    
    [Output("Rotation")]
    public Quaternion outputRotation;
    
    [Output("Scale")]
    public Vector3 outputScale;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetBoundComponent(out var target))
        {
            outputPosition = target.position;
            outputRotation = target.rotation;
            outputScale = target.localScale;
            
            // 写入黑板（可选）
            StateMachine.Set("transform_position", outputPosition);
            StateMachine.Set("transform_rotation", outputRotation);
        }
        
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

---

## ComponentNode API

### 核心方法

```csharp
// 尝试获取绑定的组件
protected bool TryGetBoundComponent(out T component)

// 触发输出信号
protected void TriggerSignal()

// 停止节点运行
protected void StopRunning()

// 访问状态机
protected IStateMachine StateMachine
```

### 生命周期方法

```csharp
// 接收 OnEnter 信号时调用（必须实现）
public abstract void OnEnterSignal(string sourceId);

// 接收 OnExit 信号时调用（必须实现）
public abstract void OnExitSignal(string sourceId);

// 每帧调用（仅当节点活跃时）
public virtual void OnUpdate() { }

// 清理资源
public virtual void Cleanup() { }
```

---

## 内置 ComponentNode 参考

查看源码了解更多实现细节：

- **TransformMoveNode** (`Runtime/Nodes/ComponentNodes.cs`)
  - 移动物体到目标位置
  - 支持 Lerp 插值和移动时间
  
- **AnimatorTriggerNode** (`Runtime/Nodes/ComponentNodes.cs`)
  - 控制 Animator 参数
  - 支持 Trigger、Bool、Int、Float
  
- **GameObjectSetActiveNode** (`Runtime/Nodes/ComponentNodes.cs`)
  - 激活/禁用 GameObject
  - 支持延迟执行

---

## 高级技巧

### 1. 使用 ShowInInspector 属性

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Complex Node")]
public class ComplexNode : ComponentNode<Animator>
{
    [ShowInInspector] // 在 Inspector 中显示
    public AnimationClip clip;
    
    [ShowInInspector]
    [Range(0f, 2f)]
    public float playSpeed = 1f;
    
    // ... 实现
}
```

### 2. 条件判断

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Check Distance")]
public class CheckDistanceNode : ComponentNode<Transform>
{
    [Input("Target")]
    public Transform target;
    
    public float threshold = 5f;
    
    [Output("Is Close")]
    public bool isClose;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetBoundComponent(out var source) && target != null)
        {
            float distance = Vector3.Distance(source.position, target.position);
            isClose = distance < threshold;
            
            StateMachine.Set("is_target_close", isClose);
        }
        
        TriggerSignal();
        StopRunning();
    }
}
```

### 3. 协程支持

```csharp
[Serializable]
[NodeMenuItem("My Nodes/Fade Audio")]
public class FadeAudioNode : ComponentNode<AudioSource>
{
    public float duration = 1f;
    public float targetVolume = 0f;
    
    private Coroutine fadeCoroutine;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetBoundComponent(out var audioSource))
        {
            // 需要通过 CoroutineRunner 运行协程
            // fadeCoroutine = CoroutineRunner.Instance.StartCoroutine(FadeCoroutine(audioSource));
        }
    }
    
    public override void OnExitSignal(string sourceId)
    {
        if (fadeCoroutine != null)
        {
            // CoroutineRunner.Instance.StopCoroutine(fadeCoroutine);
        }
        StopRunning();
    }
}
```

---

## 最佳实践

### ✅ 推荐做法

1. **使用泛型约束**
   - 明确指定需要的组件类型
   - 利用类型安全

2. **提供清晰的参数**
   - 使用 `[Tooltip]` 说明参数用途
   - 合理的默认值

3. **处理空引用**
   - 始终使用 `TryGetBoundComponent` 检查
   - 提供有用的错误信息

4. **及时清理**
   - 在 `OnExitSignal` 中停止协程、取消订阅等

### ❌ 避免做法

1. **不要直接访问组件**
   - 错误：`component.transform.position = ...`
   - 正确：通过 `TryGetBoundComponent` 获取

2. **不要在构造函数中初始化**
   - 节点通过反射创建，使用字段初始化或 `OnEnterSignal`

3. **不要缓存组件引用**
   - 场景可能重新加载，每次都通过 `TryGetBoundComponent` 获取

---

## 调试技巧

### 1. 日志输出

```csharp
public override void OnEnterSignal(string sourceId)
{
    Debug.Log($"[{GetType().Name}] OnEnter from {sourceId}");
    
    if (TryGetBoundComponent(out var component))
    {
        Debug.Log($"成功获取组件: {component.name}");
    }
    else
    {
        Debug.LogError($"未找到绑定的 {typeof(T).Name}");
    }
}
```

### 2. 使用 Gizmos 可视化

```csharp
// 在自定义 NodeView 中实现
public class MyNodeView : BaseStateNodeView
{
    protected override void DrawDefaultInspector()
    {
        base.DrawDefaultInspector();
        // 自定义 Inspector 显示
    }
}
```

---

## 相关文档

- [ComponentNode 使用指南](../Documentation/ComponentNode_Usage.md)
- [ComponentBinding 系统详解](API_ComponentBinding.md)
- [扩展编辑器视图](ExtendEditorView.md)

---

**提示**：查看 `Runtime/Nodes/ComponentNodes.cs` 获取更多内置节点的实现参考。

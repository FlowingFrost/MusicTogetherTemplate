# 创建自定义 ScriptStateNode

> 扩展 ScriptStateNode 实现自定义的状态逻辑节点

---

## 概述

`ScriptStateNode` 是一个代理节点，它将状态机的生命周期转发给场景中的脚本。当你需要：

- 在场景脚本中实现状态逻辑
- 快速迭代和调试状态行为
- 让非程序员也能配置状态机流程

可以使用 ScriptStateNode。

---

## 创建步骤

### 1. 实现 IStateHandler 接口

在场景中创建 MonoBehaviour 脚本，实现 `IStateHandler` 接口：

```csharp
using UnityEngine;
using LiteGameFrame.NGPStateMachine;

public class MyCustomState : MonoBehaviour, IStateHandler
{
    // 状态进入时调用
    public void OnStateEnter(StateContext context)
    {
        Debug.Log("状态进入");
    }
    
    // 每帧调用（仅当节点活跃时）
    public void OnStateUpdate(StateContext context)
    {
        // 状态逻辑
    }
    
    // 状态退出时调用
    public void OnStateExit(StateContext context)
    {
        Debug.Log("状态退出");
    }
}
```

### 2. 使用 StateContext

`StateContext` 提供了状态控制和数据访问的 API：

```csharp
public void OnStateUpdate(StateContext context)
{
    // 1. 请求完成状态（触发 Signal 输出）
    if (/* 完成条件 */)
    {
        context.RequestComplete();
    }
    
    // 2. 请求停止节点（从活跃列表移除）
    if (/* 停止条件 */)
    {
        context.RequestStop();
    }
    
    // 3. 访问黑板数据
    if (context.GetBlackboardValue<int>("health", out int health))
    {
        Debug.Log($"当前生命值: {health}");
    }
    
    context.SetBlackboardValue("score", 100);
    
    // 4. 获取绑定的组件（如果有）
    if (context.TryGetBinding<Animator>("animator", out var anim))
    {
        anim.SetTrigger("Attack");
    }
}
```

### 3. 在图中使用

1. 打开状态机图编辑器
2. 右键选择 `State Machine > Script State Node`
3. 将场景中带有 `IStateHandler` 脚本的物体拖到节点的 `🔗 Script` 字段
4. 连接控制流
5. 保存并运行

---

## 高级用法

### 绑定多个组件

ScriptStateNode 支持绑定多个组件，在状态逻辑中使用：

```csharp
public class ComplexState : MonoBehaviour, IStateHandler
{
    public void OnStateEnter(StateContext context)
    {
        // 获取多个绑定的组件
        var animator = context.GetBinding<Animator>("animator");
        var rigidbody = context.GetBinding<Rigidbody>("rb");
        var audioSource = context.GetBinding<AudioSource>("audio");
        
        animator?.SetTrigger("Jump");
        rigidbody?.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        audioSource?.Play();
    }
}
```

在编辑器中，节点会显示所有需要的绑定字段，你可以拖拽场景对象到这些字段。

### 条件判断与分支

```csharp
public class HealthCheckState : MonoBehaviour, IStateHandler
{
    public void OnStateEnter(StateContext context)
    {
        // 读取黑板数据
        if (context.GetBlackboardValue<int>("health", out int health))
        {
            if (health <= 0)
            {
                // 触发死亡状态
                context.SetBlackboardValue("isDead", true);
            }
            else if (health < 30)
            {
                // 触发低血量状态
                context.SetBlackboardValue("isLowHealth", true);
            }
        }
        
        // 完成检查
        context.RequestComplete();
        context.RequestStop();
    }
}
```

### 持续运行的状态

```csharp
public class PatrolState : MonoBehaviour, IStateHandler
{
    private Transform[] waypoints;
    private int currentIndex = 0;
    
    public void OnStateEnter(StateContext context)
    {
        waypoints = GetWaypoints();
        currentIndex = 0;
        
        // 立即触发信号，但不停止运行
        context.RequestComplete();
    }
    
    public void OnStateUpdate(StateContext context)
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        // 持续巡逻逻辑
        MoveTowardsWaypoint();
        
        if (ReachedWaypoint())
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }
    
    public void OnStateExit(StateContext context)
    {
        // 清理
        StopMoving();
    }
}
```

---

## 最佳实践

### ✅ 推荐做法

1. **状态逻辑单一职责**
   - 每个 IStateHandler 只做一件事
   - 复杂逻辑拆分为多个状态

2. **使用黑板共享数据**
   - 不要在状态之间直接引用
   - 通过黑板传递数据

3. **在 OnStateExit 清理资源**
   - 停止协程、取消订阅事件等

4. **明确完成条件**
   - 调用 `RequestComplete()` 时机要明确
   - 避免忘记调用导致流程卡住

### ❌ 避免做法

1. **不要在状态中保存状态机引用**
   - 使用 `StateContext` 而不是直接访问状态机

2. **不要在多个状态间共享可变数据**
   - 容易导致状态污染和难以调试的 bug

3. **不要阻塞主线程**
   - 耗时操作使用协程或异步

---

## 示例项目

查看 `Examples/` 文件夹中的示例：

- `CheckHealthState.cs` - 条件判断示例
- `PlayerAttackState.cs` - 动画控制示例
- `SimpleMoveState.cs` - 移动控制示例

---

## 相关文档

- [ScriptStateNode 使用指南](../Documentation/ScriptStateNode_Usage.md)
- [IStateHandler API 参考](API_IStateHandler.md)
- [StateContext API 参考](API_StateContext.md)

---

**提示**：如果你的状态逻辑不需要访问场景对象，考虑使用继承 `BaseStateNode` 的方式创建纯逻辑节点，性能会更好。

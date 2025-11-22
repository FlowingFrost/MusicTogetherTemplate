# ScriptStateNode 使用指南

## 概述

`ScriptStateNode` 是一种新的节点类型，专门用于将状态机的生命周期转发给场景中的脚本。

### 设计理念

```
旧模式（ComponentNode）：
[节点] = 执行器 + 绑定对象
  └─ 逻辑写在节点类中（需要继承、编译）

新模式（ScriptStateNode）：
[节点] = 代理 + 多绑定支持
  └─ 逻辑写在场景脚本中（直接调试、灵活）
```

---

## 快速开始

### 1. 创建场景脚本

在场景中创建一个 MonoBehaviour 脚本，实现 `IStateHandler` 接口：

```csharp
using UnityEngine;
using LiteGameFrame.NGPStateMachine;

public class MyState : MonoBehaviour, IStateHandler
{
    public void OnStateEnter(StateContext context)
    {
        Debug.Log("状态进入");
        
        // 获取绑定的对象
        var animator = context.GetBinding<Animator>("animator");
        animator?.SetTrigger("Enter");
    }
    
    public void OnStateUpdate(StateContext context)
    {
        // 每帧执行的逻辑
        
        if (/* 完成条件 */)
        {
            context.RequestComplete(); // 触发下一状态
        }
    }
    
    public void OnStateExit(StateContext context)
    {
        Debug.Log("状态退出");
    }
}
```

### 2. 在 State Graph 中创建节点

1. 打开 State Graph 编辑器
2. 右键 → `State Machine/Script State`
3. 创建 `ScriptStateNode`

### 3. 绑定脚本

在 Inspector 中：
1. 选择场景中的 NGPStateMachine
2. 找到 `ScriptStateNode`
3. 绑定 Target Script（拖拽场景中的脚本）
4. 可选：配置 `additionalBindingFields`

---

## 核心接口

### IStateHandler

脚本必须实现的接口：

```csharp
public interface IStateHandler
{
    void OnStateEnter(StateContext context);  // 状态进入
    void OnStateUpdate(StateContext context); // 每帧更新
    void OnStateExit(StateContext context);   // 状态退出（强制）
}
```

### StateContext

传递给脚本的上下文对象：

```csharp
public class StateContext
{
    // 状态机引用（访问黑板数据）
    public NGPStateMachine StateMachine { get; }
    
    // 节点 ID
    public string NodeId { get; }
    
    // 源节点 ID（从哪个状态转换来）
    public string SourceId { get; }
    
    // 获取绑定对象
    public T GetBinding<T>(string fieldName) where T : UnityEngine.Object;
    
    // 带错误提示的获取
    public bool TryGetBinding<T>(string fieldName, out T binding);
    
    // 脚本主动完成（触发输出信号）
    public void RequestComplete();
    
    // 脚本主动停止（不触发输出）
    public void RequestStop();
}
```

---

## 多绑定使用

### 配置多个绑定字段

在 ScriptStateNode 的 Inspector 中：

```csharp
// ScriptStateNode 配置
targetScriptField = "targetScript"  // 目标脚本字段名
additionalBindingFields = [         // 额外绑定字段
    "animator",
    "weapon", 
    "audioSource"
]
```

### 在编辑器中绑定

节点视图会显示：
```
🎯 Target Script: [PlayerAttackState]
  🔗 animator:     [Player Animator]
  🔗 weapon:       [Sword GameObject]
  🔗 audioSource:  [Attack Sound]
```

### 在脚本中访问

```csharp
public void OnStateEnter(StateContext context)
{
    // 方式1：直接获取
    var animator = context.GetBinding<Animator>("animator");
    var weapon = context.GetBinding<GameObject>("weapon");
    var audio = context.GetBinding<AudioSource>("audioSource");
    
    // 方式2：带错误检查
    if (context.TryGetBinding<Animator>("animator", out var anim))
    {
        anim.SetTrigger("Attack");
    }
}
```

---

## 状态流程控制

### 主动完成状态

```csharp
public void OnStateUpdate(StateContext context)
{
    if (_attackCompleted)
    {
        // 触发节点的输出信号 → 转到下一状态
        context.RequestComplete();
    }
}
```

### 主动停止（不转换）

```csharp
public void OnStateEnter(StateContext context)
{
    if (!_conditionMet)
    {
        // 只停止节点，不触发输出
        context.RequestStop();
    }
}
```

---

## 黑板数据交互

```csharp
public void OnStateEnter(StateContext context)
{
    // 读取黑板数据
    if (context.StateMachine.Get<float>("health", out var health))
    {
        Debug.Log($"Health: {health}");
    }
    
    // 写入黑板数据
    context.StateMachine.Set("score", 100);
    
    // 查询类型
    if (context.StateMachine.Find<int>("score", out var type))
    {
        Debug.Log($"Score type: {type}");
    }
}
```

---

## 完整示例

### 示例1：攻击状态

```csharp
public class PlayerAttackState : MonoBehaviour, IStateHandler
{
    [SerializeField] private float attackDuration = 1.0f;
    private float _elapsedTime;
    
    public void OnStateEnter(StateContext context)
    {
        _elapsedTime = 0f;
        
        var animator = context.GetBinding<Animator>("animator");
        var weapon = context.GetBinding<GameObject>("weapon");
        
        animator?.SetTrigger("Attack");
        weapon?.SetActive(true);
    }
    
    public void OnStateUpdate(StateContext context)
    {
        _elapsedTime += Time.deltaTime;
        
        if (_elapsedTime >= attackDuration)
        {
            context.RequestComplete(); // 完成攻击，转下一状态
        }
    }
    
    public void OnStateExit(StateContext context)
    {
        var weapon = context.GetBinding<GameObject>("weapon");
        weapon?.SetActive(false);
    }
}
```

### 示例2：移动状态

```csharp
public class MoveToTargetState : MonoBehaviour, IStateHandler
{
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 _targetPos;
    
    public void OnStateEnter(StateContext context)
    {
        // 从绑定获取目标点
        var targetMarker = context.GetBinding<Transform>("target");
        if (targetMarker != null)
        {
            _targetPos = targetMarker.position;
        }
        else
        {
            context.RequestStop(); // 没有目标，直接停止
        }
    }
    
    public void OnStateUpdate(StateContext context)
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            _targetPos, 
            moveSpeed * Time.deltaTime
        );
        
        if (Vector3.Distance(transform.position, _targetPos) < 0.1f)
        {
            context.RequestComplete(); // 到达目标
        }
    }
    
    public void OnStateExit(StateContext context) { }
}
```

---

## 性能考虑

### 缓存机制

ScriptStateNode 在首次调用时会缓存脚本引用：

```csharp
// 首次进入时
OnEnterSignal() → EnsureInitialized() → 查找并缓存脚本

// 后续调用
OnUpdate() → 直接使用缓存 (O(1) 性能)
```

### 大规模场景（500+节点）

- ✅ 旧节点：零性能损耗（不受影响）
- ✅ 新节点：O(1) 缓存访问（~50次虚调用/帧）
- ❌ 避免：每帧调用 `GetComponentBinding`（已通过缓存避免）

---

## 对比：旧 vs 新

| 特性 | ComponentNode（旧） | ScriptStateNode（新） |
|------|-------------------|---------------------|
| 执行逻辑位置 | 节点类内部 | 场景脚本中 |
| 绑定对象数量 | 单一泛型类型 | 多个字段 |
| 扩展方式 | 继承节点类 | 实现接口 |
| 调试便利性 | 需要重新编译 | 场景直接调试 |
| 性能 | 直接访问 | 缓存访问（相当） |
| 脚本控制流程 | 节点内部判断 | context.RequestComplete() |

---

## 常见问题

### Q: 可以不实现某个方法吗？

A: 必须实现所有接口方法，但可以留空：

```csharp
public void OnStateUpdate(StateContext context)
{
    // 不需要每帧更新可以留空
}
```

### Q: 如何在脚本间传递数据？

A: 使用黑板数据：

```csharp
// 状态A写入
context.StateMachine.Set("attackCount", 5);

// 状态B读取
context.StateMachine.Get<int>("attackCount", out var count);
```

### Q: 旧节点还能用吗？

A: 可以！两种系统完全共存，不互相影响。

### Q: 如何调试绑定问题？

A: 使用 TryGetBinding 会自动打印错误日志：

```csharp
if (!context.TryGetBinding<Animator>("animator", out var anim))
{
    // 控制台会显示：Binding not found: field=animator
    return;
}
```

---

## 最佳实践

1. **单一职责**：一个脚本处理一个状态的逻辑
2. **使用 TryGetBinding**：防御性编程，避免空引用
3. **及时 RequestComplete**：避免状态卡死
4. **利用黑板传数据**：状态间通信的标准方式
5. **Editor 调试**：在 Scene 视图中直接调整参数

---

更多示例请查看：
- `Examples/PlayerAttackState.cs`
- `Examples/SimpleMoveState.cs`
- `Examples/CheckHealthState.cs`

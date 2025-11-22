# 控制流与数据流详解

> 理解 NGPStateMachine 的双流系统

---

## 概述

NGPStateMachine 使用**双流系统**进行节点间的通信：

- **控制流（Control Flow）**：决定节点的执行顺序和生命周期
- **数据流（Data Flow）**：在节点间传递数据

理解这两种流的区别和使用场景是掌握状态机的关键。

---

## 控制流（Control Flow）

### 设计理念

控制流负责**调度**节点的执行，回答以下问题：

- 哪个节点应该开始执行？
- 哪个节点应该停止执行？
- 执行的顺序是什么？

### 端口类型

每个节点都有 **4 个控制流端口**：

```
输入端口：
  ├─ OnEnter  - 接收启动信号
  └─ OnExit   - 接收停止信号

输出端口：
  └─ Signal   - 发出控制信号
```

### 信号语义

#### OnEnter 信号
- **含义**：通知节点开始执行
- **状态机行为**：将节点加入活跃列表，开始每帧调用 `OnUpdate()`
- **节点响应**：执行 `OnEnterSignal(string sourceId)` 方法

#### OnExit 信号
- **含义**：通知节点停止执行
- **状态机行为**：将节点从活跃列表移除，停止调用 `OnUpdate()`
- **节点响应**：执行 `OnExitSignal(string sourceId)` 方法

#### Signal 输出
- **含义**：节点主动发出的信号
- **连接规则**：
  - 连接到目标的 `OnEnter` → 启动目标
  - 连接到目标的 `OnExit` → 停止目标

### 关键概念

#### 1. 信号 ≠ 停止运行

```csharp
public override void OnEnterSignal(string sourceId)
{
    DoSomething();
    
    TriggerSignal(); // 发出信号
    
    // 节点仍在运行！
    // 如果需要停止，必须显式调用：
    // StopRunning();
}
```

**用途**：允许节点在触发下一个状态的同时继续运行。

**示例**：
```
[倒计时节点] --Signal--> [显示结束UI]
     |
  (继续运行并更新倒计时显示)
```

#### 2. 节点生命周期

```
初始化
  ↓
接收 OnEnter 信号 → OnEnterSignal()
  ↓
加入活跃列表
  ↓
每帧调用 OnUpdate()
  ↓
主动调用 StopRunning() 或 接收 OnExit 信号
  ↓
从活跃列表移除
  ↓
调用 Cleanup()
```

#### 3. 并联逻辑（OR）

多个节点可以连接到同一个输入端口：

```
[节点A] ─┐
         ├─ OnEnter ─> [目标节点]
[节点B] ─┘

只要 A 或 B 任一发出信号，目标节点就会启动
```

**用途**：实现"或"条件，多个来源触发同一动作。

---

## 数据流（Data Flow）

### 设计理念

数据流负责**传递值**，回答以下问题：

- 如何在节点间传递数据？
- 如何避免重复计算？
- 如何实现类型安全的数据传递？

### 端口定义

```csharp
[Serializable]
public class MyNode : BaseStateNode
{
    [Input("Input Value")]
    public float inputValue;
    
    [Output("Output Result")]
    public float outputResult;
    
    public override void OnEnterSignal(string sourceId)
    {
        // 读取输入
        float value = inputValue;
        
        // 计算
        outputResult = value * 2f;
        
        // 输出会自动传递给连接的节点
    }
}
```

### 数据流特性

#### 1. 自动计算与缓存

```
[计算节点A] ---value---> [使用节点B]
                    ├──> [使用节点C]
                    └──> [使用节点D]

节点 A 的 output 只计算一次，
结果会缓存并传递给 B、C、D
```

#### 2. 延迟计算（Lazy Evaluation）

数据端口的值只在**需要时**才计算：

```csharp
[Output("Expensive Calculation")]
public float expensiveValue
{
    get
    {
        // 仅在其他节点读取时才执行
        return PerformExpensiveCalculation();
    }
}
```

#### 3. 支持的数据类型

**基础类型**：
- `int`, `float`, `bool`, `string`

**Unity 类型**：
- `Vector2`, `Vector3`, `Vector4`
- `Quaternion`, `Color`
- `GameObject`, `Transform`
- `Texture2D`, `Material`, `AnimationClip`

**自定义类型**：
```csharp
[Serializable]
public struct CustomData
{
    public int id;
    public string name;
    public float value;
}

// 在节点中使用
[Input("Custom")]
public CustomData customInput;
```

---

## 控制流 vs 数据流

| 特性 | 控制流 | 数据流 |
|------|--------|--------|
| **作用** | 决定执行顺序 | 传递数据值 |
| **端口类型** | OnEnter、OnExit、Signal | 自定义类型 |
| **连接规则** | 一对多（OR） | 一对多（广播） |
| **执行时机** | 信号触发时 | 需要时计算 |
| **可视化** | 粗线条 | 细线条 |

---

## 实际应用模式

### 模式 1：纯控制流（顺序执行）

```
[Entry] --Signal--> [节点A] --Signal--> [节点B] --Signal--> [节点C]
```

**用途**：简单的线性流程，不需要传递数据。

### 模式 2：控制流 + 黑板

```csharp
// 节点 A：写入黑板
public override void OnEnterSignal(string sourceId)
{
    StateMachine.Set("player_health", 100);
    TriggerSignal();
}

// 节点 B：读取黑板
public override void OnEnterSignal(string sourceId)
{
    if (StateMachine.Get<int>("player_health", out int health))
    {
        Debug.Log($"Health: {health}");
    }
}
```

**用途**：跨节点共享全局状态。

### 模式 3：控制流 + 数据流

```
[计算伤害节点]
  ├─ Signal ──> [应用伤害节点]
  └─ Damage ──> [应用伤害节点]
```

```csharp
// 计算伤害节点
[Output("Damage")]
public float outputDamage;

public override void OnEnterSignal(string sourceId)
{
    outputDamage = CalculateDamage();
    TriggerSignal(); // 触发下一个节点
}

// 应用伤害节点
[Input("Damage")]
public float inputDamage;

public override void OnEnterSignal(string sourceId)
{
    ApplyDamage(inputDamage);
}
```

**用途**：计算结果直接传递给后续节点，避免使用黑板。

### 模式 4：数据流驱动控制流

```
[条件节点] ──┬─ True ──> [成功分支]
            └─ False ──> [失败分支]
```

```csharp
public class ConditionNode : BaseStateNode
{
    [Input("Value")]
    public float inputValue;
    
    [Output("Is Greater Than 10")]
    public bool isGreater;
    
    public override void OnEnterSignal(string sourceId)
    {
        isGreater = inputValue > 10f;
        
        // 根据条件触发不同的输出
        if (isGreater)
        {
            // 触发 true 分支
        }
        else
        {
            // 触发 false 分支
        }
    }
}
```

**用途**：根据数据结果决定执行路径。

---

## 黑板系统（Blackboard）

### 何时使用黑板

黑板是**全局键值字典**，适用于：

✅ **跨节点共享状态**
```csharp
StateMachine.Set("game_score", 1000);
```

✅ **条件判断**
```csharp
if (StateMachine.Get<bool>("is_game_over", out bool gameOver) && gameOver)
{
    // 游戏结束逻辑
}
```

✅ **运行时配置**
```csharp
StateMachine.Set("difficulty", "hard");
```

### 何时使用数据流端口

数据流端口适用于：

✅ **直接的数据传递**
- 计算结果 → 使用结果

✅ **类型安全**
- 编译时检查类型

✅ **可视化清晰**
- 明确的数据依赖关系

### 对比

| 场景 | 推荐方式 | 原因 |
|------|----------|------|
| 计算并使用结果 | 数据流 | 直接、类型安全 |
| 全局游戏状态 | 黑板 | 跨图共享 |
| 条件标志 | 黑板 | 灵活查询 |
| 临时传递值 | 数据流 | 性能好 |

---

## 高级技巧

### 1. 信号链

```
[A] --Signal--> [B] --Signal--> [C]
 |                               |
 └──────────── OnExit ───────────┘

A 启动 B，B 启动 C，C 停止 A
```

**用途**：循环流程、状态机嵌套。

### 2. 并发执行

```
[Entry] --Signal--> [节点A]
        └─ Signal--> [节点B]
        
两个节点会同时执行（并发）
```

### 3. 条件分支

```csharp
public override void OnEnterSignal(string sourceId)
{
    if (StateMachine.Get<int>("health", out int health))
    {
        if (health > 50)
        {
            TriggerSignalToPort("highHealthOutput");
        }
        else
        {
            TriggerSignalToPort("lowHealthOutput");
        }
    }
}
```

---

## 最佳实践

### ✅ 推荐

1. **控制流用于调度，数据流用于传值**
2. **简单数据用端口，复杂状态用黑板**
3. **信号发出后明确是否需要停止节点**
4. **避免循环依赖的数据流**

### ❌ 避免

1. **不要用控制流传递数据**（使用数据流或黑板）
2. **不要过度使用黑板**（数据流更直观）
3. **不要忘记停止节点**（导致内存泄漏）

---

## 调试技巧

### 1. 可视化活跃节点

Play 模式下，活跃的节点会高亮显示。

### 2. 日志输出

```csharp
public override void OnEnterSignal(string sourceId)
{
    Debug.Log($"[{NodeId}] OnEnter from {sourceId}");
    Debug.Log($"Active nodes: {StateMachine.GetActiveNodeCount()}");
}
```

### 3. 检查黑板内容

```csharp
// 打印所有黑板数据
foreach (var key in StateMachine.GetAllKeys())
{
    Debug.Log($"{key} = {StateMachine.Get(key)}");
}
```

---

## 相关文档

- [技术规格文档](../DevelopmentLog/NGP_StateMachine_TechSpec.md) - 深入架构设计
- [IStateMachine API](API_IStateMachine.md) - 黑板操作 API
- [BaseStateNode API](API_BaseStateNode.md) - 节点基类 API

---

**重要提示**：理解双流系统是高效使用状态机的基础，建议反复阅读并通过实践加深理解。

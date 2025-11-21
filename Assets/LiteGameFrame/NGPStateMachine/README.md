# NGP StateMachine - 基于 Node Graph Processor 的图结构状态机

## ⚠️ 重要更新（v2.0）

**图编辑器打开方式已改变**（类似 Timeline/Animation）：

1. 菜单：`Window > State Machine Graph Editor`
2. 在 Hierarchy 中选中带有 `NGPStateMachine` 组件的 GameObject
3. 图编辑器会自动加载该物体的状态机图

**新功能**：
- ✅ **Component Node**：绑定场景中的 Transform、Animator 等组件
- ✅ **ScriptableAsset Node**：引用 ScriptableObject 资源实现数据驱动
- ✅ 支持场景对象绑定，类似 Timeline 的 PlayableDirector

详见 `Documentation/ComponentNode_Usage.md` 和 `Documentation/ScriptableAssetNode_Usage.md`

---

## 概述

NGPStateMachine 是一个基于 Unity 插件 [Node Graph Processor](https://github.com/alelievr/NodeGraphProcessor) 的可视化状态机系统。它通过图形化界面降低了状态机调度的复杂度，让没有代码基础的人也能快速开发游戏逻辑。

### 核心特性

- **可视化编辑**: 使用 Node Graph Processor 提供的图形编辑器创建状态机
- **简化的控制流**: 
  - **输入端口**: OnEnter（启动节点）、OnExit（强制停止节点）
  - **输出端口**: Signal（单一输出信号）
  - **信号传导**: 输出连接到目标的 OnEnter 则启动目标，连接到 OnExit 则停止目标
- **数据流**: 使用节点端口和黑板（Blackboard）进行数据传递
- **灵活的节点生命周期**: 节点自主决定何时发出信号、何时停止运行
- **扩展性**: 继承 `BaseStateNode` 即可创建自定义节点

## 快速开始

### 第一步：创建状态机图资源

1. 在 Unity Project 窗口中右键点击
2. 选择 `Create > State Machine > State Machine Graph`
3. 命名为 `TestStateMachine`

### 第二步：在场景中设置状态机

1. 创建新场景或使用现有场景
2. 在 Hierarchy 中创建空物体，命名为 `StateMachineTest`
3. 添加 `NGPStateMachine` 组件
4. 将 `TestStateMachine` 资源拖到组件的 `State Graph` 字段

### 第三步：打开图编辑器

1. 菜单：`Window > State Machine Graph Editor`
2. 在 Hierarchy 中**选中** `StateMachineTest` 物体
3. 图编辑器会自动加载该物体的状态机图

**新的工作流程**（类似 Timeline/Animation）：
- 图编辑器窗口保持打开
- 选中不同的 GameObject 自动切换编辑不同的状态机
- 支持场景对象绑定（Component Node）

### 第四步：构建简单的测试图

在图编辑器中创建以下节点链：

```
Entry → Delay(2s) → Log("Step 1") → Delay(1s) → Log("Step 2")
```

**理解控制流**：
- 每个节点有 2 个输入端口（OnEnter/OnExit）和 1 个输出端口（Signal）
- Signal 输出连接到下一个节点的 OnEnter 输入 = 启动下一个节点
- Signal 输出连接到某个节点的 OnExit 输入 = 强制停止那个节点

详细步骤：
1. **添加 Entry 节点**:
   - 右键 → `State Machine > Entry`
   - Entry 节点是自动创建的起始节点

2. **添加第一个 Delay 节点**:
   - 右键 → `State Machine > Logic > Delay`
   - 在 Inspector 中设置 `duration = 2.0`

3. **连接 Entry → Delay**:
   - 从 Entry 的 `Signal` 输出端口（白色）拖线
   - 连接到 Delay 的 `OnEnter` 输入端口（绿色）
   - 这表示：Entry 发出信号后，启动 Delay 节点

4. **添加 Log 节点**:
   - 右键 → `State Machine > Logic > Log`
   - 设置 `message = "Step 1 completed"`

5. **连接 Delay → Log**:
   - 从 Delay 的 `Signal` 输出拖线到 Log 的 `OnEnter` 输入

6. **重复添加第二组 Delay + Log**

7. **保存图**（Ctrl+S）

### 第三步：在场景中设置状态机

1. 创建新场景或使用现有场景
2. 在 Hierarchy 中创建空物体，命名为 `StateMachineTest`
3. 添加 `NGPStateMachine` 组件
4. 将 `TestStateMachine` 资源拖到 `Graph` 字段

### 第四步：运行测试

1. 点击 Play 按钮
2. 观察 Console 输出：
   - 应该看到 `[EntryNode] State machine started`
   - 2 秒后看到 `[DelayNode] Delay completed`
   - 然后看到 `[LogNode] Step 1 completed`
   - 1 秒后看到下一组日志

### 故障排除

- **图编辑器无法打开**: 确保 Node Graph Processor 插件已正确导入，并检查 Console 是否有脚本编译错误。
- **状态机不执行**: 检查 NGPStateMachine 组件的 `Graph` 字段是否已赋值，并确保图中有 Entry 节点。
- **条件节点不工作**: 确保变量已通过 `StateMachine.Set()` 设置到黑板，检查 `variableNames` 列表是否包含所有变量，并确认表达式语法正确。

## 节点类型

### 内置节点

#### EntryNode（入口节点）
- **用途**: 状态机的起始点
- **行为**: 收到信号后立即触发 OnEnter 信号
- **端口**: 只有输出端口

#### DelayNode（延时节点）
- **用途**: 等待指定时间后继续执行
- **参数**: 
  - `duration`: 延时时长（秒）
- **端口**:
  - 输入: `Duration` (float) - 可选，动态设置延时时长
  - 输出: `Elapsed Time` (float) - 已经过的时间
- **行为**: 持续型节点，在 Update 中计时，到时后发出信号并停止

#### LogNode（日志节点）
- **用途**: 打印调试信息
- **参数**:
  - `logLevel`: Log/Warning/Error
  - `message`: 要打印的消息
- **端口**:
  - 输入: `Value` (object) - 可选，附加打印值
- **行为**: 瞬时型节点，打印日志后立即发出信号并停止
- **格式化字符串**: 支持在消息中使用占位符从黑板读取值。
    - **消息**: `"Player health: %f"`
    - **变量名**: `["player_health"]`
    - **说明符**: `%s` (字符串), `%d` (整数), `%f` (浮点数), `%b` (布尔值)

#### ConditionNode（条件判断节点）
- **用途**: 根据条件表达式决定是否继续执行
- **参数**:
  - `expression`: 条件表达式（如 "health > 0"）
  - `variableNames`: 表达式中使用的变量名列表
- **端口**:
  - 标准控制流端口（2输入1输出）
- **行为**: 
  - 瞬时型节点
  - 从黑板读取变量并求值
  - **条件为 true**: 发出信号继续执行
  - **条件为 false**: 不发出信号，流程终止

## 节点生命周期

每个节点都有以下生命周期方法：

```csharp
public override void OnEnterSignal(string sourceId) { /* ... */ }
public override void OnExitSignal(string sourceId) { /* ... */ }
public override void OnUpdate() { /* ... */ }
public override void Cleanup() { /* ... */ }
```

### 节点生命周期模式

#### 1. 瞬时型节点（Instant Node）
收到 OnEnter 信号后立即执行工作、发出信号并停止。
**示例**: `LogNode`, `ConditionNode`

#### 2. 持续型节点（Continuous Node）
OnEnter 启动工作，在 OnUpdate 中持续检查完成条件。完成时发出信号并停止。
**示例**: `DelayNode`

#### 3. 持久型节点（Persistent Node）
OnEnter 启动工作，在 OnUpdate 中可以多次发出信号但不停止。只有收到 OnExit 信号或达到停止条件才停止运行。
**示例**: `TimerNode`

## 创建自定义节点

### 基础模板

```csharp
using GraphProcessor;

[System.Serializable, NodeMenuItem("State Machine/Custom/YourNode")]
public class YourCustomNode : BaseStateNode
{
    public override string name => "Your Node";
    
    public override void OnEnterSignal(string sourceId)
    {
        // 实现你的逻辑
        TriggerSignal();
        StopRunning();
    }
}
```

## 黑板（Blackboard）系统

黑板是一个全局键值存储，用于在节点之间共享数据。

### 在代码中使用黑板

```csharp
// 设置值
StateMachine.Set("playerHealth", 100);

// 读取值
if (StateMachine.Get<int>("playerHealth", out var health)) { /* ... */ }

// 查找值（带默认值）
float damage = StateMachine.Find<float>("damage", 10f);
```

### 黑板操作节点

#### Set Blackboard Value
- **路径**: `State Machine/Blackboard/Set Value`
- **功能**: 将输入值写入黑板。

#### Get Blackboard Value (Action)
- **路径**: `State Machine/Blackboard/Get Value (Action)`
- **功能**: 从黑板读取值并通过输出端口传递。

#### Get Blackboard Value (Data)
- **路径**: `State Machine/Blackboard/Get Value`
- **功能**: 纯数据节点，用于将黑板的值连接到其他节点的输入端口。

### 使用场景

- **参数化**: 使用黑板的值动态配置节点（如 `Delay` 的时长）。
- **条件判断**: `ConditionNode` 从黑板读取变量进行判断。
- **数据共享**: 在不同节点之间传递和修改游戏状态（如玩家生命值、得分等）。

## 架构概览

```
StateMachineGraph (ScriptableObject)
    ↓
NGPStateMachine (MonoBehaviour)
    - 管理图的生命周期和执行
    - 持有黑板实例
    ↓
BaseStateNode (抽象类)
    - 所有状态节点的基类
    - 定义了生命周期和信号传递机制
```

## 依赖项

- Unity 2021.3.x LTS 或更高版本
- Node Graph Processor v1.3.1+

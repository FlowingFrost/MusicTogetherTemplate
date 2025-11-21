# Node Graph Processor 状态机技术规格文档

**项目名称**: LightGameFrame - Node Graph Processor Edition  
**文档版本**: 1.0  
**创建日期**: 2025年11月21日  
**目标 Unity 版本**: 2021.3.x LTS  
**依赖插件**: Node Graph Processor

---

## 1. 项目目标与设计理念

### 1.1 核心目标
1. **可视化调度系统**: 将复杂的脚本调度逻辑以图形方式呈现，降低系统间通信复杂度
2. **降低开发门槛**: 
   - 让非程序员能够通过拼装节点快速构建游戏逻辑
   - 简化 AI 生成脚本的难度（节点无需考虑架构与通信设计）
3. **模块化与解耦**: 每个节点自包含逻辑，图结构仅负责执行顺序编排

### 1.2 设计理念
> **"功能全部在脚本完成，图结构只负责调度"**

- 状态机不关心节点的具体实现细节
- 节点通过标准化的输入/输出接口与外界交互
- 支持热插拔式的节点扩展，无需修改核心框架

### 1.3 与原版 LiteGameFrame 的关键差异

| 特性 | 原版 | NGP 版本 |
|------|------|----------|
| **图编辑器** | 手动配置 ScriptableObject | 可视化节点编辑器（NGP） |
| **节点参数** | 在外部 SO/Component 配置 | 直接在图编辑器中配置 |
| **通信方式** | 仅全局字典 (`Get/Set`) | 端口连线 + 全局黑板 |
| **边的语义** | 执行流 + 可附加条件字符串 | 控制流 + 数据流（无条件附着） |
| **条件判断** | 边上的 `LaunchCondition` 字符串 | 专用条件判断节点 |
| **节点类型** | SO/MonoBehaviour | 纯逻辑/条件/SO/Component |

---

## 2. 核心架构设计

### 2.1 双流系统

#### 2.1.1 控制流（Control Flow）
控制流决定节点的执行顺序，使用两种控制信号：

- **`OnEnter` 信号**: 通知节点开始执行
  - 节点接收到来自前驱节点的 `OnEnter` 输出后开始执行
  
- **`OnExit` 信号**: 通知节点停止执行
  - 节点接收到来自前驱节点的 `OnExit` 输出后停止执行

**端口规则**:
- 所有的节点都有 **2 个控制输入端口**（`OnEnter` 与 `OnExit`）
- 所有的节点都有 **2 个控制输出端口**（`OnEnter` 与 `OnExit`）
- 节点自行决定何时输出 `OnEnter` 与 `OnExit` 信号
- **信号输出与节点运行无必然联系**：节点可以在输出信号后继续运行

**节点生命周期管理**:
- **节点的启动**: 由状态机负责（当节点收到 `OnEnter` 信号时，状态机将其加入活跃列表）
- **节点的停止**: 由节点自身负责（节点主动调用 `StateMachine.RemoveActiveNode(this)` 请求移除）
- **关键原则**: 发出输出信号 ≠ 停止运行，节点可以：
  - 发出信号后立即停止（如 `SaveDataNode`）
  - 发出信号后继续运行（如 `CountdownNode` 发出 `OnTick` 后继续倒计时）
  - 永久运行直到收到 `OnExit` 信号（如监听节点）

**并联逻辑**:
- 控制流端口支持**多条连线并联**
- 只要**任意一条输入连线**收到信号，节点即可响应（OR 逻辑）
- 例如：节点 A 的 `OnEnter` 可以连接到多个后续节点，触发时所有连接的节点都会收到信号

#### 2.1.2 数据流（Data Flow）
数据流通过节点端口连接传递值：

- **输入数据端口**: 从前驱节点或黑板读取数据
  - 支持基础类型：`int`, `float`, `bool`, `string`
  - 支持 Unity 类型：`Vector3`, `GameObject`, `Transform` 等
  - 支持自定义类型（需实现序列化）

- **输出数据端口**: 将计算结果传递给后续节点
  - 可连接到多个后续节点的输入端口
  - 运行时计算并缓存值（避免重复计算）

### 2.2 黑板系统（Blackboard）

保留原版 StateMachine 的全局字典功能，作为**跨节点的全局状态共享机制**：

```csharp
public interface IStateMachine
{
    // 黑板操作
    bool Get<T>(string key, out T value);
    bool Set<T>(string key, T value);
    bool Find<T>(string key, out Type valueType);
    
    // 节点生命周期控制
    void ProcessEnterSignal(IStateNode node, string sourceId);  // 处理节点接收 OnEnter 信号
    void ProcessExitSignal(IStateNode node, string sourceId);   // 处理节点接收 OnExit 信号
    void RemoveActiveNode(IStateNode node);                     // 节点主动请求停止运行
    void AddActiveNode(IStateNode node);                        // 节点主动请求开始运行（罕见）
}
```

**使用场景**:
- 存储全局游戏状态（玩家血量、关卡进度等）
- 节点间的松耦合通信（避免直接连线导致图结构过于复杂）
- 持久化数据在不同状态机实例间共享

---

## 3. 节点类型系统

### 3.1 节点基类设计

所有节点继承自 Node Graph Processor 的 `BaseNode`，并实现 `IStateNode` 接口：

```csharp
public interface IStateNode
{
    string NodeId { get; }
    IStateMachine StateMachine { get; }
    
    // 生命周期回调（由状态机或信号触发调用）
    void Initialize(IStateMachine stateMachine);
    void OnEnterSignal(string sourceId);  // 接收到 OnEnter 控制信号
    void OnExitSignal(string sourceId);   // 接收到 OnExit 控制信号
    void OnUpdate();                      // 每帧更新（仅活跃节点）
    void Cleanup();                       // 清理资源
    
    // 节点主动控制自身运行状态的方法
    void StopRunning();                   // 节点主动停止运行（内部调用 StateMachine.RemoveActiveNode）
}
```

### 3.2 四种节点类型

#### 3.2.1 纯逻辑节点（Pure Logic Node）
**特点**:
- 所有参数直接在节点上通过 `[Input]` 特性声明
- 序列化由 Node Graph Processor 自动处理
- 不依赖外部资产

**示例节点**:
- `DelayNode`: 延时指定秒数后发出完成信号
- `LogNode`: 打印日志信息
- `MathNode`: 执行数学运算（加减乘除）
- `SetBlackboardNode`: 写入黑板值
- `GetBlackboardNode`: 读取黑板值

**代码示例**:
```csharp
[System.Serializable, NodeMenuItem("Logic/Delay")]
public class DelayNode : BaseStateNode
{
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    [Input("Duration")] public float duration = 1.0f;
    
    private float _elapsedTime;
    private bool _isRunning;
    
    public override void OnEnterSignal(string sourceId)
    {
        _elapsedTime = 0f;
        _isRunning = true;
        Debug.Log($"DelayNode started, will wait {duration}s");
    }
    
    public override void OnExitSignal(string sourceId)
    {
        _isRunning = false;
        Debug.Log("DelayNode stopped by exit signal");
    }
    
    public override void OnUpdate()
    {
        if (!_isRunning) return;
        
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= duration)
        {
            _isRunning = false;
            // 延时结束，发出 OnEnter 信号给后续节点
            TriggerOutput(nameof(outputEnter));
            
            // 主动请求停止运行（从活跃列表移除）
            StopRunning();
        }
    }
    
    // 节点主动停止运行
    public override void StopRunning()
    {
        StateMachine.RemoveActiveNode(this);
    }
}
```

#### 3.2.2 条件判断节点（Condition Node）
**特点**:
- 使用 `ConditionEvaluator` 解析字符串表达式
- 支持动态输入端口数量（运行时可添加/删除变量）
- 输出两个控制信号：`OnEnter` 和 `OnExit`

**表达式语法**:
```
支持运算符: &&, ||, !, ==, !=, >, <, >=, <=
支持类型: 数字, 字符串字面量, 变量引用
示例: (health > 0) && (enemyCount < 5) || hasPowerUp
```

**代码示例**:
```csharp
[System.Serializable, NodeMenuItem("Logic/Condition")]
public class ConditionNode : BaseStateNode
{
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter_True"), Vertical] public ControlFlow outputTrue;
    [Output("OnEnter_False"), Vertical] public ControlFlow outputFalse;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    public string expression = "value > 0";
    
    // 动态输入端口（编辑器中可添加）
    [Input] public Dictionary<string, object> variables = new();
    
    public override void OnEnterSignal(string sourceId)
    {
        bool result = ConditionEvaluator.Evaluate(expression, variables);
        
        // 根据条件结果发出不同的 OnEnter 信号
        if (result)
            TriggerOutput(nameof(outputTrue));
        else
            TriggerOutput(nameof(outputFalse));
        
        // 条件节点是瞬时节点，立即停止运行
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        // 转发 OnExit 信号
        TriggerOutput(nameof(outputExit));
    }
    
    public override void StopRunning()
    {
        StateMachine.RemoveActiveNode(this);
    }
}
```

#### 3.2.3 ScriptableObject 节点（SO Node）
**特点**:
- 引用外部 ScriptableObject 资产
- 用于复杂配置型系统（存档、关卡数据、技能配置等）
- SO 需要实现 `IStateLogic` 接口

**使用场景**:
- 存档系统：`SaveDataNode` 引用 `SaveDataSO`
- 关卡配置：`LoadLevelNode` 引用 `LevelConfigSO`
- 技能系统：`CastSkillNode` 引用 `SkillDataSO`

**代码示例**:
```csharp
[System.Serializable, NodeMenuItem("System/SaveData")]
public class SaveDataNode : BaseStateNode
{
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    public SaveDataSO saveDataConfig;  // 在 Inspector 中拖入
    
    [Input] public string saveKey;
    [Input] public object saveValue;
    
    public override void OnEnterSignal(string sourceId)
    {
        // 执行保存操作
        saveDataConfig.Save(saveKey, saveValue);
        
        // 立即发出完成信号
        TriggerOutput(nameof(outputEnter));
        
        // 保存完成，立即停止运行
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        TriggerOutput(nameof(outputExit));
    }
    
    public override void StopRunning()
    {
        StateMachine.RemoveActiveNode(this);
    }
}
```

#### 3.2.4 Component 节点（Component Node）
**特点**:
- 引用场景中的 MonoBehaviour 组件
- 用于与游戏对象交互（移动角色、播放动画等）
- Component 需要实现 `IStateComponent` 接口

**使用场景**:
- 角色控制：`MovePlayerNode` 引用 `PlayerController`
- UI 交互：`ShowDialogNode` 引用 `DialogManager`
- 音效播放：`PlaySoundNode` 引用 `AudioManager`

**代码示例**:
```csharp
[System.Serializable, NodeMenuItem("Character/MovePlayer")]
public class MovePlayerNode : BaseStateNode
{
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    public PlayerController playerController;  // 在 Inspector 中拖入
    
    [Input] public Vector3 targetPosition;
    [Input] public float moveSpeed = 5f;
    
    private bool _isMoving = false;
    
    public override void OnEnterSignal(string sourceId)
    {
        _isMoving = true;
        playerController.StartMoveTo(targetPosition, moveSpeed);
        Debug.Log($"Player started moving to {targetPosition}");
    }
    
    public override void OnExitSignal(string sourceId)
    {
        _isMoving = false;
        playerController.StopMoving();
        Debug.Log("Player movement stopped by exit signal");
        TriggerOutput(nameof(outputExit));
    }
    
    public override void OnUpdate()
    {
        if (_isMoving && playerController.HasReachedTarget())
        {
            _isMoving = false;
            // 到达目标，发出完成信号
            TriggerOutput(nameof(outputEnter));
            
            // 移动完成，停止运行
            StopRunning();
        }
    }
    
    public override void StopRunning()
    {
        if (_isMoving)
        {
            _isMoving = false;
            playerController.StopMoving();
        }
        StateMachine.RemoveActiveNode(this);
    }
}
```

---

## 4. 状态机运行时

### 4.1 StateMachine 核心职责

```csharp
public class NGPStateMachine : MonoBehaviour
{
    [SerializeField] private BaseGraph stateGraph;  // NGP 图资产
    
    private List<IStateNode> _activeNodes = new();
    private Dictionary<string, object> _blackboard = new();
    
    void Awake()
    {
        BuildGraph();
    }
    
    void Update()
    {
        foreach (var node in _activeNodes)
        {
            node.OnUpdate();
        }
    }
    
    private void BuildGraph()
    {
        // 从 NGP 图中加载所有节点
        // 初始化节点连接关系
        // 查找入口节点并启动
    }
    
    public void ProcessEnterSignal(IStateNode node, string sourceId)
    {
        // 如果节点不在活跃列表，添加进去
        if (!_activeNodes.Contains(node))
        {
            _activeNodes.Add(node);
        }
        
        // 触发节点的 OnEnter 处理
        node.OnEnterSignal(sourceId);
    }
    
    public void ProcessExitSignal(IStateNode node, string sourceId)
    {
        // 触发节点的 OnExit 处理
        node.OnExitSignal(sourceId);
        
        // 注意：不自动移除节点，由节点自己决定是否调用 StopRunning()
    }
    
    // 节点主动请求停止运行
    public void RemoveActiveNode(IStateNode node)
    {
        if (_activeNodes.Remove(node))
        {
            Debug.Log($"Node {node.NodeId} stopped running");
        }
    }
    
    // 节点主动请求开始运行（罕见场景，如节点需要重新激活）
    public void AddActiveNode(IStateNode node)
    {
        if (!_activeNodes.Contains(node))
        {
            _activeNodes.Add(node);
            Debug.Log($"Node {node.NodeId} manually added to active list");
        }
    }
}
```

### 4.2 执行流程

```
1. Awake: BuildGraph()
   ├─ 加载图资产
   ├─ 实例化所有节点
   ├─ 建立连接关系
   └─ 启动入口节点

2. Update: 遍历活跃节点调用 OnUpdate()

3. 节点接收 OnEnter 信号时:
   ├─ 状态机将节点添加到活跃列表（如果尚未添加）
   ├─ 调用节点的 OnEnterSignal()
   ├─ 节点内部开始执行逻辑
   ├─ 节点可选择立即或延迟发出输出信号
   └─ **关键**: 发出信号 ≠ 停止运行

4. 节点接收 OnExit 信号时:
   ├─ 调用节点的 OnExitSignal()
   ├─ 节点内部处理退出逻辑
   ├─ 节点可选择转发 OnExit 信号给后续节点
   └─ **关键**: 节点自己决定是否调用 StopRunning()

5. 节点主动停止运行:
   ├─ 节点内部逻辑完成（如延时结束、移动到达目标）
   ├─ 调用 StopRunning() 或 StateMachine.RemoveActiveNode(this)
   ├─ 从活跃列表移除
   └─ 不再接收 OnUpdate() 调用

6. 并联处理:
   ├─ 多个节点可连接到同一输出端口
   ├─ 信号触发时广播到所有连接的节点
   └─ 每个节点独立响应信号

7. 销毁时:
   ├─ 遍历所有活跃节点调用 Cleanup()
   ├─ 清空活跃列表
   └─ 释放图资源
```

---

## 5. Node Graph Processor 集成

### 5.1 需要使用的 NGP API

| API | 用途 |
|-----|------|
| `BaseGraph` | 图资产的基类，用于序列化整个状态机图 |
| `BaseNode` | 所有节点的基类，提供序列化和端口管理 |
| `[Input]` / `[Output]` | 声明端口的特性 |
| `NodePort` | 端口对象，用于获取连接信息 |
| `BaseGraphWindow` | 编辑器窗口基类，用于自定义图编辑器 |
| `NodeView` | 节点视图基类，用于自定义节点外观 |

### 5.2 核心映射关系

```csharp
// 原版 StateGraph -> NGP BaseGraph
[System.Serializable]
public class StateMachineGraph : BaseGraph
{
    public string entryNodeId;
    
    // NGP 会自动序列化 nodes 和 edges
}

// 原版 IStateNode -> NGP BaseNode + IStateNode
public abstract class BaseStateNode : BaseNode, IStateNode
{
    // NGP 提供的端口系统 - 所有节点都有这 4 个标准端口
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    // IStateNode 接口实现
    public abstract void OnEnterSignal(string sourceId);
    public abstract void OnExitSignal(string sourceId);
    public virtual void OnUpdate() { }
    public virtual void Cleanup() { }
    
    // 节点生命周期控制
    public virtual void StopRunning()
    {
        StateMachine?.RemoveActiveNode(this);
    }
    
    // 辅助方法：触发输出端口
    protected void TriggerOutput(string portName)
    {
        // NGP 的端口触发逻辑
        var port = GetOutputPort(portName);
        foreach (var connection in port.GetEdges())
        {
            var targetNode = connection.inputNode as BaseStateNode;
            if (portName.Contains("OnEnter"))
                targetNode?.OnEnterSignal(this.GUID);
            else if (portName.Contains("OnExit"))
                targetNode?.OnExitSignal(this.GUID);
        }
    }
}

// 控制流端口类型
[System.Serializable]
public struct ControlFlow
{
    // 空结构体，仅用于标识控制流连接
}
```

### 5.3 编辑器扩展

```csharp
public class StateMachineGraphWindow : BaseGraphWindow
{
    [MenuItem("Window/State Machine Editor")]
    public static void OpenWindow()
    {
        GetWindow<StateMachineGraphWindow>("State Machine");
    }
    
    protected override void InitializeWindow(BaseGraph graph)
    {
        // 自定义工具栏
        // 添加黑板面板
        // 配置节点搜索菜单
    }
}
```

---

## 6. 节点开发范式

### 6.1 开发步骤

1. **继承基类**
   ```csharp
   public class MyCustomNode : BaseStateNode
   ```

2. **声明端口**
   ```csharp
   // 标准控制流端口（继承自 BaseStateNode）
   [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
   [Input("OnExit"), Vertical] public ControlFlow inputExit;
   [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
   [Output("OnExit"), Vertical] public ControlFlow outputExit;
   
   // 额外的数据端口
   [Input("Value")] public int inputValue;
   [Output("Result")] public int outputResult;
   ```

3. **实现生命周期**
   ```csharp
   public override void OnEnterSignal(string sourceId)
   {
       // 节点接收到 OnEnter 信号时的逻辑
       Debug.Log($"Node entered from {sourceId}");
   }
   
   public override void OnExitSignal(string sourceId)
   {
       // 节点接收到 OnExit 信号时的逻辑
       Debug.Log($"Node exited from {sourceId}");
       TriggerOutput(nameof(outputExit));
   }
   
   public override void OnUpdate()
   {
       // 每帧更新逻辑（可选）
   }
   
   public override void Cleanup()
   {
       // 清理资源（可选）
   }
   
   // 节点主动停止运行（由节点内部逻辑决定何时调用）
   public override void StopRunning()
   {
       // 清理内部状态
       _isProcessing = false;
       
       // 请求状态机移除此节点
       StateMachine.RemoveActiveNode(this);
   }
   ```

4. **注册到菜单**
   ```csharp
   [NodeMenuItem("Category/NodeName")]
   ```

### 6.2 开发规范

#### 6.2.1 命名约定
- 节点类名：`XxxNode`（如 `DelayNode`, `LogNode`）
- 标准控制端口：必须使用 `inputEnter`, `inputExit`, `outputEnter`, `outputExit`
- 额外控制端口：描述性命名（如 `outputTrue`, `outputFalse` 用于条件分支）
- 数据端口：描述性命名（如 `duration`, `targetPosition`）

#### 6.2.2 生命周期约定
- **`OnEnterSignal`**: 必须实现，处理节点接收到 OnEnter 信号的逻辑
- **`OnExitSignal`**: 必须实现，处理节点接收到 OnExit 信号的逻辑
- **`OnUpdate`**: 可选，仅需要持续更新的节点实现（如 `DelayNode`, `MovePlayerNode`）
- **`Cleanup`**: 可选，用于清理资源（如关闭文件、释放对象池）
- **`StopRunning`**: 可选重写，节点完成工作后主动调用以停止运行

**关键职责划分**:
- **状态机负责**: 
  - 将收到 `OnEnter` 信号的节点加入活跃列表
  - 每帧调用活跃节点的 `OnUpdate()`
  - 响应节点的 `RemoveActiveNode()` 请求
  
- **节点负责**:
  - 决定何时发出输出信号（可以多次发出）
  - 决定何时停止运行（调用 `StopRunning()`）
  - 输出信号和停止运行是**独立的两个操作**

**典型模式**:
1. **瞬时节点**（如 `SaveDataNode`, `ConditionNode`）:
   ```csharp
   OnEnterSignal() {
       DoWork();
       TriggerOutput();
       StopRunning();  // 立即停止
   }
   ```

2. **持续节点**（如 `DelayNode`, `MovePlayerNode`）:
   ```csharp
   OnEnterSignal() {
       StartWork();
   }
   OnUpdate() {
       if (WorkComplete()) {
           TriggerOutput();
           StopRunning();  // 工作完成后停止
       }
   }
   ```

3. **持久节点**（如监听器、循环节点）:
   ```csharp
   OnEnterSignal() {
       StartListening();
   }
   OnUpdate() {
       if (EventTriggered()) {
           TriggerOutput();  // 发出信号但不停止
       }
   }
   OnExitSignal() {
       StopListening();
       StopRunning();  // 收到退出信号才停止
   }
   ```

#### 6.2.3 线程安全
- 所有节点逻辑运行在主线程
- 如需异步操作，使用 `CoroutineRunner` 或 `async/await`
- 更新黑板数据时无需加锁（单线程执行）

### 6.3 示例：完整节点实现

```csharp
using UnityEngine;
using GraphProcessor;
using LiteGameFrame.StateMachine;

[System.Serializable, NodeMenuItem("Example/CountdownNode")]
public class CountdownNode : BaseStateNode
{
    // 标准控制流端口（继承自 BaseStateNode）
    // inputEnter, inputExit, outputEnter, outputExit
    
    // 额外的控制流端口
    [Output("OnTick"), Vertical] 
    public ControlFlow outputTick;
    
    // 数据端口
    [Input("Count From")] 
    public int startCount = 10;
    
    [Output("Current Count")] 
    public int currentCount;
    
    // 内部状态
    private float _timer;
    private bool _isRunning;
    
    public override string name => "Countdown";
    
    public override void OnEnterSignal(string sourceId)
    {
        currentCount = startCount;
        _timer = 0f;
        _isRunning = true;
        
        Debug.Log($"Countdown started from {startCount} (source: {sourceId})");
    }
    
    public override void OnExitSignal(string sourceId)
    {
        _isRunning = false;
        Debug.Log("Countdown stopped by exit signal");
        
        // 转发 OnExit 信号
        TriggerOutput(nameof(outputExit));
    }
    
    public override void OnUpdate()
    {
        if (!_isRunning) return;
        
        _timer += Time.deltaTime;
        if (_timer >= 1f)
        {
            _timer = 0f;
            currentCount--;
            
            // 每秒触发一次 Tick 信号
            TriggerOutput(nameof(outputTick));
            
            if (currentCount <= 0)
            {
                _isRunning = false;
                // 倒计时完成，发出 OnEnter 信号
                TriggerOutput(nameof(outputEnter));
                
                // 倒计时结束，主动停止运行
                StopRunning();
            }
        }
    }
    
    public override void Cleanup()
    {
        _isRunning = false;
    }
    
    public override void StopRunning()
    {
        _isRunning = false;
        StateMachine.RemoveActiveNode(this);
    }
}
```

---

## 7. 兼容性与迁移

### 7.1 保留的原版特性
- ✅ `ConditionEvaluator` 表达式解析器
- ✅ 黑板系统（全局字典）
- ✅ `ServiceLocator` 和基础设施
- ✅ `DebugLogger` 调试工具
- ✅ SO/Component 节点支持

### 7.2 废弃的原版特性
- ❌ 边上的 `LaunchCondition` 字符串
- ❌ 手动配置 `BindingData` 列表
- ❌ 反射创建节点（改为 NGP 序列化）
- ❌ `StateNodeData` 结构体（改为 NGP 节点对象）

### 7.3 迁移指南

#### 从原版迁移步骤：
1. 安装 Node Graph Processor 插件
2. 创建新的 `StateMachineGraph` 资产
3. 在图编辑器中重建节点和连接
4. 将原 `BindingData` 中的 SO/Component 引用拖入对应节点
5. 将边上的条件字符串迁移到 `ConditionNode`
6. 测试并调整节点参数

---

## 8. 性能与优化

### 8.1 性能考量
- **图构建**: 仅在 `Awake` 执行一次，运行时无开销
- **节点更新**: 仅更新活跃节点（`_activeNodes` 列表）
- **端口求值**: NGP 自动缓存端口值，避免重复计算
- **黑板访问**: 字典查找 O(1) 复杂度

### 8.2 优化建议
- 避免在 `OnUpdate` 中进行重复的复杂计算
- 使用对象池管理频繁创建的节点（如粒子效果节点）
- 条件表达式使用编译缓存（`ConditionEvaluator` 已实现）
- 大型状态机考虑拆分为多个子图

---

## 9. 调试与测试

### 9.1 调试工具
- **图编辑器高亮**: 运行时高亮当前活跃节点
- **黑板查看器**: 实时查看黑板变量值
- **日志节点**: 快速插入 `LogNode` 打印中间值
- **断点节点**: `BreakpointNode` 暂停状态机执行

### 9.2 单元测试范式
```csharp
[Test]
public void TestDelayNode()
{
    var graph = ScriptableObject.CreateInstance<StateMachineGraph>();
    var delayNode = graph.AddNode<DelayNode>();
    delayNode.duration = 1.0f;
    
    var stateMachine = new GameObject().AddComponent<NGPStateMachine>();
    stateMachine.stateGraph = graph;
    
    // 模拟发送 OnEnter 信号
    delayNode.OnEnterSignal("TestSource");
    
    // 模拟 1 秒的更新
    for (int i = 0; i < 60; i++)
    {
        delayNode.OnUpdate();
        yield return null;
    }
    
    // 验证节点已完成（发出 outputEnter 信号）
    Assert.IsFalse(delayNode._isRunning);
}

[Test]
public void TestConditionNode_TrueBranch()
{
    var conditionNode = new ConditionNode();
    conditionNode.expression = "value > 5";
    conditionNode.variables["value"] = 10;
    
    bool trueTriggered = false;
    bool falseTriggered = false;
    
    // 模拟连接输出端口（实际由 NGP 处理）
    conditionNode.outputTrue.OnTriggered += () => trueTriggered = true;
    conditionNode.outputFalse.OnTriggered += () => falseTriggered = true;
    
    // 触发 OnEnter 信号
    conditionNode.OnEnterSignal("TestSource");
    
    // 验证走了 True 分支
    Assert.IsTrue(trueTriggered);
    Assert.IsFalse(falseTriggered);
}

[Test]
public void TestParallelSignals()
{
    // 测试并联逻辑：一个输出连接到多个节点
    var sourceNode = new DelayNode();
    var targetNode1 = new LogNode();
    var targetNode2 = new LogNode();
    
    int node1Triggered = 0;
    int node2Triggered = 0;
    
    targetNode1.OnEnterSignal = (id) => node1Triggered++;
    targetNode2.OnEnterSignal = (id) => node2Triggered++;
    
    // 模拟并联连接（实际由 NGP 处理）
    sourceNode.outputEnter.ConnectTo(targetNode1.inputEnter);
    sourceNode.outputEnter.ConnectTo(targetNode2.inputEnter);
    
    // 触发信号
    sourceNode.TriggerOutput(nameof(sourceNode.outputEnter));
    
    // 验证两个节点都收到信号
    Assert.AreEqual(1, node1Triggered);
    Assert.AreEqual(1, node2Triggered);
}
```

---

## 10. 扩展与未来计划

### 10.1 计划中的功能
- [ ] 子图支持（嵌套状态机）
- [ ] 并行节点（同时执行多个分支）
- [ ] 事件节点（响应全局事件）
- [ ] 动画集成节点（Timeline、Animator）
- [ ] 可视化调试工具增强

### 10.2 社区扩展指南
- 贡献自定义节点到节点库
- 分享常用图模板（对话系统、战斗系统等）
- 编写节点开发教程和最佳实践

---

## 附录 A: 术语表

| 术语 | 定义 |
|------|------|
| **节点（Node）** | 状态机中的最小执行单元，封装特定功能 |
| **端口（Port）** | 节点上的输入/输出接口，用于连接和传递数据 |
| **控制流（Control Flow）** | 决定节点执行顺序的信号流 |
| **数据流（Data Flow）** | 在节点间传递值的连接 |
| **黑板（Blackboard）** | 全局共享的数据存储，用于跨节点通信 |
| **图（Graph）** | 完整的状态机结构，包含所有节点和连接 |
| **NGP** | Node Graph Processor 插件的缩写 |

---

## 附录 B: 参考资源

- **Node Graph Processor 文档**: https://github.com/alelievr/NodeGraphProcessor
- **Unity Visual Scripting**: Unity 官方可视化脚本系统
- **原版 LiteGameFrame**: 本项目的前身实现
- **ConditionEvaluator 设计文档**: `EvaluatorDesign.md`

---

**文档维护**: 本文档应随项目演进持续更新。如有架构变更，请及时同步修改。

# Node Graph Processor API 分析与映射方案

**文档日期**: 2025年11月21日  
**目的**: 分析 NGP 运行时 API，并设计 LiteGameFrame 状态机到 NGP 的映射方案

---

## 1. NGP 核心运行时组件分析

### 1.1 BaseNode 核心特性

```csharp
namespace GraphProcessor
{
    public abstract class BaseNode
    {
        // 节点标识
        public string GUID;                    // 唯一标识符
        public virtual string name;            // 节点名称
        public Rect position;                  // 节点位置
        
        // 端口容器
        public readonly NodeInputPortContainer inputPorts;
        public readonly NodeOutputPortContainer outputPorts;
        
        // 生命周期方法
        public void Initialize(BaseGraph graph);   // 由 Graph 调用初始化
        public virtual void InitializePorts();     // 初始化端口
        public virtual void Enable();              // 节点启用
        public virtual void Disable();             // 节点禁用
        public virtual void OnNodeCreated();       // 节点创建时调用
        
        // 端口管理
        protected NodePort AddPort(bool input, string fieldName, PortData portData);
        protected void RemovePort(bool input, NodePort port);
        public NodePort GetPort(string fieldName, string identifier = null);
        
        // 事件
        public event Action<SerializableEdge> onAfterEdgeConnected;
        public event Action<SerializableEdge> onAfterEdgeDisconnected;
        
        // 字段反射机制
        protected BaseGraph graph;  // 所属图
    }
}
```

**关键发现**:
- ✅ NGP 使用 `[Input]` 和 `[Output]` 特性标记字段自动生成端口
- ✅ 端口通过反射机制自动创建，无需手动管理
- ✅ 生命周期：`OnNodeCreated` → `Initialize` → `Enable` → `Disable`
- ✅ 连接/断开边时会触发事件

### 1.2 端口与连接系统

```csharp
// 端口数据描述
public class PortData
{
    public string identifier;           // 端口唯一标识符（用于多端口场景）
    public string displayName;          // 显示名称
    public Type displayType;            // 显示类型（用于着色）
    public bool acceptMultipleEdges;    // 是否接受多条连线
    public bool vertical;               // 是否垂直显示
    public string tooltip;              // 提示信息
}

// 运行时端口
public class NodePort
{
    public string fieldName;            // 对应的 C# 字段名
    public BaseNode owner;              // 所属节点
    public FieldInfo fieldInfo;         // 字段反射信息
    public PortData portData;           // 端口数据
    
    public void Add(SerializableEdge edge);          // 连接边
    public void Remove(SerializableEdge edge);       // 断开边
    public List<SerializableEdge> GetEdges();        // 获取所有连接的边
    
    public void PushData();   // 推送数据（输出端口）
    public void PullData();   // 拉取数据（输入端口）
}

// 边连接
public class SerializableEdge
{
    public string GUID;
    public BaseNode inputNode;
    public BaseNode outputNode;
    public NodePort inputPort;
    public NodePort outputPort;
    public string inputFieldName;
    public string outputFieldName;
    public string inputPortIdentifier;
    public string outputPortIdentifier;
    
    public object passThroughBuffer;  // 自定义 IO 的临时缓冲区
}
```

**关键发现**:
- ✅ 端口通过 `fieldName` 关联到节点的 C# 字段
- ✅ 边存储输入/输出节点、端口、字段名等完整信息
- ✅ `PushData` / `PullData` 机制用于自动数据传递（通过 Reflection 或委托）
- ✅ `passThroughBuffer` 用于自定义 IO 场景

### 1.3 BaseGraph 核心功能

```csharp
public class BaseGraph : ScriptableObject
{
    // 节点和边的存储
    [SerializeReference]
    public List<BaseNode> nodes;
    [SerializeField]
    public List<SerializableEdge> edges;
    
    // 快速查找字典
    public Dictionary<string, BaseNode> nodesPerGUID;
    public Dictionary<string, SerializableEdge> edgesPerGUID;
    
    // 生命周期
    protected virtual void OnEnable();
    protected virtual void OnDisable();
    
    // 节点管理
    public BaseNode AddNode(BaseNode node);
    public void RemoveNode(BaseNode node);
    
    // 连接管理
    public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true);
    public void Disconnect(BaseNode inputNode, string inputFieldName, BaseNode outputNode, string outputFieldName);
    public void Disconnect(string edgeGUID);
    
    // 事件
    public event Action<GraphChanges> onGraphChanges;
    public event Action onEnabled;
}
```

**关键发现**:
- ✅ Graph 是 ScriptableObject，可序列化保存
- ✅ 使用 `[SerializeReference]` 支持多态序列化节点
- ✅ Graph 负责节点的初始化和生命周期管理
- ✅ 连接/断开操作由 Graph 统一管理

---

## 2. LiteGameFrame 到 NGP 的映射方案

### 2.1 核心类映射

| LiteGameFrame | NGP 映射 | 说明 |
|---------------|----------|------|
| `StateGraph` (ScriptableObject) | `BaseGraph` | 状态机图资产 |
| `IStateNode` (接口) | `BaseNode` + 自定义接口 | 状态节点基类 |
| `StateMachine` (MonoBehaviour) | 自定义运行时管理器 | 不直接使用 NGP 的处理器 |
| `StateNodeData` (结构体) | NGP 节点字段 | 不再需要，由 NGP 序列化 |
| `BindingData` (结构体) | NGP [Input] 字段 | 不再需要，直接在节点配置 |

### 2.2 信号系统实现策略

#### 方案 A: 使用 NGP 的数据流 + 自定义处理（推荐）

**设计思路**:
- 控制流信号 (`OnEnter`/`OnExit`) 作为**特殊类型的数据端口**
- 使用 `struct ControlFlow` 作为端口类型标识
- 节点内部监听端口变化，手动触发信号处理

```csharp
// 控制流标识类型
[Serializable]
public struct ControlFlow
{
    // 空结构体，仅用于类型标识和连接验证
}

// 状态节点基类
public abstract class BaseStateNode : BaseNode, IStateNode
{
    // 标准控制流端口（所有节点都有）
    [Input("OnEnter"), Vertical] public ControlFlow inputEnter;
    [Input("OnExit"), Vertical] public ControlFlow inputExit;
    [Output("OnEnter"), Vertical] public ControlFlow outputEnter;
    [Output("OnExit"), Vertical] public ControlFlow outputExit;
    
    // 状态机引用
    protected IStateMachine StateMachine { get; private set; }
    
    // IStateNode 接口实现
    public abstract void OnEnterSignal(string sourceId);
    public abstract void OnExitSignal(string sourceId);
    public virtual void OnUpdate() { }
    public virtual void StopRunning() 
    {
        StateMachine?.RemoveActiveNode(this);
    }
    
    // 初始化时注入状态机引用
    public void InjectStateMachine(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
    
    // 触发输出信号（遍历连接的边）
    protected void TriggerOutputSignal(string portFieldName)
    {
        var port = GetPort(portFieldName);
        if (port == null) return;
        
        foreach (var edge in port.GetEdges())
        {
            var targetNode = edge.inputNode as BaseStateNode;
            if (targetNode == null) continue;
            
            // 根据端口名称判断是 OnEnter 还是 OnExit
            if (portFieldName.Contains("Enter"))
                targetNode.OnEnterSignal(this.GUID);
            else if (portFieldName.Contains("Exit"))
                targetNode.OnExitSignal(this.GUID);
        }
    }
}
```

**优点**:
- ✅ 利用 NGP 的连接系统，无需自己管理边
- ✅ 可视化编辑器中清晰显示控制流连接
- ✅ 支持多连接（并联逻辑）
- ✅ 类型安全（`ControlFlow` 只能连接到 `ControlFlow`）

**缺点**:
- ⚠️ 控制流和数据流混在一起，可能视觉上有点混乱
- ⚠️ 需要手动遍历边来触发信号

#### 方案 B: 完全自定义控制流系统

**设计思路**:
- 控制流不使用 NGP 的端口系统
- 在节点中添加自定义字段存储前驱/后继节点
- 手动管理控制流连接

```csharp
public abstract class BaseStateNode : BaseNode, IStateNode
{
    // 自定义控制流连接（不使用 NGP 端口）
    public List<string> enterSuccessors = new List<string>();
    public List<string> exitSuccessors = new List<string>();
    
    // 数据流仍使用 NGP 端口
    [Input("Value")] public int inputValue;
    [Output("Result")] public int outputResult;
}
```

**优点**:
- ✅ 控制流和数据流完全分离
- ✅ 更灵活的控制流处理

**缺点**:
- ❌ 失去 NGP 可视化编辑的优势
- ❌ 需要自己实现连接管理和序列化
- ❌ 增加实现复杂度

**结论**: **采用方案 A**，利用 NGP 的端口系统实现控制流。

---

## 3. 运行时架构设计

### 3.1 NGPStateMachine 职责

```csharp
public class NGPStateMachine : MonoBehaviour
{
    [SerializeField] private BaseGraph stateGraph;  // NGP 图资产
    
    private List<IStateNode> _activeNodes = new List<IStateNode>();
    private Dictionary<string, object> _blackboard = new Dictionary<string, object>();
    
    void Awake()
    {
        BuildGraph();
        StartStateMachine();
    }
    
    void Update()
    {
        // 更新所有活跃节点
        foreach (var node in _activeNodes.ToList())
        {
            node.OnUpdate();
        }
    }
    
    private void BuildGraph()
    {
        // 1. Graph 已在 OnEnable 时初始化所有节点
        // 2. 我们只需要注入 StateMachine 引用
        foreach (var node in stateGraph.nodes)
        {
            if (node is BaseStateNode stateNode)
            {
                stateNode.InjectStateMachine(this);
            }
        }
    }
    
    private void StartStateMachine()
    {
        // 查找入口节点（可通过特定标记或名称）
        var entryNode = stateGraph.nodes
            .OfType<BaseStateNode>()
            .FirstOrDefault(n => n is EntryNode);
        
        if (entryNode != null)
        {
            ProcessEnterSignal(entryNode, "Root");
        }
    }
    
    // IStateMachine 接口实现
    public void ProcessEnterSignal(IStateNode node, string sourceId)
    {
        if (!_activeNodes.Contains(node))
            _activeNodes.Add(node);
        
        node.OnEnterSignal(sourceId);
    }
    
    public void ProcessExitSignal(IStateNode node, string sourceId)
    {
        node.OnExitSignal(sourceId);
    }
    
    public void RemoveActiveNode(IStateNode node)
    {
        _activeNodes.Remove(node);
    }
    
    // 黑板操作
    public bool Get<T>(string key, out T value) { /* 实现 */ }
    public bool Set<T>(string key, T value) { /* 实现 */ }
}
```

**关键设计点**:
- ✅ **不使用 NGP 的 BaseGraphProcessor**：因为我们需要自定义的执行逻辑
- ✅ **利用 NGP 的序列化和可视化**：图结构由 NGP 管理
- ✅ **自定义运行时逻辑**：活跃节点管理、信号触发、黑板系统
- ✅ **轻量级集成**：只在 `Awake` 时注入引用，不改变 NGP 的初始化流程

### 3.2 执行流程

```
1. Unity Awake (NGPStateMachine)
   ├─ NGP 图已在 OnEnable 时初始化（BaseGraph.OnEnable）
   ├─ 所有节点已创建并调用 Initialize
   └─ 所有边已反序列化并连接

2. BuildGraph (NGPStateMachine.Awake)
   ├─ 遍历所有节点
   ├─ 为 BaseStateNode 注入 StateMachine 引用
   └─ 查找入口节点

3. StartStateMachine (NGPStateMachine.Awake)
   ├─ 找到 EntryNode
   └─ 触发 ProcessEnterSignal(entryNode, "Root")

4. 节点接收 OnEnter 信号
   ├─ StateMachine 将节点加入 _activeNodes
   ├─ 调用 node.OnEnterSignal(sourceId)
   ├─ 节点执行逻辑（可能立即或延迟完成）
   └─ 节点调用 TriggerOutputSignal("outputEnter") 发出信号

5. TriggerOutputSignal 内部逻辑
   ├─ 获取输出端口 (GetPort("outputEnter"))
   ├─ 遍历连接的所有边 (port.GetEdges())
   ├─ 对每条边的目标节点调用 OnEnterSignal
   └─ 实现并联广播

6. Update 循环 (NGPStateMachine.Update)
   ├─ 遍历 _activeNodes
   └─ 调用每个节点的 OnUpdate()

7. 节点停止运行
   ├─ 节点完成工作
   ├─ 调用 StopRunning()
   └─ StateMachine.RemoveActiveNode(this)
```

---

## 4. 数据流集成

### 4.1 数据端口的使用

NGP 的数据端口可以正常使用，与控制流端口共存：

```csharp
public class MathNode : BaseStateNode
{
    // 控制流端口（继承自 BaseStateNode）
    // inputEnter, inputExit, outputEnter, outputExit
    
    // 数据流端口（NGP 原生支持）
    [Input("A")] public float inputA;
    [Input("B")] public float inputB;
    [Output("Result")] public float outputResult;
    
    public enum Operation { Add, Subtract, Multiply, Divide }
    public Operation operation = Operation.Add;
    
    public override void OnEnterSignal(string sourceId)
    {
        // NGP 会自动通过 PullData 填充 inputA 和 inputB
        // 我们直接使用即可
        
        switch (operation)
        {
            case Operation.Add:
                outputResult = inputA + inputB;
                break;
            case Operation.Subtract:
                outputResult = inputA - inputB;
                break;
            case Operation.Multiply:
                outputResult = inputA * inputB;
                break;
            case Operation.Divide:
                outputResult = inputA / inputB;
                break;
        }
        
        // NGP 会自动通过 PushData 将 outputResult 传递给后续节点
        
        // 计算完成，发出信号并停止
        TriggerOutputSignal(nameof(outputEnter));
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        TriggerOutputSignal(nameof(outputExit));
    }
}
```

**关键点**:
- ✅ NGP 的数据传递机制（`PullData`/`PushData`）仍然有效
- ✅ 数据端口和控制流端口可以同时存在
- ✅ 在 `OnEnterSignal` 中直接使用输入数据，计算后写入输出数据

### 4.2 黑板与数据端口的选择

| 使用场景 | 推荐方式 | 原因 |
|----------|----------|------|
| 节点间直接数据传递 | NGP 数据端口 | 类型安全、可视化、自动传递 |
| 跨多层节点的全局状态 | 黑板 | 避免连线复杂、松耦合 |
| 配置型数据（关卡、玩家属性） | 黑板或 SO 节点 | 持久化、共享 |
| 临时计算结果 | NGP 数据端口 | 性能好、局部作用域 |

---

## 5. 需要实现的核心文件

### 5.1 运行时文件结构

```
Assets/LiteGameFrame/NGPStateMachine/
├── Runtime/
│   ├── Core/
│   │   ├── IStateNode.cs              # 状态节点接口
│   │   ├── IStateMachine.cs           # 状态机接口
│   │   ├── BaseStateNode.cs           # 状态节点基类
│   │   ├── NGPStateMachine.cs         # 状态机运行时
│   │   ├── ControlFlow.cs             # 控制流类型
│   │   └── StateMachineGraph.cs       # 继承 BaseGraph
│   │
│   ├── Nodes/
│   │   ├── Logic/
│   │   │   ├── EntryNode.cs           # 入口节点
│   │   │   ├── ExitNode.cs            # 出口节点
│   │   │   ├── DelayNode.cs           # 延时节点
│   │   │   ├── LogNode.cs             # 日志节点
│   │   │   └── ConditionNode.cs       # 条件节点
│   │   │
│   │   ├── Blackboard/
│   │   │   ├── GetBlackboardNode.cs   # 读取黑板
│   │   │   └── SetBlackboardNode.cs   # 写入黑板
│   │   │
│   │   └── Integration/
│   │       ├── SOStateNode.cs         # ScriptableObject 节点
│   │       └── ComponentStateNode.cs  # Component 节点
│   │
│   └── Utils/
│       └── StateMachineExtensions.cs  # 扩展方法
│
└── Editor/
    ├── StateMachineGraphWindow.cs     # 图编辑器窗口
    ├── StateMachineGraphView.cs       # 图视图
    └── Inspectors/
        └── NGPStateMachineInspector.cs # 状态机 Inspector
```

### 5.2 最小可运行集合（MVP）

**第一阶段**（核心框架）:
1. ✅ `ControlFlow.cs` - 控制流类型
2. ✅ `IStateNode.cs` - 节点接口
3. ✅ `IStateMachine.cs` - 状态机接口
4. ✅ `BaseStateNode.cs` - 节点基类（含信号触发逻辑）
5. ✅ `StateMachineGraph.cs` - 图资产类
6. ✅ `NGPStateMachine.cs` - 运行时管理器
7. ✅ `EntryNode.cs` - 入口节点（用于启动）

**第二阶段**（示例节点）:
8. ✅ `DelayNode.cs` - 延时节点（验证持续型节点）
9. ✅ `LogNode.cs` - 日志节点（验证瞬时型节点）
10. ✅ `ConditionNode.cs` - 条件节点（验证分支逻辑）

**第三阶段**（编辑器）:
11. ✅ `StateMachineGraphWindow.cs` - 自定义编辑器窗口
12. ✅ 节点创建菜单配置

---

## 6. 潜在问题与解决方案

### 6.1 问题：NGP 的 PushData/PullData 机制与信号系统冲突？

**分析**:
- NGP 的数据传递在 `BaseGraphProcessor` 中调用
- 我们不使用 `BaseGraphProcessor`，所以默认不会触发数据传递

**解决方案**:
- 在节点的 `OnEnterSignal` **前**手动调用 `inputPorts.PullDatas()`
- 在节点的 `OnEnterSignal` **后**手动调用 `outputPorts.PushDatas()`
- 或者在 `TriggerOutputSignal` 中自动处理

```csharp
protected void TriggerOutputSignal(string portFieldName)
{
    // 先推送数据端口的值
    outputPorts.PushDatas();
    
    // 再发送控制信号
    var port = GetPort(portFieldName);
    if (port == null) return;
    
    foreach (var edge in port.GetEdges())
    {
        var targetNode = edge.inputNode as BaseStateNode;
        if (targetNode == null) continue;
        
        // 目标节点拉取数据
        targetNode.inputPorts.PullDatas();
        
        // 触发信号
        if (portFieldName.Contains("Enter"))
            targetNode.OnEnterSignal(this.GUID);
        else if (portFieldName.Contains("Exit"))
            targetNode.OnExitSignal(this.GUID);
    }
}
```

### 6.2 问题：如何在编辑器中区分控制流和数据流？

**解决方案**:
- 使用 NGP 的端口着色系统
- 为 `ControlFlow` 类型定义独特的颜色（如绿色）
- 数据类型使用默认颜色

```csharp
// 在编辑器代码中
TypeColorSettings.Add(typeof(ControlFlow), Color.green);
```

### 6.3 问题：序列化兼容性

**现状**:
- NGP 使用 `[SerializeReference]` 支持多态序列化
- Unity 2021.3 LTS 完全支持

**注意事项**:
- ⚠️ 节点类改名或移动命名空间需要使用 `[MovedFrom]` 特性
- ⚠️ 字段改名会导致数据丢失，使用 `[FormerlySerializedAs]`

---

## 7. 下一步行动计划

### 7.1 立即开始实现（按顺序）

1. **创建核心接口和类型**
   - `ControlFlow.cs`
   - `IStateNode.cs`
   - `IStateMachine.cs`

2. **实现基类**
   - `BaseStateNode.cs`（关键：信号触发逻辑）
   - `StateMachineGraph.cs`

3. **实现运行时**
   - `NGPStateMachine.cs`

4. **实现入口节点**
   - `EntryNode.cs`

5. **实现测试节点**
   - `DelayNode.cs`
   - `LogNode.cs`

6. **创建测试场景**
   - 创建 `StateMachineGraph` 资产
   - 在编辑器中连接 Entry → Delay → Log
   - 在场景中添加 `NGPStateMachine` 组件
   - 运行验证

### 7.2 验证标准

- ✅ Entry 节点能正确启动状态机
- ✅ Delay 节点能正确延时后触发后续节点
- ✅ Log 节点能正确打印日志
- ✅ 活跃节点列表正确管理
- ✅ 信号能正确在节点间传递
- ✅ 节点能正确调用 `StopRunning()`

---

**总结**: 通过 **方案 A**，我们可以充分利用 NGP 的可视化编辑和序列化能力，同时保持对状态机执行逻辑的完全控制。核心策略是将控制流信号实现为特殊的数据端口，通过手动遍历边来触发目标节点的信号处理方法。

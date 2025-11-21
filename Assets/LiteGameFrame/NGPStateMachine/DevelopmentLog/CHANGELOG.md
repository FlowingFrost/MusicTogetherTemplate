# NGPStateMachine 更新日志

本文件记录了 NGPStateMachine 模块的主要开发、重构和修复历史。

---

## [v2.0.0] Component Node 与 ScriptableAsset Node 实现 (2025年11月22日)

### 重大架构变更

#### 新的图编辑器打开方式
- **从资源文件打开** → **选中物体自动加载**（类似 Timeline/Animation）
- 打开方式：`Window > State Machine Graph Editor`
- 选中带有 `NGPStateMachine` 组件的 GameObject 时自动加载其状态机图
- 支持实时切换不同物体的状态机

#### Component Node 系统
实现了场景组件绑定机制，类似 Timeline 的 ExposedReference。

**核心文件**：
- `ComponentBinding.cs`: 绑定数据结构（存储 nodeGUID + fieldName + target）
- `ComponentNode.cs`: Component 节点基类，提供 `GetBoundComponent<T>()` API
- `ComponentNodes.cs`: 内置节点实现（TransformMove, AnimatorTrigger, GameObjectSetActive）
- `ComponentNodeView.cs`: 编辑器视图，在节点上显示绑定字段

**工作原理**：
1. Component 引用存储在 `NGPStateMachine._componentBindings[]` 中
2. 节点通过 `GetComponentBinding<T>(nodeGUID, fieldName)` 获取绑定的组件
3. 编辑器通过 `SetComponentBinding()` 保存绑定关系
4. 类似 Timeline 的 PlayableAsset 机制

**内置节点**：
- `TransformMoveNode`: 移动物体（支持绝对/相对、本地/世界坐标）
- `AnimatorTriggerNode`: 控制 Animator 参数（Trigger/Bool/Int/Float）
- `GameObjectSetActiveNode`: 激活/禁用 GameObject

**使用方式**：
```csharp
// 创建自定义 Component Node
[Serializable, NodeMenuItem("My/Custom Component Node")]
public class MyNode : ComponentNode<Rigidbody>
{
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetBoundComponent(out var rb))
        {
            rb.AddForce(Vector3.up * 10f);
        }
        TriggerSignal();
        StopRunning();
    }
}
```

#### ScriptableAsset Node 系统
实现了 ScriptableObject 资源引用机制。

**核心文件**：
- `ScriptableAssetNode.cs`: ScriptableObject 节点基类，提供 `GetAsset<T>()` API
- `ScriptableAssetNodes.cs`: 示例节点（LoadGameData, LoadAsset）
- `ExampleGameData.cs`: 示例数据类型

**工作原理**：
1. ScriptableObject 引用直接序列化在节点的 `asset` 字段中
2. 节点通过 `TryGetAsset(out T asset)` 获取资源
3. 无需额外的绑定机制（与 Component Node 不同）

**内置节点**：
- `LoadGameDataNode`: 加载示例游戏数据到黑板
- `LoadScriptableAssetNode`: 加载任意 SO 资源到黑板

**使用方式**：
```csharp
// 创建自定义 ScriptableAsset Node
[Serializable, NodeMenuItem("My/Load Config")]
public class LoadConfigNode : ScriptableAssetNode<GameConfig>
{
    public override void OnEnterSignal(string sourceId)
    {
        if (TryGetAsset(out var config))
        {
            StateMachine.Set("moveSpeed", config.playerMoveSpeed);
        }
        TriggerSignal();
        StopRunning();
    }
}
```

### NGPStateMachine 扩展

**新增方法**：
```csharp
// Component Binding 管理
T GetComponentBinding<T>(string nodeGUID, string fieldName = "")
void SetComponentBinding(string nodeGUID, string fieldName, UnityEngine.Object target)
void RemoveComponentBindings(string nodeGUID)
ComponentBinding[] GetAllComponentBindings()
```

**新增字段**：
- `_componentBindings`: Component 绑定数组（序列化）

### 编辑器改进

**StateMachineGraphWindow**：
- 订阅 `Selection.selectionChanged` 事件
- 自动检测选中物体的 `NGPStateMachine` 组件
- 动态加载/卸载状态机图
- 无选中时显示提示信息

**StateMachineGraphView**：
- 添加 `SetCurrentStateMachine()` 方法
- 右键菜单新增"Refresh Bindings"功能
- 支持运行时启动/停止状态机

**StateMachineGraphInspector**：
- 更新提示信息，指导用户使用新工作流
- 添加"Open State Machine Graph Editor"快捷按钮

### 文档更新

**新增文档**：
- `ComponentNode_Usage.md`: Component Node 完整使用指南
- `ScriptableAssetNode_Usage.md`: ScriptableAsset Node 完整使用指南

**内容包括**：
- 概念介绍与工作流程
- 内置节点说明
- 自定义节点创建教程
- 最佳实践与故障排除

### 破坏性变更

⚠️ **图编辑器打开方式变更**：
- 旧方式：在 Project 中选中 StateMachineGraph → Inspector 中点击"Open"按钮
- 新方式：打开 `Window > State Machine Graph Editor` → 在 Hierarchy 中选中带有 NGPStateMachine 的物体

⚠️ **Component Node 需要重新绑定**：
- 如果之前有自定义的 Component 节点，需要适配新的 `ComponentNode<T>` 基类
- 需要在图编辑器中重新绑定场景对象

### 迁移指南

**1. 更新图编辑器打开方式**：
- 打开 `Window > State Machine Graph Editor`
- 窗口保持打开
- 通过选中 GameObject 切换编辑不同的状态机

**2. 适配 Component Node**：
```csharp
// 旧代码（假设）
public class MyNode : BaseStateNode
{
    public Transform target; // 手动赋值
}

// 新代码
public class MyNode : ComponentNode<Transform>
{
    // 不需要手动字段，通过 GetBoundComponent() 获取
}
```

**3. 重新绑定场景对象**：
- 在图编辑器中选中 Component Node
- 拖拽场景对象到节点的 "Bind [ComponentType]" 字段

### 技术细节

**序列化设计**：
- Component 引用通过 Unity 的场景引用序列化机制存储
- 与 Timeline 的 `ExposedReference<T>` 类似，但更简单
- 使用 `ComponentBinding[]` 数组存储，避免字典序列化问题

**Editor 生命周期**：
- `StateMachineGraphWindow.OnEnable()`: 订阅选中事件
- `OnSelectionChanged()`: 检测物体切换并加载图
- `OnDisable()`: 取消订阅，避免内存泄漏

**运行时安全**：
- 所有 Component 访问都通过 `TryGetBoundComponent()` 检查
- 缺少绑定时会记录清晰的错误信息
- 不会因为绑定丢失而导致崩溃

---

## 实现总结

### 1. 核心运行时框架 (6 个文件)

#### `ControlFlow.cs`
- 空结构体，用于类型标识
- 标记控制流端口，区别于数据端口

#### `IStateNode.cs`
- 节点接口定义
- 关键方法：
  - `InjectStateMachine()`: 注入状态机引用
  - `OnEnterSignal()` / `OnExitSignal()`: 信号处理
  - `OnUpdate()`: 每帧更新
  - `StopRunning()`: 主动停止运行

#### `IStateMachine.cs`
- 状态机接口定义
- 信号处理方法：`ProcessEnterSignal()` / `ProcessExitSignal()`
- 节点管理：`RemoveActiveNode()`
- 黑板操作：`Get<T>()` / `Set()` / `Find<T>()`

#### `BaseStateNode.cs`
- 节点基类，所有自定义节点继承此类
- 自动声明 2 个输入端口 + 2 个输出端口（OnEnter/OnExit）
- **核心实现**：`TriggerEnterSignal()` / `TriggerExitSignal()`
  - 遍历输出端口的所有边（`port.GetEdges()`）
  - 调用目标节点的信号方法（通过 `StateMachine.ProcessXxxSignal()`）
- 提供虚方法供子类重写

#### `StateMachineGraph.cs`
- 继承 `BaseGraph`（NGP 的图资源基类）
- 添加 `entryNodeGUID` 字段
- `FindEntryNode()`: 查找入口节点
- `ValidateGraph()`: 验证图的有效性

#### `NGPStateMachine.cs`
- MonoBehaviour 运行时管理器
- `BuildGraph()`: 
  - 遍历所有节点
  - 注入 StateMachine 引用
- `StartStateMachine()`:
  - 查找 Entry 节点
  - 发送初始信号
- `Update()`:
  - 遍历活动节点列表
  - 调用 `OnUpdate()`
- 黑板实现（`Dictionary<string, object>`）

### 2. 内置逻辑节点 (5 个文件)

#### `EntryNode.cs`
- **瞬时型节点**
- 状态机的起始点
- 行为：收到信号 → 立即触发输出 → 停止运行

#### `DelayNode.cs`
- **持续型节点**
- 等待指定时间后继续
- 参数：`duration`（延时时长）
- 输出：`elapsedTime`（已过时间）
- 行为：在 `OnUpdate()` 中计时 → 到时后触发输出 → 停止运行

#### `LogNode.cs`
- **瞬时型节点**
- 打印调试信息
- 参数：`logLevel`（Log/Warning/Error）、`message`
- 输入：`inputValue`（可选附加值）
- 行为：打印日志 → 立即触发输出 → 停止运行

#### `ConditionNode.cs`
- **瞬时型节点**
- 条件分支判断
- 参数：`expression`（条件表达式）、`variableNames`（变量列表）
- 输出：2 个独立端口（OnEnter True / OnEnter False）
- 行为：从黑板读取变量 → 使用 `ConditionEvaluator` 求值 → 根据结果选择分支 → 停止运行

#### `TimerNode.cs`
- **持久型节点**
- 定时重复触发
- 参数：`interval`（间隔）、`maxTriggers`（最大次数）
- 输出：`triggerCount`（触发次数）、`progress`（进度）
- 行为：在 `OnUpdate()` 中计时 → 每次到时发出信号（但不停止）→ 达到最大次数或收到退出信号时停止

### 3. 技术亮点

- **信号驱动的控制流**: 使用 `ControlFlow` 标记控制流端口，通过 NGP 的边系统实现信号传递。
- **节点生命周期自主管理**: 节点通过 `StopRunning()` 主动停止，支持瞬时、持续、持久三种模式。
- **双流系统**: 控制流（OnEnter/OnExit）与数据流（数据端口 + 黑板）分离。
- **扩展性设计**: 继承 `BaseStateNode` 即可创建新节点。

---

## 编辑器集成更新

### 问题诊断
用户反馈无法双击打开 StateMachineGraph 资源。原因是 NGP 需要自定义的 `EditorWindow`, `GraphView`, 和 `GraphInspector` 才能正确集成。

### 已添加的文件

- **`StateMachineGraphWindow.cs`**: 提供图编辑器窗口。
- **`StateMachineGraphView.cs`**: 自定义图视图，控制节点显示和交互。
- **`StateMachineGraphInspector.cs`**: 为 StateMachineGraph 资源添加 "Open" 按钮。
- **`BaseStateNodeView.cs`**: 自定义状态节点的视觉显示（如端口颜色）。

### 使用方法
1. **从资源打开**: 选中 StateMachineGraph 资源，在 Inspector 点击 "Open State Machine Graph" 按钮。
2. **从菜单打开**: `Window > State Machine Graph Editor`，用于快速测试。

### 总结
通过添加这几个编辑器脚本，完成了与 NGP 的完整集成，现在用户可以通过 Inspector 按钮正常打开、编辑和保存图资源。

---

## 控制流简化更新 (2025年11月21日 21:43)

### 架构变更
为简化节点逻辑，对控制流进行了统一。

- **旧设计**: 输入(OnEnter, OnExit), 输出(OnEnter, OnExit)。节点需要区分发出哪种信号。
- **新设计**: 输入(OnEnter-启动, OnExit-停止), 输出(Signal-唯一)。节点只需调用 `TriggerSignal()`，信号的语义由连接的目标端口决定。

### 核心概念
- `节点A.Signal → 节点B.OnEnter` = 启动节点B
- `节点A.Signal → 节点B.OnExit` = 停止节点B

### 代码变更
- `BaseStateNode.cs`: 将 `outputEnter` 和 `outputExit` 合并为单一的 `outputSignal`。`TriggerEnterSignal()` 和 `TriggerExitSignal()` 合并为 `TriggerSignal()`。
- **所有节点**: 更新为新的 API，`OnExitSignal()` 不再转发信号，仅用于停止自身。

### 优势
- **概念简化**: 开发者只需关心“发出信号”，而不用关心“发出何种信号”。
- **连接直观**: 绿色端口（OnEnter）代表启动，红色端口（OnExit）代表停止，一目了然。
- **代码一致**: 所有节点都使用统一的 `TriggerSignal()`。

---

## 问题修复报告 (2025年11月21日)

### 问题 1: Delay 节点的输入数据不生效
- **原因**: 节点在使用输入数据前没有调用 `inputPorts.PullDatas()` 来拉取数据。
- **解决方案**: 在所有使用数据输入端口的节点的 `OnEnterSignal()` 开头添加 `inputPorts.PullDatas()`。
- **修改文件**: `DelayNode.cs`, `LogNode.cs`。

### 问题 2: 打开 graph 有概率导致 Unity 崩溃
- **原因**: 视图初始化时机过早，且缺少空值检查，在某些情况下（如 DX11）可能导致 GPU 超时。
- **解决方案**:
  1. 增加初始化延迟：`ExecuteLater(0)` → `ExecuteLater(100)`。
  2. 添加空值检查和 `try-catch` 异常处理，增强代码健壮性。
- **修改文件**: `BaseStateNodeView.cs`, `EntryNodeView.cs`。

### 问题 3: Graph 窗口没有未保存提示
- **原因**: NGP 的 `BaseGraphWindow` 没有实现 `hasUnsavedChanges` 功能。
- **解决方案**: 在 `StateMachineGraphWindow.cs` 中自行实现：
  1. 添加 `hasUnsavedChanges` 状态标志。
  2. 订阅 `graph.onGraphChanges` 事件，在图变化时更新标志和窗口标题。
  3. 添加 Ctrl+S 保存快捷键。
- **修改文件**: `StateMachineGraphWindow.cs`。

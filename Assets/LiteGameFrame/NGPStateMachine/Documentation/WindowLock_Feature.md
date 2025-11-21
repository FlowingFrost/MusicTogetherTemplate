# State Machine Graph Window - Lock Feature

## 概述

状态机图编辑器窗口 (`StateMachineGraphWindow`) 已实现了类似于 Unity Inspector 和 Timeline 窗口的锁定功能。当窗口被锁定时，窗口内容不会随着 Hierarchy 中选择的变化而自动更新。

## 功能特性

### 1. 锁定按钮
- **位置**: 窗口工具栏右侧
- **图标**: 使用 Unity 内置的锁定图标
  - 未锁定: `"IN LockButton"` 
  - 已锁定: `"IN LockButton on"`
- **提示文本**: "Lock window to prevent auto-loading graph on selection change"

### 2. 锁定行为
- **未锁定状态**: 
  - 当在 Hierarchy 中选择带有 `NGPStateMachine` 组件的 GameObject 时，窗口会自动加载并显示对应的状态机图
  - 如果选择的对象没有状态机组件，窗口会清空显示

- **锁定状态**:
  - 窗口内容保持不变，不受 Hierarchy 选择变化的影响
  - 可以自由地在场景中选择其他对象，而不会改变当前编辑的状态机图
  - 适用于需要同时查看多个对象但保持编辑某个特定状态机的场景

## 实现细节

### 核心字段
```csharp
private bool _isLocked;           // 锁定状态标志
private Button _lockButton;       // 锁定按钮 UI 元素
```

### 关键方法

#### 1. CreateToolbar()
在 `OnEnable()` 中调用，创建包含锁定按钮的工具栏：
- 使用 `UnityEditor.UIElements.Toolbar` 创建工具栏
- 添加弹性空间使按钮靠右对齐
- 创建 30x20 像素的锁定按钮
- 将工具栏插入到 `rootView` 的顶部（索引 0）

#### 2. OnSelectionChanged()
响应 `Selection.selectionChanged` 事件：
```csharp
private void OnSelectionChanged()
{
    // 如果窗口被锁定，忽略选择变化
    if (_isLocked)
        return;
    
    // ... 其他选择处理逻辑
}
```
这是锁定功能的核心：通过在方法开始处检查 `_isLocked` 标志，直接返回以阻止任何图加载行为。

#### 3. ToggleLock()
切换锁定状态：
- 反转 `_isLocked` 布尔值
- 更新按钮图标
- 在控制台输出锁定状态日志

#### 4. UpdateLockButtonIcon()
更新按钮的视觉状态：
- 根据 `_isLocked` 状态选择对应的 Unity 内置图标
- 将图标设置为按钮的背景图片

## 使用场景

### 场景 1: 对比多个状态机
1. 打开状态机图编辑器窗口
2. 选择第一个带有 `NGPStateMachine` 的对象
3. 点击锁定按钮
4. 选择其他对象进行对比或编辑
5. 当前窗口仍显示第一个对象的状态机图

### 场景 2: 编辑状态机的同时调整场景对象
1. 锁定窗口到特定状态机
2. 在 Hierarchy 中选择和调整其他 GameObject
3. 无需担心窗口内容被意外切换

### 场景 3: 查看运行时状态
1. 在 Play Mode 前锁定到特定状态机
2. 进入 Play Mode 后可以自由选择其他对象进行调试
3. 状态机图窗口保持对原始状态机的显示

## 技术考虑

### 生命周期管理
- `OnEnable()`: 订阅 `Selection.selectionChanged` 事件并创建工具栏
- `OnDisable()`: 取消��件订阅
- `OnDestroy()`: 清理事件订阅和图视图

### 与现有系统的集成
- 不影响图的保存和加载
- 不改变序列化数据
- 完全兼容现有的 `BaseGraphWindow` 基类架构
- 不干扰 `StateMachineGraphView` 的 Component Binding 功能

### 性能影响
- 锁定功能的性能开销极小
- 仅在选择变化时进行一次布尔值检查
- 不涉及任何额外的序列化或反序列化操作

## 未来扩展建议

### 1. 持久化锁定状态
考虑在编辑器会话间保存锁定状态：
```csharp
private const string LOCK_PREF_KEY = "StateMachineGraphWindow.IsLocked";

private void LoadLockState()
{
    _isLocked = EditorPrefs.GetBool(LOCK_PREF_KEY, false);
}

private void SaveLockState()
{
    EditorPrefs.SetBool(LOCK_PREF_KEY, _isLocked);
}
```

### 2. 键盘快捷键
添加快捷键支持（例如 Ctrl+L）：
```csharp
private void OnGUI()
{
    Event e = Event.current;
    if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.L)
    {
        ToggleLock();
        e.Use();
    }
}
```

### 3. 多窗口支持
允许同时打开多个状态机图编辑器窗口，每个窗口独立锁定到不同的状态机。

## 参考实现

- **Unity Inspector Lock**: Unity 原生 Inspector 窗口的锁定功能
- **Unity Timeline Lock**: Timeline 窗口的锁定功能
- **Unity Animation Window**: Animation 窗口的绑定和锁定机制

## 更新日志

- **2025-01-22**: 初始实现
  - 添加锁定按钮到工具栏
  - 实现基本的锁定/解锁切换
  - 使用 Unity 内置图标
  - 添加工具提示文本
  - 在控制台输出锁定状态日志

## 相关文件

- `StateMachineGraphWindow.cs`: 主窗口实现
- `StateMachineGraphView.cs`: 图视图实现
- `NGPStateMachine.cs`: 状态机运行时组件


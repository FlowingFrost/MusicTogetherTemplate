# NGPStateMachine 文档索引

> **基于 Node Graph Processor 的图结构状态机系统**  
> Unity 2021.3.x LTS | 版本 v2.0

---

## 📚 文档导航

### 🚀 快速开始
新用户从这里开始，快速了解并使用状态机系统。

- **[README.md](README.md)** - 项目概述与快速上手
- **[快速开始教程](Documentation/QuickStart.md)** - 5分钟创建你的第一个状态机

---

### 📖 用户指南
了解如何使用不同类型的节点和高级特性。

#### 节点类型指南
- **[ScriptStateNode 使用指南](Documentation/ScriptStateNode_Usage.md)** - 将状态机连接到场景脚本
- **[ComponentNode 使用指南](Documentation/ComponentNode_Usage.md)** - 操作场景中的组件
- **[ScriptableAssetNode 使用指南](Documentation/ScriptableAssetNode_Usage.md)** - 使用数据资源驱动逻辑

#### 高级特性
- **[编辑器窗口锁定功能](Documentation/WindowLock_Feature.md)** - 类似 Timeline 的编辑体验

---

### 🔧 开发者指南
面向开发者的扩展与自定义指南。

- **[创建自定义 ScriptStateNode](DeveloperGuide/CreateCustomScriptNode.md)** - 扩展脚本状态节点
- **[创建自定义 ComponentNode](DeveloperGuide/CreateCustomComponentNode.md)** - 创建自己的组件节点
- **[创建自定义 ScriptableAssetNode](DeveloperGuide/CreateCustomAssetNode.md)** - 创建数据驱动节点
- **[扩展编辑器视图](DeveloperGuide/ExtendEditorView.md)** - 自定义节点的编辑器显示

---

### 📋 API 参考
核心类和接口的详细说明。

- **[IStateNode 接口](DeveloperGuide/API_IStateNode.md)** - 状态节点核心接口
- **[IStateMachine 接口](DeveloperGuide/API_IStateMachine.md)** - 状态机接口
- **[BaseStateNode 基类](DeveloperGuide/API_BaseStateNode.md)** - 节点基类
- **[控制流与数据流](DeveloperGuide/ControlFlow_DataFlow.md)** - 双流系统详解

---

### 🔬 技术文档
深入了解系统架构和设计理念。

- **[技术规格文档](DevelopmentLog/NGP_StateMachine_TechSpec.md)** - 完整的技术设计文档
- **[v2.0 实现总结](DevelopmentLog/v2.0_Implementation_Summary.md)** - 版本 2.0 的重大变更

---

### 📝 更新日志
版本历史和变更记录。

- **[CHANGELOG.md](DevelopmentLog/CHANGELOG.md)** - 完整的版本更新历史

---

## 🎯 按使用场景查找

### 我想...

| 场景 | 推荐文档 |
|------|----------|
| 🆕 **第一次使用状态机** | [README.md](README.md) → [快速开始教程](Documentation/QuickStart.md) |
| 🎮 **让状态机控制场景物体** | [ComponentNode 使用指南](Documentation/ComponentNode_Usage.md) |
| 📜 **用自己的脚本实现状态逻辑** | [ScriptStateNode 使用指南](Documentation/ScriptStateNode_Usage.md) |
| 📦 **用数据文件驱动游戏逻辑** | [ScriptableAssetNode 使用指南](Documentation/ScriptableAssetNode_Usage.md) |
| 🔨 **创建自定义节点** | [开发者指南](DeveloperGuide/) |
| 🐛 **遇到问题需要调试** | [技术规格文档](DevelopmentLog/NGP_StateMachine_TechSpec.md) |
| 📚 **了解系统架构** | [技术规格文档](DevelopmentLog/NGP_StateMachine_TechSpec.md) |

---

## 🆘 常见问题

<details>
<summary><strong>Q: 状态机图编辑器在哪里打开？</strong></summary>

**A:** 菜单 `Window > State Machine Graph Editor`，然后在 Hierarchy 中选择带有 `NGPStateMachine` 组件的物体，图会自动加载。
</details>

<details>
<summary><strong>Q: ComponentNode 和 ScriptStateNode 有什么区别？</strong></summary>

**A:** 
- **ComponentNode**: 节点本身包含逻辑，直接操作绑定的组件（如移动物体、触发动画）
- **ScriptStateNode**: 节点是代理，将生命周期转发给场景中的脚本，逻辑写在 MonoBehaviour 中
</details>

<details>
<summary><strong>Q: 如何在节点之间传递数据？</strong></summary>

**A:** 
1. **数据流端口**: 直接连接节点的输入/输出数据端口
2. **黑板系统**: 使用 `StateMachine.Set<T>(key, value)` 和 `Get<T>(key)` 共享全局数据
</details>

<details>
<summary><strong>Q: 节点什么时候停止运行？</strong></summary>

**A:** 
- 节点主动调用 `StopRunning()` 请求停止
- 或收到 `OnExit` 信号时被强制停止
- 发出输出信号 ≠ 停止运行，节点可以在输出信号后继续运行
</details>

---

## 📞 反馈与支持

如果你发现文档中的问题或有改进建议，请提交 Issue 或 Pull Request。

---

**最后更新**: 2025年11月22日  
**维护者**: FlowingFrost

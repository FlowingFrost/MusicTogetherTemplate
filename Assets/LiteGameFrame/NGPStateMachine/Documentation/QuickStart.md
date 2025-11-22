# 快速开始教程

> **5分钟创建你的第一个状态机**

本教程将引导你创建一个简单的状态机，让一个物体在场景中移动。

---

## 第一步：创建状态机图资源

1. 在 Unity Project 窗口中右键点击
2. 选择 `Create > State Machine > State Machine Graph`
3. 命名为 `MyFirstStateMachine`

---

## 第二步：在场景中设置状态机

1. 创建一个空物体，命名为 `StateMachineRunner`
2. 添加 `NGPStateMachine` 组件
3. 将 `MyFirstStateMachine` 资源拖到组件的 `State Graph` 字段

---

## 第三步：打开图编辑器

1. 打开菜单：`Window > State Machine Graph Editor`
2. 在 Hierarchy 中选中 `StateMachineRunner` 物体
3. 图编辑器会自动加载状态机图

---

## 第四步：添加节点

### 方式一：使用内置 ComponentNode（推荐新手）

1. 在图编辑器中右键，选择 `State Machine > Component > Transform Move`
2. 将 Entry 节点的 `Signal` 端口连接到 TransformMove 节点的 `OnEnter` 端口

3. 创建一个 Cube 物体在场景中
4. 选中 TransformMove 节点，在节点上找到 `🔗 Transform` 字段
5. 从 Hierarchy 拖拽 Cube 到该字段

6. 配置移动参数：
   - **Target Position**: (5, 0, 0)
   - **Duration**: 2.0
   - **Is Local**: 勾选

7. 保存（Ctrl+S）并运行，Cube 会移动到目标位置！

### 方式二：使用 ScriptStateNode（适合有脚本基础）

1. 创建一个新脚本 `MyMoveState.cs`：

```csharp
using UnityEngine;
using LiteGameFrame.NGPStateMachine;

public class MyMoveState : MonoBehaviour, IStateHandler
{
    private float timer = 0f;
    
    public void OnStateEnter(StateContext context)
    {
        Debug.Log("开始移动");
        timer = 0f;
    }
    
    public void OnStateUpdate(StateContext context)
    {
        timer += Time.deltaTime;
        transform.position += Vector3.right * Time.deltaTime;
        
        if (timer >= 2f)
        {
            context.RequestComplete(); // 完成状态
        }
    }
    
    public void OnStateExit(StateContext context)
    {
        Debug.Log("移动结束");
    }
}
```

2. 将脚本添加到场景中的 Cube 上
3. 在图编辑器中右键，选择 `State Machine > Script State Node`
4. 将 Entry 节点的 `Signal` 连接到 ScriptStateNode 的 `OnEnter`
5. 在 ScriptStateNode 上找到 `🔗 Script` 字段，拖拽 Cube 到该字段
6. 保存并运行！

---

## 第五步：添加更多状态

### 创建状态链

1. 添加第二个 TransformMove 节点，让 Cube 移动回原点
2. 将第一个节点的 `Signal` 连接到第二个节点的 `OnEnter`
3. 配置第二个节点：
   - **Target Position**: (0, 0, 0)
   - **Duration**: 2.0

4. 运行，Cube 会来回移动！

### 创建循环

1. 将第二个节点的 `Signal` 连接回第一个节点的 `OnEnter`
2. 运行，Cube 会无限循环移动！

---

## 常见操作

### 编辑器快捷键
- **右键**: 打开节点创建菜单
- **Ctrl+S**: 保存图
- **Delete**: 删除选中节点
- **F**: 聚焦到选中节点

### 调试技巧
1. 在 Play 模式下，活跃的节点会高亮显示
2. 使用 `Debug.Log()` 在节点中输出信息
3. 在 Inspector 中查看 NGPStateMachine 组件的运行时状态

---

## 下一步

🎉 恭喜！你已经创建了第一个状态机。接下来可以：

- 📖 学习 [ScriptStateNode 使用指南](ScriptStateNode_Usage.md) - 更灵活的状态逻辑
- 🎮 学习 [ComponentNode 使用指南](ComponentNode_Usage.md) - 控制动画、粒子等
- 📦 学习 [ScriptableAssetNode 使用指南](ScriptableAssetNode_Usage.md) - 数据驱动设计
- 🔨 查看 [开发者指南](../DeveloperGuide/) - 创建自定义节点

---

## 故障排除

<details>
<summary><strong>图编辑器打开了但是是空白的</strong></summary>

**解决方案**：
1. 确保 Hierarchy 中选中了带有 `NGPStateMachine` 组件的物体
2. 确保该组件的 `State Graph` 字段已赋值
3. 尝试重新选择物体
</details>

<details>
<summary><strong>运行时节点没有执行</strong></summary>

**检查清单**：
1. Entry 节点是否连接到你的节点？
2. 连接是否正确（Signal → OnEnter）？
3. ComponentNode 是否绑定了场景对象？
4. ScriptStateNode 是否绑定了脚本？
5. 查看 Console 是否有错误信息
</details>

<details>
<summary><strong>ComponentNode 的绑定字段不显示</strong></summary>

**解决方案**：
1. 确保选中了带有 NGPStateMachine 组件的物体
2. 确保图已经保存（Ctrl+S）
3. 尝试重新打开图编辑器
</details>

---

**需要更多帮助？** 查看 [INDEX.md](../INDEX.md) 获取完整文档列表。

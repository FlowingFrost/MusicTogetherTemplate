# ComponentNode 绑定功能快速测试

## 测试目标
验证 ComponentNode 的绑定字段是否正确显示和工作

## 测试步骤

### 步骤 1：准备测试环境

1. 打开 Unity 编辑器
2. 创建新场景或打开测试场景
3. 在 Hierarchy 中创建以下对象：
   - 空物体 `TestStateMachine`（添加 NGPStateMachine 组件）
   - Cube `MovingCube`（位置 0,0,0）

### 步骤 2：创建状态机图

1. Project 窗口右键 → `Create > State Machine > State Machine Graph`
2. 命名为 `ComponentTest`
3. 将图资源拖到 `TestStateMachine` 的 `State Graph` 字段

### 步骤 3：打开图编辑器

1. 菜单：`Window > State Machine Graph Editor`
2. 在 Hierarchy 中选中 `TestStateMachine`
3. ✅ 验证：图编辑器自动加载，标题显示 "TestStateMachine - ComponentTest"

### 步骤 4：添加 Component Node

1. 在图编辑器中右键
2. `State Machine > Entry` （添加入口）
3. `State Machine > Component > Transform Move`
4. 连接：Entry.Signal → TransformMove.OnEnter

### 步骤 5：验证绑定字段显示

1. 选中 TransformMove 节点
2. ✅ **关键验证**：节点上应该显示 `🔗 Transform` 绑定字段
3. ✅ 字段类型应该是 ObjectField
4. ✅ 可以接受场景中的对象

### 步骤 6：绑定场景对象

1. 从 Hierarchy 拖拽 `MovingCube` 到 `🔗 Transform` 字段
2. ✅ 验证：字段显示 "MovingCube"
3. ✅ 验证：Console 显示 "[ComponentNode] Bound Transform: MovingCube"
4. ✅ 验证：场景被标记为已修改（需要保存）

### 步骤 7：配置节点参数

在 TransformMove 节点设置：
- `Target Position` = (5, 0, 0)
- `Move Mode` = Absolute
- `Duration` = 2.0
- `Use Local Space` = false

### 步骤 8：保存并运行

1. 保存图：Ctrl+S
2. 保存场景：Ctrl+S
3. 进入 Play 模式
4. ✅ 验证：Cube 在 2 秒内移动到 (5,0,0)
5. ✅ 验证：Console 显示：
   ```
   [TransformMoveNode] Started moving MovingCube to (5.00, 0.00, 0.00) over 2s
   [TransformMoveNode] Move completed
   ```

### 步骤 9：测试绑定持久化

1. 退出 Play 模式
2. 关闭图编辑器
3. 关闭 Unity
4. 重新打开项目
5. 打开图编辑器，选中 TestStateMachine
6. ✅ 验证：TransformMove 节点的绑定字段仍然显示 "MovingCube"
7. ✅ 验证：运行时仍然正常工作

### 步骤 10：测试多个 Component Node

1. 添加更多节点：
   - `State Machine > Component > Animator Trigger`
   - `State Machine > Component > GameObject SetActive`
2. ✅ 验证：每个节点都显示对应的绑定字段
3. ✅ 验证：可以绑定不同类型的组件

## 预期结果

### ✅ 成功标志

1. **绑定字段显示**：
   - ComponentNode 自动显示 `🔗 [ComponentType]` 字段
   - 字段出现在节点的 controls 区域
   - 样式清晰，易于识别

2. **绑定功能**：
   - 可以拖拽场景对象到字段
   - 绑定后字段显示对象名称
   - Console 有绑定成功的日志

3. **运行时行为**：
   - 节点能正确访问绑定的组件
   - 无 "Component binding is missing" 错误
   - 功能正常执行

4. **持久化**：
   - 绑定信息保存在场景中
   - 重新打开项目后绑定仍然存在
   - 不影响 Prefab

### ❌ 失败标志

1. **绑定字段不显示**：
   - 可能原因：
     - `BaseStateNodeView.AddComponentBindingFieldIfNeeded()` 未被调用
     - `GetComponentTypeIfComponentNode()` 返回 null
     - GraphView 不是 StateMachineGraphView
   - 解决方法：检查 Console 是否有警告，确保图编辑器正确加载

2. **无法拖拽对象**：
   - 可能原因：
     - `allowSceneObjects = true` 未设置
     - 对象类型不匹配
   - 解决方法：检查 ObjectField 的 `objectType` 属性

3. **运行时报错**：
   - 可能原因：
     - 绑定未保存
     - 场景引用丢失
   - 解决方法：确保保存场景，检查绑定数组

## 调试技巧

### 查看绑定数据

1. 选中 `TestStateMachine` GameObject
2. 在 Inspector 中查看 `NGPStateMachine` 组件
3. 展开 `Component Bindings` 数组
4. ✅ 应该看到绑定条目，包含：
   - `Node GUID`
   - `Field Name` = "target"
   - `Target` = MovingCube (Transform)

### Console 日志

正常日志输出应该包括：
```
[StateMachineGraphWindow] Loaded graph from TestStateMachine
[ComponentNode] Bound Transform: MovingCube
[EntryNode] State machine started (source: Root)
[TransformMoveNode] Started moving MovingCube to (5.00, 0.00, 0.00) over 2s
[TransformMoveNode] Move completed
```

### 常见警告

如果看到这些警告，功能仍然正常：
```
[BaseStateNodeView] GraphView is not StateMachineGraphView
[BaseStateNodeView] No StateMachine selected
```
原因：在未选中物体时打开图编辑器

## 性能检查

- 绑定字段添加不应导致明显延迟
- 延迟 100ms 后添加字段是正常的（等待端口创建）
- 节点拖拽和连接应该流畅

## 成功标准

- [ ] 绑定字段自动显示
- [ ] 可以拖拽场景对象
- [ ] 绑定信息正确保存
- [ ] 运行时正确访问组件
- [ ] 无错误日志
- [ ] 重启后绑定仍然有效

---

**测试完成日期**：___________  
**测试结果**：通过 / 失败  
**备注**：___________


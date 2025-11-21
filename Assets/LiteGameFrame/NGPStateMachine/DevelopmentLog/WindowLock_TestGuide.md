# 状态机图编辑器窗口 - 锁定功能测试指南

## 测试目标
验证状态机图编辑器窗口的锁定功能是否正常工作，确保锁定状态能够正确阻止窗口内容随选择变化而更新。

## 前置条件
1. Unity 编辑器已打开 LightGameFrame 项目
2. 场景中至少有 2 个带有 `NGPStateMachine` 组件的 GameObject
3. 每个 `NGPStateMachine` 组件都绑定了有效的 `StateMachineGraph` 资产

## 测试环境准备

### 创建测试场景
```
Scene Hierarchy:
├── TestStateMachine_A (带有 NGPStateMachine 组件)
│   └── StateGraph: TestGraphA
├── TestStateMachine_B (带有 NGPStateMachine 组件)
│   └── StateGraph: TestGraphB
└── TestStateMachine_C (带有 NGPStateMachine 组件)
    └── StateGraph: TestGraphC
```

### 创建测试状态机图
1. 在 Project 窗口中创建 3 个不同的 StateMachineGraph 资产
2. 为每个图添加不同的节点和连接，以便区分
3. 分别命名为 `TestGraphA`, `TestGraphB`, `TestGraphC`

## 测试用例

### 测试用例 1: 基本锁定/解锁功能

#### 步骤
1. 打开状态机图编辑器窗口 (`Window > State Machine Graph Editor`)
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 验证窗口标题显示为 "TestStateMachine_A - TestGraphA"
4. 验证窗口显示 TestGraphA 的节点和连接
5. 点击工具栏右侧的锁定按钮
6. 验证按钮图标变为"锁定"状态（高亮）
7. 验证控制台输出: `[StateMachineGraphWindow] Window locked`
8. 再次点击锁定按钮
9. 验证按钮图标变回"解锁"状态
10. 验证控制台输出: `[StateMachineGraphWindow] Window unlocked`

#### 预期结果
- ✅ 锁定按钮能够正常切换状态
- ✅ 按钮图标正确反映当前锁定状态
- ✅ 控制台输出正确的日志信息

---

### 测试用例 2: 锁定状态下的选择变化

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 验证窗口显示 TestGraphA
4. 点击锁定按钮（锁定窗口）
5. 在 Hierarchy 中选择 `TestStateMachine_B`
6. 验证窗口**仍然**显示 TestGraphA（内容未变）
7. 验证窗口标题**仍然**显示 "TestStateMachine_A - TestGraphA"
8. 在 Hierarchy 中选择 `TestStateMachine_C`
9. 验证窗口**仍然**显示 TestGraphA（内容未变）

#### 预期结果
- ✅ 锁定状态下，窗口内容不随选择变化而改变
- ✅ 窗口标题保持不变
- ✅ 图视图保持不变

---

### 测试用例 3: 解锁后的选择响应

#### 步骤
1. 继续上一个测试用例的状态（窗口锁定，当前显示 TestGraphA，选中 TestStateMachine_C）
2. 点击锁定按钮（解锁窗口）
3. 验证窗口**立即**更新为显示 TestGraphC
4. 验证窗口标题更新为 "TestStateMachine_C - TestGraphC"
5. 在 Hierarchy 中选择 `TestStateMachine_B`
6. 验证窗口更新为显示 TestGraphB
7. 验证窗口标题更新为 "TestStateMachine_B - TestGraphB"

#### 预期结果
- ✅ 解锁后，窗口立即响应当前选择
- ✅ 后续选择变化能够正常触发窗口更新

---

### 测试用例 4: 锁定状态下选择无状态机对象

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 验证窗口显示 TestGraphA
4. 点击锁定按钮（锁定窗口）
5. 在 Hierarchy 中选择一个**没有** `NGPStateMachine` 组件的 GameObject（例如 Main Camera）
6. 验证窗口**仍然**显示 TestGraphA
7. 在 Hierarchy 中选择多个对象
8. 验证窗口**仍然**显示 TestGraphA

#### 预期结果
- ✅ 锁定状态下，即使选择无状态机对象，窗口内容也不变
- ✅ 多选对象时窗口内容保持不变

---

### 测试用例 5: 锁定状态下编辑图

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 点击锁定按钮（锁定窗口）
4. 在图视图中添加新节点
5. 连接节点
6. 验证修改能够正常应用
7. 在 Hierarchy 中选择 `TestStateMachine_B`
8. 验证窗口仍然显示 TestGraphA 及刚才的修改
9. 解锁窗口
10. 验证窗口切换到 TestGraphB
11. 再次选择 `TestStateMachine_A`
12. 验证之前的修改已保存

#### 预期结果
- ✅ 锁定状态下仍可正常编辑图
- ✅ 编辑操作正常保存
- ✅ 锁定不影响图的保存和加载

---

### 测试用例 6: 按钮悬停提示

#### 步骤
1. 打开状态机图编辑器窗口
2. 将鼠标悬停在锁定按钮上
3. 验证显示提示文本

#### 预期结果
- ✅ 悬停时显示: "Lock window to prevent auto-loading graph on selection change"
- ✅ 提示文本清晰易懂

---

### 测试用例 7: 窗口重新打开后的状态

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 点击锁定按钮
4. 关闭状态机图编辑器窗口
5. 重新打开状态机图编辑器窗口 (`Window > State Machine Graph Editor`)
6. 验证锁定状态

#### 预期结果
- ✅ 当前实现：重新打开后锁定状态**不会**保持（默认为解锁状态）
- ⚠️ 未来改进：可以考虑使用 `EditorPrefs` 持久化锁定状态

---

### 测试用例 8: Play Mode 切换

#### 步骤
1. 在 Edit Mode 下打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 点击锁定按钮
4. 进入 Play Mode
5. 验证窗口状态
6. 在 Hierarchy 中选择其他对象
7. 验证窗口内容
8. 退出 Play Mode
9. 验证窗口状态

#### 预期结果
- ✅ 进入 Play Mode 时锁定状态保持
- ✅ Play Mode 下锁定功能正常工作
- ✅ 退出 Play Mode 后状态保持

---

### 测试用例 9: 多窗口锁定（如果支持）

#### 步骤
1. 打开第一个状态机图编辑器窗口（Window 1）
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 锁定 Window 1
4. 打开第二个状态机图编辑器窗口（Window 2）
5. 在 Hierarchy 中选择 `TestStateMachine_B`
6. 验证 Window 1 仍显示 TestGraphA
7. 验证 Window 2 显示 TestGraphB
8. 锁定 Window 2
9. 在 Hierarchy 中选择 `TestStateMachine_C`
10. 验证两个窗口都保持各自的内容

#### 预期结果
- ✅ 每个窗口独立管理自己的锁定状态
- ✅ 多窗口之间互不干扰

---

## 性能测试

### 测试用例 10: 快速切换选择

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中快速连续选择 10 个不同的带状态机的对象（解锁状态）
3. 观察窗口更新的流畅度
4. 锁定窗口
5. 再次快速连续选择 10 个不同的对象
6. 观察性能和响应速度

#### 预期结果
- ✅ 解锁状态下窗口能跟上选择变化
- ✅ 锁定状态下性能更好（因为跳过了更新逻辑）
- ✅ 无明显卡顿或延迟

---

## 边界情况测试

### 测试用例 11: 锁定状态下删除当前对象

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 锁定窗口
4. 在 Hierarchy 中删除 `TestStateMachine_A`
5. 观察窗口行为

#### 预期结果
- ✅ 窗口不会崩溃
- ⚠️ 可能显示空白或错误信息（取决于具体实现）

---

### 测试用例 12: 锁定状态下修改绑定的图资产

#### 步骤
1. 打开状态机图编辑器窗口
2. 在 Hierarchy 中选择 `TestStateMachine_A`
3. 锁定窗口
4. 在 Inspector 中将 `TestStateMachine_A` 的 StateGraph 字段更改为另一个图
5. 观察窗口行为

#### 预期结果
- ✅ 窗口可能仍显示原始图的引用
- ⚠️ 行为取决于图引用的存储方式

---

## 回归测试检查清单

在实现锁定功能后，确保以下现有功能未受影响：

- [ ] 正常的图加载功能
- [ ] 节点的创建、删除、移动
- [ ] 节点的连接和断开
- [ ] 节点属性的编辑
- [ ] Component Binding 功能
- [ ] 图的保存和自动保存
- [ ] Inspector 中的属性同步
- [ ] 图的撤销/重做 (Undo/Redo)

---

## 已知问题和限制

1. **锁定状态不持久化**
   - 关闭并重新打开窗口后，锁定状态会重置为解锁
   - 建议：使用 `EditorPrefs` 保存状态

2. **锁定后对象被删除的处理**
   - 如果锁定的对象被删除，窗口可能显示已销毁对象的引用
   - 建议：添加对象有效性检查

3. **无键盘快捷键**
   - 目前只能通过点击按钮来切换锁定状态
   - 建议：添加快捷键支持（如 Ctrl+L）

---

## 测试通过标准

- ✅ 所有功能测试用例通过
- ✅ 所有性能测试无明显问题
- ✅ 所有边界情况有合理的处理
- ✅ 回归测试全部通过
- ✅ 无编译错误或警告
- ✅ 无控制台错误或异常

---

## 测试报告模板

```
测试日期: ____年__月__日
测试人员: __________
Unity 版本: __________

测试用例编号 | 测试结果 | 备注
----------|---------|------
用例1      | ☐ Pass ☐ Fail |
用例2      | ☐ Pass ☐ Fail |
用例3      | ☐ Pass ☐ Fail |
...

发现的问题:
1. 
2. 
3. 

总体评价:
☐ 可以发布
☐ 需要修复后发布
☐ 需要重新设计
```

---

## 参考资料

- [WindowLock_Feature.md](WindowLock_Feature.md) - 锁定功能的详细文档
- Unity Inspector Lock - Unity 官方文档
- Unity Timeline Window - Unity 官方文档


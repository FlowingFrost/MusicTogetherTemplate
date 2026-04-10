# DancingBall Inspector CRUD 改进方案

> 目的：评估当前项目对 CRUD 操作界面与可视化列表的设计需求，并给出可执行的功能性改进方案。该文档提供“原文参考”与定位提示，便于下一位 AI 直接进入修改点。

## 结论概览
当前 `InspectorWindow` 仅能在选中 Road/Block 时展示少量字段与按钮，**缺少 Road 与 Block 的可视化列表、创建/删除/批量修改能力**，且 `InspectorWindow` 没有将 UI 事件绑定到数据层操作（仅绑定了 Selection 事件）。因此需要新增以下核心功能：

1. **Road CRUD 与可视化列表**（基于 `SceneData.roadDataList`）
2. **Block CRUD 与可视化列表**（基于 `RoadData.BlockCount` 与 `RoadData.blockDisplacementDataList`）
3. **Block 位移规则数据创建/删除**（`ClassicBlockDisplacementData`）
4. **Inspector 面板布局升级**（列表 + 详情面板 + 操作工具栏）
5. **EditorCenter/SceneData 扩展**（新增 CRUD 的统一入口与事件回调）

---

## 现状要点与问题清单

### 1) InspectorWindow 仅绑定 Selection，缺少 CRUD 行为
**原文参考**：`Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`

```csharp
EditorCenter.OnRoadSelectionChanged += OnRoadSelected;
EditorCenter.OnBlockSelectionChanged += OnBlockSelected;
...
private void MapRebuildRoadsRequested() { if (!VerifyMap()) return; EditorCenter.MapRebuildRoadsRequested(); }
```

问题：
- 仅在选中 Road/Block 时更新 UI。
- `InspectorWindowManager` 中定义的按钮回调（如 `RoadModifyNoteBeginRequested`、`RoadRefreshBlocksRequested`）**未被绑定**。
- UI 没有 Road/Block 的列表入口，无法选择/创建/删除。

### 2) Road UI 只做“字段修改”，没有列表与 CRUD
**原文参考**：`Assets/MusicTogether/DancingBall/UI/InspectorWindow/RoadEditorPanel.uxml`

```xml
<ui:Button name="road-refresh-road-blocks" text="重建Block列表..." />
<ui:IntegerField name="road-note-begin" label="Note 起始序号" />
<ui:Button name="road-modify-note-begin" text="更改Note起始序号..." />
<ui:TextField name="road-target-data-name" label="目标Road数据名称" />
```

问题：
- 没有 Road 列表展示（`SceneData.roadDataList` 无 UI 映射）。
- 无新增/删除/复制 Road 操作。
- Road 选择完全依赖 `EditorCenter.SelectedRoadIndex` 的外部设置。

### 3) Block 没有数据创建/修改逻辑
**原文参考**：`Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`

```csharp
if (displacementData == null)
{
    
}
else switch (displacementData)
{
    case ClassicBlockDisplacementData classicData:
        _windowManager.SetClassicBlockTurnType(classicData.turnType);
        _windowManager.SetClassicBlockDisplacementType(classicData.displacementType);
        break;
}
```

问题：
- `displacementData == null` 时没有创建逻辑。
- Block “列表/导航/批量创建”缺失。
- 无 UI 展示 Block 目前是否有位移数据、是否为 tap block 等关键状态。

### 4) 数据层已有操作，但未被 UI/EditorCenter 统一调用
**原文参考**：
- `Assets/MusicTogether/DancingBall/Scene/ClassicRoad.cs`（`ModifyNoteBeginIndex` / `RecoverBlocks` / `ModifyDisplacementData`）
- `Assets/MusicTogether/DancingBall/Scene/ClassicMap.cs`（`RecoverRoads` / `RefreshAllRoads`）

这些操作已存在，但 Inspector 无统一入口调用，导致“可用但不可用”。

---

## 建议的功能性设计方案

### A. Road 列表 + CRUD（重点）
**目标**：在 Inspector 中加入 Road 列表与基础 CRUD 入口，支持编辑 `SceneData.roadDataList`。

**功能设计**
- **Road 列表区（ListView）**
  - 显示字段：`roadName`、`targetSegmentIndex`、`noteBeginIndex`、`noteEndIndex`、`BlockCount`
  - 支持搜索 / 排序 / 过滤（如按 segment 或 name）
  - 选择后同步 `EditorCenter.JumpTo(roadIndex)`

- **CRUD 操作**
  - 新增 Road：提供“新建 Road”表单（name/segment/note range），调用 `SceneData.Set_RoadData` + `Map.RecoverRoads`
  - 删除 Road：从 `SceneData.roadDataList` 移除，并 `Map.RecoverRoads`
  - 复制 Road：复制 `RoadData`，生成新 name
  - 批量重排：调用 `SceneData.Refresh_RoadDataList()`（需改为 public 或新增接口）

**涉及文件（建议新增或修改）**
- `EditorTool/Editor/InspectorWindow.cs`：绑定 Road CRUD 事件 → 调用 `EditorCenter`。
- `EditorTool/UIManager/InspectorWindowManager.cs`：添加 Road ListView 与按钮事件。
- `UI/InspectorWindow/RoadEditorPanel.uxml`：增加 Road 列表与表单区域。
- `Data/SceneData.cs`：新增/公开 `Remove_RoadData`、`CreateRoadData`、`Refresh_RoadDataList` 等接口。
- `EditorTool/EditorCenter.cs`：新增 `RoadListChanged` 事件 + CRUD 转发。

---

### B. Block 列表 + 位移数据 CRUD（重点）
**目标**：在 Road 详情中显示 Block 列表，并支持创建/编辑/删除 `IBlockDisplacementData`。

**功能设计**
- **Block 列表区**
  - 显示：`BlockLocalIndex`、是否 Tap、是否有位移数据、位移类型摘要
  - 选择 Block → `EditorCenter.JumpTo(roadIndex, blockIndex)`
  - 支持“快速跳转”与“多选批量设置”

- **位移数据 CRUD**
  - 无数据时提供“创建位移数据”按钮 → `new ClassicBlockDisplacementData(blockIndex)`
  - 有数据时可修改 `turnType` / `displacementType`
  - 删除数据：从 `RoadData.blockDisplacementDataList` 移除
  - 批量应用：对选中 blocks 统一设置（支持多个 block）

**涉及文件（建议新增或修改）**
- `EditorTool/Editor/InspectorWindow.cs`：在 `OnBlockSelected` 中处理空数据创建 + 绑定 UI 事件。
- `EditorTool/UIManager/InspectorWindowManager.cs`：增加 Block ListView、批量操作按钮。
- `UI/InspectorWindow/BlockEditorPanel.uxml`：新增 Block 列表 + CRUD 按钮 + 详情区。
- `Scene/ClassicRoad.cs`：增加 `RemoveDisplacementData` 等辅助方法（或直接在 `RoadData` 上操作）。
- `Data/SceneData.cs` / `RoadData`：提供更明确的“添加/移除/批量设置”接口。

---

### C. Inspector 布局升级建议（UI 结构）
**目标**：从“单列按钮”升级为“左侧列表 + 右侧详情”的编辑模式。

**布局建议**
- **Road 面板**：
  - 左侧：Road ListView + 工具栏（新建/删除/复制/刷新）
  - 右侧：Road 详情表单 + 操作按钮（Note Range、Segment、Save Transform）

- **Block 面板**：
  - 左侧：Block 列表（支持多选）
  - 右侧：位移规则详情（type/turn/displacement + 应用 + 删除）

**实现方式**
- UIElements `ListView` + `VisualElement` 模板行
- 使用 `DancingBallEditor_Common.uss` 统一风格

---

## 具体修改点与原文定位

### 1) 需要绑定的 UI 事件（现状未连接）
**原文参考**：`Assets/MusicTogether/DancingBall/EditorTool/UIManager/InspectorWindowManager.cs`

```csharp
_roadModifyNoteBeginButton.clicked += () => RoadModifyNoteBeginRequested?.Invoke(_roadNoteBeginField?.value ?? 0);
_classicBlockApplyTurnTypeButton.clicked += () => ClassicBlockApplyTurnTypeRequested?.Invoke(_classicBlockTurnTypeField?.value);
```

**建议**：在 `InspectorWindow.BindEditorCenter` 中补充所有事件绑定（Road/Block）。

### 2) Block 空数据逻辑未实现
**原文参考**：`Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`

```csharp
if (displacementData == null)
{
    
}
```

**建议**：
- 当 `displacementData == null` 时显示“创建数据”按钮
- 在点击后创建 `ClassicBlockDisplacementData` 并调用 `RoadData.Set_BlockData`

### 3) Road/Block 列表缺失
**原文参考**：
- `UI/InspectorWindow/RoadEditorPanel.uxml`、`BlockEditorPanel.uxml` 无 ListView
- `EditorCenter` 无 Road/Block 列表变化事件

**建议**：新增 ListView + 对应数据绑定与刷新方法。

---

## 推荐的 API 设计（供下位 AI 快速落地）

### EditorCenter 扩展（建议接口）
- `event Action<List<RoadData>> OnRoadListChanged`  
- `event Action<List<IBlock>> OnBlockListChanged`
- `void CreateRoad(RoadData data)` / `void DeleteRoad(string roadName)` / `void DuplicateRoad(string roadName)`
- `void CreateBlockDisplacementData(int blockIndex, IBlockDisplacementData data)`
- `void RemoveBlockDisplacementData(int blockIndex)`

### RoadData 扩展（建议接口）
- `bool AddOrReplace_BlockData(IBlockDisplacementData data)`
- `bool Remove_BlockData(int blockIndexLocal)`（已有，但建议公开用途与返回值）

---

## 关键风险与边界条件（实现时应处理）
1. **空 Map / 空 SceneData** → Inspector 显示“未绑定”提示，禁用操作。
2. **RoadName 重复** → 创建时自动重命名或拒绝保存。
3. **BlockCount 与 note 范围不一致** → 修改 note 时要同步 `RecoverBlocks()`。
4. **大量 Block（5000+）** → ListView 需虚拟化与分页。
5. **批量操作撤销** → 可考虑引入 Undo（UnityEditor.Undo）。

---

## 最小落地路径（优先级）
1. 在 `InspectorWindow` 绑定现有按钮事件（快速解锁已有功能）
2. Road ListView + 新建/删除（核心 CRUD）
3. Block ListView + DisplacementData 创建/删除
4. 右侧详情编辑 + 批量操作

---

## 结语
当前 InspectorWindow 的 CRUD 功能与列表视图**显著不足**，阻碍 Road 与 Block 的数据编辑。建议按上述方案逐步实现：先完成事件绑定与 Road CRUD，再补全 Block 数据创建逻辑与列表。该方案对现有数据结构（`SceneData` / `RoadData` / `ClassicRoad`）改动较小，可快速落地。

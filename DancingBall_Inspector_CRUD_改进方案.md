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

---

## 近期实装需求：Block 界面功能（仅 Classic）

> 目标：让 Block 面板可用；先不考虑多类型扩展，仅围绕 `ClassicBlockDisplacementData` 完成创建/展示/删除流程。

### ✅ 功能需求（Block 面板内部完成）
1. **当 Block 没有 `IBlockDisplacementData` 时**：
  - 显示 **IDisplacementData 类型选择器**（当前仅 `Classic`，但保留 UI 入口）。
  - 显示 **创建数据按钮**。
  - 点击按钮后：为当前 `blockLocalIndex` 创建 `ClassicBlockDisplacementData`，并写入 `RoadData.blockDisplacementDataList`。

2. **当 Block 已存在 displacement data 时**：
  - 隐藏类型选择器与创建按钮。
  - 显示该数据的详细信息（`turnType`、`displacementType`）。
  - 显示 **删除当前数据按钮**：删除当前 block 的 displacement data 并刷新 UI。

3. **UI 行为要求**：
  - 所有操作均在 Block 面板内完成，不依赖外部 Road/Map 面板。
  - 创建 / 删除后需刷新当前 Block 的显示与列表状态。

### ✅ 数据与逻辑对接点（落地提示）
- UI：`Assets/MusicTogether/DancingBall/UI/InspectorWindow/BlockEditorPanel.uxml`
- UI 管理：`InspectorWindowManager` 中添加对应控件引用与显隐切换
- 逻辑入口：`InspectorWindow.OnBlockSelected(...)`
- 数据操作：`EditorCenter.CreateBlockDisplacementDataForSelected(...)` / `RemoveBlockDisplacementDataForSelected(...)`

### ✅ 交互状态（简化版）
- `displacementData == null` → **显示创建区域** / **隐藏详情+删除**
- `displacementData != null` → **显示详情+删除** / **隐藏创建区域**

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

## 分阶段代码级待办清单（按 Data → UI → UIManager/EditorCenter 顺序）

### Phase 1：数据层（Data / Scene）
- `Assets/MusicTogether/DancingBall/Data/SceneData.cs`
  - [x] 新增函数：`public RoadData CreateRoadData(string roadName, int segmentIndex, int noteBegin, int noteEnd)`
  - [x] 新增函数：`public bool RemoveRoadData(string roadName)`（按 `roadName` 删除）
  - [x] 新增函数：`public bool RenameRoadData(string oldName, string newName)`（确保唯一性）
  - [x] 新增函数：`public void RefreshRoadDataList()`（将 `Refresh_RoadDataList` 改为 public 包装）

- `Assets/MusicTogether/DancingBall/Data/SceneData.cs`（或 `RoadData` 内部）
  - [x] 新增函数：`public IBlockDisplacementData CreateBlockDisplacementData(int blockLocalIndex, Type dataType)`（落在 `RoadData` 中）
  - [x] 新增函数：`public bool RemoveBlockDisplacementData(int blockLocalIndex)`（落在 `RoadData` 中）
  - [x] 新增函数：`public bool AddOrReplace_BlockData(IBlockDisplacementData data)`（新增公共包装）

- `Assets/MusicTogether/DancingBall/Data/SceneData.cs`
  - [x] 新增函数：`public bool ValidateRoadNameUnique(string roadName)`（用于 UI 提示）

- `Assets/MusicTogether/DancingBall/Scene/ClassicRoad.cs`
  - [x] 新增函数：`public bool EnsureBlockDisplacementData(int blockLocalIndex, IBlockDisplacementData data)`（调用 `RoadData.AddOrReplace_BlockData` + `OnBlockDisplacementRuleChanged`）
  - [x] 新增函数：`public bool RemoveBlockDisplacementData(int blockLocalIndex)`（调用 `RoadData.Remove_BlockData` + `OnBlockDisplacementRuleChanged`）

### Phase 2：UI 结构（UXML / USS）
- `Assets/MusicTogether/DancingBall/UI/InspectorWindow.uxml`
  - [x] 为 Inspector 主体添加 `ScrollView`，支持滚动显示。
- `Assets/MusicTogether/DancingBall/UI/InspectorWindow/MapEditorPanel.uxml`
  - [x] 将 Road 列表移至 Map 内，新增 `road-list-view` + `road-create/delete/duplicate/refresh` 工具栏。
  - [x] Map/Road/Block 通用/继承区域改为 `Foldout`。
- `Assets/MusicTogether/DancingBall/UI/InspectorWindow/RoadEditorPanel.uxml`
  - [x] Road 内显示 Block 位移数据列表：`block-displacement-list-view` + 工具栏。
  - [x] 保留 Road 详情表单 `road-detail-form`。
- `Assets/MusicTogether/DancingBall/UI/InspectorWindow/BlockEditorPanel.uxml`
  - [x] 移除 Block 列表，保留位移数据详情与提示文本。
- `Assets/MusicTogether/DancingBall/UI/DancingBallEditor_Common.uss`
  - [x] 新增样式：`db-list`, `db-list-row`, `db-toolbar`, `db-detail-form`, `db-status-tag`

### Phase 3：UI 管理器 + EditorCenter 业务联动
- `Assets/MusicTogether/DancingBall/EditorTool/UIManager/InspectorWindowManager.cs`
  - [x] 新增列表绑定方法：`BindRoadList(...)` / `BindBlockDisplacementList(...)`
  - [x] 新增事件：`RoadCreateRequested` / `RoadDeleteRequested` / `RoadDuplicateRequested` / `RoadRefreshRequested`
  - [x] 新增事件：`BlockDisplacementCreateRequested` / `BlockDisplacementDeleteRequested` / `BlockDisplacementApplyBatchRequested`
  - [x] 新增选择事件：`RoadListSelectionChanged` / `BlockDisplacementSelectionChanged`
  - [ ] **Block 面板显示逻辑**：新增/调整控件引用（类型选择器、创建按钮、详情区、删除按钮）
  - [ ] **Block 面板显隐切换**：
    - `displacementData == null` → 显示类型选择器 + 创建按钮，隐藏详情 + 删除
    - `displacementData != null` → 显示详情 + 删除，隐藏创建区

- `Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`
  - [x] 在 `BindEditorCenter()` 中补全 CRUD 事件绑定（RoadCreate/Delete/Duplicate、BlockDisplacementCreate/Delete）
  - [x] 在 `OnRoadSelected` / `OnBlockSelected` 中刷新 Road/位移数据列表
  - [x] Road 列表选择 → `EditorCenter.JumpTo(roadIndex)`
  - [x] 位移数据列表选择 → `EditorCenter.JumpTo(roadIndex, blockLocalIndex)`
  - [ ] **Block 无数据时创建**：调用 `EditorCenter.CreateBlockDisplacementDataForSelected(...)`
  - [ ] **Block 有数据时展示/删除**：同步 `turnType` / `displacementType`，删除按钮调用 `EditorCenter.RemoveBlockDisplacementDataForSelected()`

> 已完成：Road/BlockDisplacement CRUD 的实际数据操作已在 EditorCenter/SceneData 层落地。

### Road/BlockDisplacement CRUD 业务联动（按文件拆分）
- `Assets/MusicTogether/DancingBall/EditorTool/EditorCenter.cs`
  - [x] 新增事件：`OnRoadListChanged` / `OnBlockListChanged`（CRUD 后广播刷新）
  - [x] 新增方法：`CreateRoad(...)` / `DeleteRoad(...)` / `DuplicateRoad(...)`
    - `CreateRoad`：调用 `SceneData.CreateRoadData(...)` → `targetMap.RecoverRoads()` → `OnRoadListChanged`
    - `DeleteRoad`：调用 `SceneData.RemoveRoadData(...)` → `targetMap.RecoverRoads()` → `OnRoadListChanged`
    - `DuplicateRoad`：复制 `RoadData` 并改名 → `SceneData.Set_RoadData(...)` → `targetMap.RecoverRoads()` → `OnRoadListChanged`
  - [x] 新增方法：`CreateBlockDisplacementData(...)` / `RemoveBlockDisplacementData(...)`
    - `CreateBlockDisplacementData`：`selectedRoad.RoadData.CreateBlockDisplacementData(...)` → `selectedRoad.OnBlockDisplacementRuleChanged()` → `OnBlockListChanged`
    - `RemoveBlockDisplacementData`：`selectedRoad.RemoveBlockDisplacementData(...)` → `OnBlockListChanged`

- `Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`
  - 绑定新增事件：
    - `RoadCreateRequested / RoadDeleteRequested / RoadDuplicateRequested` → 调用 `EditorCenter` 对应方法
    - `BlockDisplacementCreateRequested / BlockDisplacementDeleteRequested / BlockDisplacementApplyBatchRequested` → 调用 `EditorCenter` 或 `ClassicRoad` 批量应用
  - [x] CRUD 完成后统一调用 `EditorCenter.RefreshSelection()` + `BindRoadList/BindBlockDisplacementList`
  - [x] 处理 `OnBlockSelected` 中 `displacementData == null` 的创建逻辑与 UI 提示

- `Assets/MusicTogether/DancingBall/EditorTool/UIManager/InspectorWindowManager.cs`
  - [x] 为 Road 列表与位移数据列表增加“空列表提示”（如 `ListView` 无数据时显示提示）
  - [x] 为位移数据列表支持 `blockLocalIndex` 与类型摘要展示（已做基础，待加入状态标签）

### Road 创建弹窗（新增，Editor/Runtime UI 复用）
- `Assets/MusicTogether/DancingBall/UI/RoadCreateWindow.uxml`
  - [x] Road 创建表单 UI（name/segment/note range）
- `Assets/MusicTogether/DancingBall/EditorTool/UIManager/RoadCreateWindowManager.cs`
  - [x] Road 创建弹窗的元素控制与回调封装
- `Assets/MusicTogether/DancingBall/EditorTool/Editor/RoadCreateWindow.cs`
  - [x] 通过 UXML + Manager 构建 Editor 弹窗并回调创建
- `Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`
  - [x] RoadCreateRequested 改为弹窗输入后创建

### 位移数据类型自动映射（基础版）
- `Assets/MusicTogether/DancingBall/EditorTool/BlockDisplacementDataType.cs`
  - [x] 增加位移数据类型枚举（当前含 Classic）
- `Assets/MusicTogether/DancingBall/EditorTool/UIManager/InspectorWindowManager.cs`
  - [x] 读取 EnumField 选择并提供 `GetSelectedDisplacementDataType()`
- `Assets/MusicTogether/DancingBall/EditorTool/Editor/InspectorWindow.cs`
  - [x] 创建位移数据时根据枚举映射到 `ClassicBlockDisplacementData`

### 待实现：批量应用策略（逻辑规划）
- 批量范围来源：
  - [x] 方案 A：使用当前列表多选（已切换 `selectionType` 为 `Multiple`）
  //- 方案 B：输入区间（起始/结束索引）
- 批量应用逻辑：
  - [x] 以当前选中的位移数据为模板，批量 `AddOrReplace_BlockData`
  - [x] 调用 `OnBlockDisplacementRuleChanged()` 统一刷新
  - 可选：接入 `Undo.RecordObject` 以支持撤销

- `Assets/MusicTogether/DancingBall/EditorTool/EditorCenter.cs`
  - [x] 新增事件：`OnRoadListChanged` / `OnBlockListChanged`
  - [x] 新增方法：`CreateRoad(...)` / `DeleteRoad(...)` / `DuplicateRoad(...)` / `SelectRoad(int index)`
  - [x] 新增方法：`CreateBlockDisplacementData(...)` / `RemoveBlockDisplacementData(...)` / `SelectBlock(int index)`
  - [x] 在 CRUD 后统一触发 `RefreshSelection()` + `OnRoadListChanged` / `OnBlockListChanged`


## 结语
当前 InspectorWindow 的 CRUD 功能与列表视图**显著不足**，阻碍 Road 与 Block 的数据编辑。建议按上述方案逐步实现：先完成事件绑定与 Road CRUD，再补全 Block 数据创建逻辑与列表。该方案对现有数据结构（`SceneData` / `RoadData` / `ClassicRoad`）改动较小，可快速落地。

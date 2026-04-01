# DancingBall 项目迁移指南（旧版 Archived_DancingBall → 新版 DancingBall）

本文档用于指导将旧版 `Archived_DancingBall` 的关卡/数据结构迁移到新版 `DancingBall`。

> 适用范围：
> - 旧版目录：`Assets/MusicTogether/Archived_DancingBall`
> - 新版目录：`Assets/MusicTogether/DancingBall`

---

## 迁移目标与原则

- **目标**：将旧版 Road/Block 数据迁移为新版的 RoadData 与 IBlockDisplacementData 结构。
- **原则**：
  - 不再依赖全局索引（`roadIndex_Global` / `blockIndex_Global`）
  - Road 按 **时间排序** 与 **名称识别**
  - Block 规则由“数据记录”升级为“可执行规则对象”

---

## 结构对照（核心差异）

### RoadData

| 旧版字段 | 新版字段 | 变化说明 |
|---|---|---|
| `RoadGlobalIndex` | *弃用* `roadIndex_Global` | 新版不再依赖全局索引，改用列表顺序+时间排序 |
| *无* | `roadName` | 新增 Road 名称，用于定位与替换 |
| `TargetSegmentIndex` | `targetSegmentIndex` | 保留 |
| `NoteBeginIndex` | `noteBeginIndex` | 保留 |
| `NoteEndIndex` | `noteEndIndex` | 保留 |
| `blockDataList : List<BlockData>` | `blockDisplacementDataList : List<IBlockDisplacementData>` | 数据类型变为接口化规则对象 |

### Block 数据

| 旧版 BlockData | 新版 IBlockDisplacementData / ClassicBlockDisplacementData |
|---|---|
| 仅存 `TurnType`/`DisplacementType` | 同样字段，但**新增 ApplyDisplacementRule()** 逻辑 |
| `HasTurn/HasDisplacement` | `HasDisplacementRule`（意义略不同） |

---

## 迁移步骤（推荐流程）

### 1. Road 迁移（数据层）

1) 从旧版 `SceneData.roadDataList` 读取 Road 条目。
2) 为每条 Road 生成 `roadName`（建议：`Road_{TargetSegmentIndex}_{NoteBeginIndex}`）。
3) 将 `TargetSegmentIndex / NoteBeginIndex / NoteEndIndex` 迁移到新版 RoadData。
4) 使用 `SceneData.Set_RoadData()` 写入新版数据。

> 注意：新版 `SceneData` 会根据起始时间排序 Road，请避免依赖旧索引顺序。

### 2. Block 迁移（规则层）

对每个旧版 `BlockData`：

1) 创建 `ClassicBlockDisplacementData` 对象。
2) 将 `turnType` / `displacementType` 映射过去。
3) 设置 `BlockIndex_Local`。
4) 将该对象写入对应 RoadData 的 `blockDisplacementDataList`。

**映射示例（字段一致）**：

- `BlockData.turnType` → `ClassicBlockDisplacementData.turnType`
- `BlockData.displacementType` → `ClassicBlockDisplacementData.displacementType`
- `BlockData.blockLocalIndex` → `ClassicBlockDisplacementData.BlockIndex_Local`

### 3. 移除旧接口调用

旧版常用方法（需替换）：

- `SceneData.GetBlockData()`
- `SceneData.SetBlockData()`
- `SceneData.HasTap()` / `GetNoteTime()`

新版建议：

- Block 数据由 `RoadData` 管理：`RoadData.Get_BlockData()` / `Set_BlockData()`
- Note 时间由 `Segment` 查询（`SceneData.GetSegment()`）

---

## 迁移检查清单 ✅

- [ ] 所有 Road 条目已生成 `roadName`
- [ ] `roadIndex_Global` 不再作为逻辑索引使用
- [ ] Block 数据全部转换为 `ClassicBlockDisplacementData`
- [ ] Block 规则已写入 `blockDisplacementDataList`
- [ ] 旧版 Block CRUD 接口已替换为 RoadData 级别操作

---

## 常见问题与注意事项

### Q1：为什么 Road 顺序变了？
新版 `SceneData` 会按 Road 起始时间排序，请确保 **路段顺序不再依赖旧索引**。

### Q2：`HasDisplacementRule` 行为不一致？
旧版是“任意 Turn/Displacement 即为规则”，新版逻辑更多依赖 **ApplyDisplacementRule**。建议统一以新版逻辑为准。

### Q3：Block 的位移规则如何生效？
新版通过 `IBlockDisplacementData.ApplyDisplacementRule()` 实现位移逻辑，不再由 `SceneData` 统一处理。

---

## 后续建议

- 为迁移后的数据添加一次完整运行验证（Road 顺序、Block 对齐、Note 时间）。
- 若需要扩展不同规则类型，可新增更多 `IBlockDisplacementData` 实现类。

---

如需我进一步生成**迁移脚本**或**自动转换工具**，告诉我你更偏向编辑器工具还是离线脚本即可。
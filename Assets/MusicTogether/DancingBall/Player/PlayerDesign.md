## 玩家移动设计文档（PlayerDesign）

更新时间：2026-03-14

本文件描述玩家在 DancingBall 模块中的运动设计与实现要点，包含：到终点停下、fallback 速度计算、block 假设、时间倒序的跳过与警告，以及所需函数清单与接口契约。目标是把需求变成可直接实现的函数接口与明确边界情况。

---

## 目标概述

- 玩家从 `Road[0].Block[0]` 开始，按 `SceneData` 中定义的 block 顺序前进。
- 当玩家到达需要 tap 的 block（即对应 note）时，必须点击鼠标左键才能继续前进；否则玩家应维持“当前速度参数”并停在当前进度上（不跳到下一 block）。
- 在其余情况下，玩家自动获取并执行运动数据（MovementData），直到遇到下一个需要 tap 的 block 为止。
- 为性能考虑：每次生成运动数据时，应一次性查找到“下一个需要 tap 的 block”为止，把中间所有 block 的 MovementData 放入一个逻辑队列（实现上用 `List<MovementData>`），并在加入后按时间/起始时间排序以防数据计算误差。

额外约束（来自需求）：
1. 到终点停下。
2. 当无法从 scene/note 获得下一组运动时间数据时，使用 fallback：基于当前所在 road（若当前 road 没有节拍数据则使用前一个有效 road）的节拍信息计算平均速度，并按上一段 `MovementData` 的方向/时间持续向前移动直到能够找到下一组数据。
3. block 均为场景中默认生成的 block，无需担心不存在的 block（Map/GetBlock/TryGetRoad 等会做保险校验）。
4. 若生成的 MovementData 时间出现倒序（下一段的 beginTime < 上一段的 endTime），跳过此段并在控制台输出 warning（同时在开发工具中暴露诊断信息）。

---

## 术语与约定

- blockDistance：默认一块（相邻两个 block）之间的中心距离，约定为 1.0 个世界单位（用于速度计算）。
- blockRadius：0.5（仅用于碰撞/容差判定，不影响位移计算）。
- Tile：一个 block 内通常只有一个 tile，但可能出现多个 tile 同时存在（如多块铺设/装饰块）。因此**同一个 block 可能对应多个 tile 位置**。
- 玩家最终落点计算：
   - 以 tile 的中心点作为基准 `tilePosition`
   - 最终位置 = `tilePosition + tile.transform.up * playerRadius + tileThickness / 2`
   - 若一个 block 有多个 tile，需根据规则（见下）选择“目标 tile”（建议默认取最上层/与移动方向最近的 tile），并以该 tile 计算最终位置。
- MovementData：已有 `MovementData` 记录（见 `Player/MovementData.cs`），包含 beginTime、beginPos、beginRot、endTime、endPos、endRot。支持 `SetBegin(...)` 以在玩家传送到 road 开头时修正起始信息。
- movementQueue：设计上的队列，使用 `List<MovementData>` 实现并维护 `int currentIndex` 指示当前执行段。

---

## 状态机（高层）

PlayerState（核心字段）:
- int currentRoadIndex
- int currentBlockLocalIndex
- bool waitingForTap
- List<MovementData> movementQueue
- int movementQueueIndex
- MovementData? lastCompletedMovement

状态转换要点：
- 初始化：SetCurrentBlock(0, 0)，生成队列直到第一个 tap block（或终点）。
- 每帧 Update：若 movementQueue 有可执行段且不处于等待 tap，则按当前时间插值执行当前 MovementData；到达段末则推进 `movementQueueIndex`。
- 若到达需要 tap 的 block：进入 `waitingForTap`；在 `waitingForTap` 状态下，若用户点击左键则退出等待并继续执行，否则保持不推进。
- 若 movementQueue 耗尽且不是终点：调用 `BuildMovementListUntilNextTap()` 生成下一批数据；若仍无法生成且存在 fallback 可用数据，则用 fallback 持续生成直到能查到 note。

---

## 核心函数列表（建议放到 `PlayerController.cs`）

说明：函数名与签名可根据项目风格调整；下列为推荐函数与职责。

1. InitializeAtStart()
   - 功能：把玩家放到 `Road=0, Block=0`，清空队列，触发第一次队列构建。
   - 放置：Start 或初始化入口。

2. SetCurrentBlock(int roadIndex, int blockLocalIndex, bool snapTransform = true)
   - 功能：更新当前 block 索引，必要时把玩家 transform 置于该 block 的 transform（snap）。

3. BuildMovementListUntilNextTap()
   - 功能：从当前 block（不含已在其上的“当前段”的下一个）开始，逐个 `NextBlock`，为每个 block 构建 MovementData 并加入 `movementQueue`，直到遇到 `SceneData.HasTap(...)` 返回 true 的 block（该 block 的 MovementData 也需加入作为最后一段）。
   - 行为：加入后对 `movementQueue` 按 `beginTime` 排序（若 `beginTime` 无效或为 0，可用 lastCompletedMovement.endTime + delta 自动填充）；若发现时间倒序（新的 beginTime < 上一段 endTime）则跳过该段并记录 `Debug.LogWarning(...)`（但仍尝试继续生成）。

4. TryBuildMovementData(int fromRoad, int fromBlockLocal, int toRoad, int toBlockLocal, out MovementData data)
   - 功能：构建单段 MovementData：读取 `SceneData.GetNoteTime(toRoad,toLocal,out time)`，若失败则返回 false（调用方决定是否走 fallback）；若成功，使用 beginTime（如果从 lastCompletedMovement 可得 endTime 则用之，否则使用当前播放时间），并填充 begin/end Transform。
   - 注意：当目标 block 内含多个 tile 时，应先选择目标 tile，然后使用其 `tilePosition` 与 `tile.transform.up` 计算最终落点。

5. SelectTargetTile(Block block, Vector3 moveDirection, out Transform tileTransform)
   - 功能：当一个 block 内有多个 tile 时，选择一个目标 tile。
   - 推荐策略：
     - 优先选择与 `moveDirection` 点积最大的 tile（方向最接近的 tile）。
     - 如果 moveDirection 无效，则选择 world-space 高度最高的 tile。
   - 输出：目标 tile 的 Transform，用于计算最终落点。

5. ComputeFallbackMovementData(MovementData lastMovement, int fallbackRoadIndex, out MovementData data)
   - 功能：当无法获取 note 时间时，基于 `fallbackRoadIndex` 的节拍信息计算平均速度并沿 lastMovement 的方向生成若干连续 MovementData，直到下一 note 被找到或到达终点。
   - 计算方法（见下）：以 road 的 InputNotes（目标 segment）在 `NoteBeginIndex..NoteEndIndex` 中的 note 时间计算平均 note 间隔 avgInterval（秒）；速度 = blockDistance / avgInterval（blockDistance=1.0）。若该 road 没有节拍/notes，则尝试使用前一个 road 的节拍；若都没有，则使用一个合理默认速度（例如 1 block / sec）。

6. UpdateMovement(double currentTime)
   - 功能：核心的 per-frame 更新，按当前 MovementData 插值 transform；处理到段末、等待 tap、以及到终点停下。

7. IsTapRequired(int roadIndex, int blockLocalIndex)
   - 功能：封装 `SceneData.HasTap(...)` 的调用。

8. TryConsumeTapInput()
   - 功能：检查 `Input.GetMouseButtonDown(0)` 并返回 true/false；在 `waitingForTap` 时使用。

9. LogTimeOrderWarning(MovementData prev, MovementData next)
   - 功能：把时间倒序情况写入 Debug，并在 Editor UI（若有）暴露。

11. StopAtEnd()
   - 功能：当 `Map.NextBlock(...)` 返回 false（已到最后一块）时，停止所有运动并置 `movementQueue` 清空，状态为终点。

---

## Fallback 速度计算（详细）

场景：当 `TryBuildMovementData` 在读取下一段的 note 时间失败（e.g., `SceneData.GetNoteTime` 返回 false），并且此时需要继续推进以寻找下一段 note 数据时执行 fallback。

步骤：
1. 选取 `fallbackRoadIndex`：优先使用当前 `currentRoadIndex`（在范围内且有 note 数据）；如果当前 road 没有可用节拍信息，则选择前一个 road（`currentRoadIndex - 1`）中第一个可用的 road（向前查找）。
2. 从该 road 的 `TargetSegmentIndex` 中提取 note 列表（`SceneData` 提供），在 `NoteBeginIndex..NoteEndIndex` 范围内计算所有连续 note 时间间隔的平均值 avgInterval（若只有一个 note 或无间隔，退回默认值，例如 1.0s）。
3. 速度计算：
   - blockDistance = 1.0（约定）
   - speed = blockDistance / avgInterval （单位：units / second）
4. 基于 `lastMovement` 的终点位置和朝向创建若干 MovementData，单段时长以 avgInterval 为基准：即每前进一步（1 block）耗时 avgInterval，endPos 指向下一个 block 的 center（或沿 lastMovement 方向推进 1 unit），并以此生成直到下一 note 被找到或到达终点。

注意事项：
- 若 avgInterval 非法（0 或 NaN）：退回默认速度（例如 1 block / sec）。
- fallback 生成的 MovementData 其时间戳也应与 lastMovement.endTime 相接；若发生时间重叠或倒序，需触发时间倒序警告并按时间排序/跳过。

---

## 时间倒序处理策略

场景：在构建 movementQueue 时，若某段 MovementData 的 beginTime < 上一段的 endTime（即时间倒序或重叠），采取以下策略：

1. 记录警告：调用 `LogTimeOrderWarning(prev, next)`，并输出 `roadIndex/blockIndex` 与两个时间戳和差值。
2. 跳过该段（不要把它放进队列），继续尝试构建后续段；若该段为必须到达的 tap block，仍需追踪但标注为“时间异常且需手动修正”——在 Editor 中高亮显示其 Block。
3. 在 Editor Mode（UNITY_EDITOR）下，发出额外的诊断事件 `EditorActionDispatcher` 以便人工修正。

理由：跳过确保运行时不会因时间倒序而瞬移或负速推进，同时为编辑者保留诊断信息便于修正源数据。

---

## 边界与异常情况

- 当前处于最后一个 block：调用 `StopAtEnd()` 停止一切自动生成并触发 UI 提示或回到主菜单（由游戏逻辑决定）。
- 无 road 可用节拍且无前向 road：使用默认速度（1 block / sec）作为最后的 fallback。
- 连续多个 tap block：`BuildMovementListUntilNextTap` 可以只生成到第一个 tap block；点击后再生成下一段。若需要预测多次 tap，可扩展为扫描多个 tap block 并把多个 tap block 的 MovementData 一次性加入队列。
- 多 tile block：若目标 block 内含多个 tile，必须先选择目标 tile，并按“玩家最终落点计算公式”计算 endPosition；否则会造成高度偏差或穿模。

---

## 日志与诊断（开发期）

- 在每次生成 `movementQueue` 时输出生成统计：起始 block -> 结束 block、段数、是否使用 fallback、是否发生时间倒序。
- 时间倒序时输出 `Debug.LogWarning` 并在 Editor 下触发 `EditorActionDispatcher` 便于定位。

---

## 后续实现建议（可选）

1. 在 `PlayerController` 完成后，添加单元测试或编辑器工具来模拟不同 note 缺失场景，验证 fallback 行为。
2. 修复 `SceneData.GetNoteTime` 的返回值（当前实现末尾始终 `return false;`，会导致所有时间查询失败）。
3. 在 `MovementData` 上增加一个 `source` 字段（enum：NoteDerived/Fallback/Manual）便于区分并在统计/日志中展示。

---

## 附：建议函数伪代码（简化）

见上文核心函数列表。实现时请确保：
- 每次把 movement 段 push 到 `movementQueue` 后调用排序（按 beginTime）并运行时间一致性检查；
- 在等待 tap 状态不要自动推进 `movementQueueIndex`；
- fallback 生成应与 lastCompletedMovement 的 endTime 严格衔接。

---

若你同意，我接下来可以：
1. 直接在项目中新增 `PlayerController.cs` 并实现上述函数的骨架与核心逻辑；
2. 顺带修复 `SceneData.GetNoteTime` 中的返回值 bug（以便 runtime 能正确读取 note 时间）。

请告诉我接下来要执行哪个步骤，我会继续实现并运行基本的编辑器/运行时自测。

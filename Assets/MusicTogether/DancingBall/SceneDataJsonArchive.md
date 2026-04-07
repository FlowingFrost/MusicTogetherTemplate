# SceneData JSON 存档

这个存档用于在类结构变更后保留 `SceneData` 数据的可恢复性，避免 Unity 反序列化失效。

## 功能

- 导出 `SceneData` 为 JSON（仅包含 Road 与 Block 规则）
- 从 JSON 还原 `SceneData` 的 Road 与 Block 数据
- 提供 Round Trip 校验入口，便于检查存档一致性

## 使用方式（Unity 编辑器）

1. 在 Project 视图中选中 `SceneData` 资源。
2. 菜单栏选择：
   - `MusicTogether/DancingBall/SceneData JSON Archive/Export Selected SceneData...`
   - `MusicTogether/DancingBall/SceneData JSON Archive/Import To Selected SceneData...`
3. 可选：`Validate Round Trip (Selected)` 用于快速检查导入导出是否一致。

## 存档格式要点

- `version`：存档版本（当前为 1）
- `roads`：Road 基本信息 + Block 规则

如需要扩展新类型的 `IBlockDisplacementData`，请在 `SceneDataJsonArchive.cs` 中补充映射逻辑。
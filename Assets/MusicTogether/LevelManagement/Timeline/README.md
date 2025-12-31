# LevelUnion Timeline Track 使用说明

## 概述

基于 Timeline 的 ILevelUnion 驱动系统，支持在 Edit Mode 和 Play Mode 下通过 Timeline 精确控制每个 LevelUnion 的生命周期和更新。

---

## 核心组件

- **LevelUnionTrack**: 自定义轨道，绑定到 ILevelManager
- **LevelUnionAsset**: Clip 资产，配置具体的 ILevelUnion 引用
- **LevelUnionBehaviour**: Playable 行为，负责调用 AwakeUnion/StartUnion/UpdateUnion

---

## 使用步骤

### 1. 创建 Timeline
在场景中选择 GameObject，添加 PlayableDirector 组件，创建 Timeline Asset。

### 2. 添加 LevelUnionTrack
在 Timeline 窗口中右键 → **MusicTogether → Level Union Track**

### 3. 绑定 LevelManager
在 Timeline 左侧的 Track 列表中，将场景中的 `SimpleLevelManager` 拖拽到 LevelUnionTrack 的绑定槽

### 4. 添加 Clip
右键 LevelUnionTrack → **Add Level Union Clip**

### 5. 配置 ILevelUnion
选中 Clip，在 Inspector 中：
- **Level Union**: 拖拽实现了 ILevelUnion 接口的 MonoBehaviour 对象

### 6. 调整时间范围
拖拽 Clip 的边缘调整：
- **开始时间**: Clip 激活时调用 `AwakeUnion()` 和 `StartUnion()`
- **持续时间**: 每帧调用 `UpdateUnion()`
- **结束时间**: Clip 结束时停止更新

---

## 示例配置

```
Timeline:
  ├─ Audio Track (音乐轨道)
  │   └─ Music Clip (0s - 120s)
  │
  └─ LevelUnion Track [绑定: SimpleLevelManager]
      ├─ Note Spawner Clip (0s - 120s)
      ├─ Enemy Manager Clip (10s - 100s)
      └─ Visual Effects Clip (30s - 60s)
```

---

## 生命周期

### Clip 激活时
```csharp
union.AwakeUnion();  // 初始化
union.StartUnion();  // 启动
```

### 每帧更新
```csharp
union.UpdateUnion(); // 在 Timeline 播放期间每帧调用
```

### Clip 结束时
```csharp
// 自动停止更新，无需额外清理
```

---

## 注意事项

1. **接口绑定**: Clip 中的 `Level Union` 字段类型为 `MonoBehaviour`，但必须实现 `ILevelUnion` 接口
2. **唯一 LevelManager**: 场景中只能有一个 LevelManager
3. **Edit Mode 预览**: 在编辑器中拖动 Timeline 时间轴即可预览效果
4. **Play Mode 运行**: SimpleLevelManager.Update() 仍负责状态管理，但 Union 更新由 Timeline 驱动

---

## 高级用法

### 半途开始的 Union
通过 Clip 的起始时间控制 Union 的激活时机：
```
例如：Boss 只在 60s - 120s 期间激活
Timeline:
  └─ LevelUnion Track
      └─ Boss Manager Clip (60s - 120s)
```

### 多个 Clip 复用同一个 Union
可以在不同时间段创建多个 Clip 引用同一个 Union 实例：
```
Timeline:
  └─ LevelUnion Track
      ├─ Effect Manager Clip (0s - 30s)
      ├─ Effect Manager Clip (60s - 90s)  // 同一个对象
```

---

## 调试

- 在 Inspector 中启用 `SimpleLevelManager.RunInEditorMode` 可在 Edit Mode 下完整预览
- 使用 `Debug.Log` 在 `AwakeUnion/StartUnion/UpdateUnion` 中输出日志验证调用
- Timeline 窗口下方的时间轴显示当前播放位置

---

## 与旧系统的区别

| 特性 | 旧系统 (MonoBehaviour.Update) | 新系统 (Timeline) |
|------|------------------------------|------------------|
| 驱动方式 | SimpleLevelManager.Update 递归调用 | Timeline Clip 独立驱动 |
| 生命周期控制 | 手动管理 | Timeline 自动管理 |
| 时间精度 | 帧同步 | Timeline 精确时间 |
| Edit Mode 预览 | 需要额外工具 | 原生支持 |
| 半途开始 | 需要额外逻辑 | Clip 起始时间直接控制 |

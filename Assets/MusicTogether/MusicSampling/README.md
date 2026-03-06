# Music Sampling Editor 使用指南

## 概述
基于 UI Toolkit 开发的音频采样 Editor 窗口，用于精确标记音乐节奏点，为音乐节奏游戏制作谱面。

## 核心功能

### ✨ 主要特性
- **Editor 模式音频播放** - 在编辑器中直接播放和控制音频
- **音频擦洗 (Audio Scrubbing)** - 拖拽时间轴实时预览音频
- **波形可视化** - 实时显示音频波形，辅助精确对音
- **音符标记** - 点击或快捷键标记节拍点
- **BPM 驱动** - 基于 BPM 和节拍细分自动计算音符位置
- **数据持久化** - 标记数据保存为 ScriptableObject 资源

---

## 快速开始

### 1. 准备音频文件
⚠️ **重要**: 音频文件必须设置为 `DecompressOnLoad` 加载类型

1. 在 Project 窗口选中音频文件
2. 在 Inspector 面板找到 **Load Type**
3. 设置为 **Decompress On Load**
4. 点击 Apply

### 2. 创建 AudioSamplingData 资源
1. 右键点击 Project 窗口
2. 选择 **Create > MusicTogether > Audio Sampling Data**
3. 命名（如 `MySong_SamplingData`）
4. 在 Inspector 中配置：
   - **Audio Clip**: 拖入准备好的音频文件
   - **BPM**: 设置歌曲的每分钟节拍数
   - **Beats Per Bar**: 设置拍型（每小节的拍数，如 4/4拍 则为 4）
   - **Beat Division**: 设置节拍细分（4 = 16分音符）
   - **Note Width**: 调整音符显示宽度
   - **Waveform Zoom**: 调整波形高度
   - **Samples Per Note**: 每个音符的波形采样条数

### 3. 打开采样窗口
菜单栏 **Window > MusicTogether > Music Sampling Window**

### 4. 开始标记
1. 将创建的 AudioSamplingData 拖入窗口顶部的 **Sampling Data** 字段
2. 点击 **Play** 按钮播放音频
3. 使用以下方式标记音符：
   - **点击音符条** - 直接点击波形上的音符
   - **空格键** - 标记当前播放位置的音符
   - **点击留白** - 点击波形下方留白区域标记当前音符
   - **标记按钮** - 点击底部的"标记当前音符"大按钮

---

## 界面说明

### 顶部控制面板
- **Sampling Data 字段**: 选择 AudioSamplingData 资源
- **▶ Play / ⏸ Pause 按钮**: 播放/暂停
- **■ Stop 按钮**: 停止并回到开始

### 中部信息区域
- **时间显示**: `当前时间 / 总时长` (格式: MM:SS.mmm)
- **BPM 显示**: 当前歌曲的 BPM 和拍型（如 "BPM: 120 (4/4拍)"）
- **小节/音符显示**: 当前播放位置的小节号和音符信息（如 "小节: 3 | 音符: 32 (1/16)"）

### 时间轴滑块
- **拖拽跳转**: 拖动滑块跳转到任意位置
- **音频擦洗**: 拖拽时会播放短暂音频预览（类似 Timeline 的 Audio Scrubbing）

### 波形显示区域
- **小节框**: 白色粗边框将音符按小节分组，每个小节顶部显示小节号
- **小节号**: 每个小节左上角显示的数字（如 1, 2, 3...）
- **节拍分隔线**: 每个节拍之间的细灰色垂直线
- **灰色条**: 未标记的音符
- **蓝色条**: 已标记的音符
- **黄色条**: 当前正在播放的音符（高亮显示）
- **红色线**: 当前播放位置（播放头）
- **波形**: 每个音符内部显示该时间段的音频波形

### 底部快速标记按钮
- **标记当前音符 (Space)**: 大按钮，点击即可标记当前播放位置的音符
- 避免手动精确点击小音符条的麻烦
- 与空格键功能相同，但更易操作

---

## 操作技巧

### 快捷键
- **空格键**: 标记当前播放位置的音符

### 标记方式对比
1. **快速标记按钮** ⭐ **推荐**
   - 播放音乐，听到节奏点时点击底部大按钮
   - 按钮大，容易点击，不会误操作
   
2. **空格键** ⭐ **最快**
   - 播放音乐，听到节奏点时按空格键
   - 适合快速连续标记
   
3. **直接点击音符条**
   - 暂停播放，观察波形找到明显的波峰
   - 精确点击对应的音符条标记
   - 适合微调和修正
   
4. **点击留白区域**
   - 点击波形下方留白区域也能标记当前音符
   - 与快速标记按钮功能相同

### 精确对音
1. **播放-标记法**: 
   - 播放音乐，听到节奏点时按空格键或点击标记按钮
   
2. **波形观察法**: 
   - 暂停播放，观察波形找到明显的波峰
   - 点击对应的音符条标记
   
3. **拖拽预览法**: 
   - 拖拽时间轴，听音频擦洗效果
   - 找到准确位置后点击音符标记

### 批量标记
- 播放音乐，在听到每个节拍点时连续按空格键
- 或者使用底部的快速标记按钮（推荐新手使用）
- 已标记的音符会显示为蓝色

### 取消标记
- 再次点击已标记（蓝色）的音符即可取消标记

### 理解小节和拍型
**小节 (Bar)**: 音乐中的基本组织单位
- 例如 4/4 拍：每小节有 4 拍，每拍是四分音符
- 例如 3/4 拍：每小节有 3 拍（华尔兹拍）
- 例如 6/8 拍：每小节有 6 个八分音符（可视为 2 拍）

**波形中的小节标记**:
```
┌─────小节1─────┐┌─────小节2─────┐┌─────小节3─────┐
│ 1  │ 2  │ 3  │ 4  ││ 1  │ 2  │ 3  │ 4  ││ 1  │ 2  │ 3  │ 4  │
│拍  │拍  │拍  │拍  ││拍  │拍  │拍  │拍  ││拍  │拍  │拍  │拍  │
└────────────────┘└────────────────┘└────────────────┘
```

**配置示例**:
- **4/4 拍，16分音符细分**: Beats Per Bar = 4, Beat Division = 4 → 每小节 16 个音符
- **3/4 拍，8分音符细分**: Beats Per Bar = 3, Beat Division = 2 → 每小节 6 个音符
- **6/8 拍，8分音符细分**: Beats Per Bar = 6, Beat Division = 1 → 每小节 6 个音符

---

## 技术特性

### 音频擦洗 (Audio Scrubbing)
拖拽时间轴时：
- 自动暂停播放
- 播放短暂音频预览
- 释放后恢复之前的播放状态

**优点**:
- 精确定位节奏点
- 类似专业音频编辑软件的体验
- 避免拖拽后的播放状态混乱

### 性能优化
- **增量更新**: 点击音符时只更新该音符状态，不重建整个波形
- **节流更新**: 播放头更新频率限制为 ~30fps
- **懒加载**: 只在需要时加载和渲染波形数据

### 数据结构
标记数据存储在 `AudioSamplingData.markedNoteIndices` 列表中：
```csharp
// 访问标记数据
List<int> markedNotes = samplingData.markedNoteIndices;

// 获取对应的时间
foreach (int noteIndex in markedNotes)
{
    double time = samplingData.GetTimeAtNoteIndex(noteIndex);
    Debug.Log($"Note {noteIndex} at time {time}");
}
```

---

## 常见问题

### Q: 音频无法加载？
A: 确保音频文件的 Load Type 设置为 **Decompress On Load**

### Q: 波形显示不正常？
A: 调整 `Waveform Zoom` 和 `Samples Per Note` 参数

### Q: 拖拽后音乐停止播放？
A: 已修复！现在拖拽结束后会自动恢复之前的播放状态

### Q: 点击音符时卡顿？
A: 已优化！现在使用增量更新，只刷新被点击的音符

### Q: 如何批量清除标记？
A: 在代码中调用：
```csharp
samplingData.ClearAllMarkedNotes();
EditorUtility.SetDirty(samplingData);
```

---

## 进阶用法

### 导出标记数据
```csharp
// 遍历所有标记的音符
foreach (int noteIndex in samplingData.markedNoteIndices)
{
    double time = samplingData.GetTimeAtNoteIndex(noteIndex);
    // 导出到你的游戏数据格式
}
```

### 自定义 BPM 变化
如果歌曲有变速，可以创建多个 AudioSamplingData 资源，分段标记。

### 集成到游戏
将 `markedNoteIndices` 转换为游戏中的音符数据：
```csharp
public class NoteData
{
    public float time;
    public int noteIndex;
}

List<NoteData> ConvertToGameData(AudioSamplingData samplingData)
{
    var notes = new List<NoteData>();
    foreach (int index in samplingData.markedNoteIndices)
    {
        notes.Add(new NoteData {
            time = (float)samplingData.GetTimeAtNoteIndex(index),
            noteIndex = index
        });
    }
    return notes;
}
```

---

## 文件结构

```
Assets/MusicTogether/MusicSampling/
├── AudioSamplingData.cs              # 数据类（ScriptableObject）
├── README.md                          # 使用文档
└── Editor/
    ├── MusicSamplingWindow.cs         # 主窗口逻辑
    ├── MusicSamplingWindow.uxml       # UI 布局定义（UXML）
    ├── MusicSamplingWindow.uss        # 样式表（USS）
    ├── EditorAudioPlayer.cs           # 音频播放控制
    └── WaveformVisualElement.cs       # 波形可视化组件
```

### 架构设计

**UI Toolkit 架构**:
- **UXML (MusicSamplingWindow.uxml)**: 声明式 UI 结构定义
- **USS (MusicSamplingWindow.uss)**: 集中管理样式和主题
- **C# (MusicSamplingWindow.cs)**: 业务逻辑和事件处理

**优点**:
- 🎨 样式与逻辑分离，易于调整 UI 外观
- 🔧 UXML 可视化编辑（UI Builder 支持）
- ♻️ 样式可重用，支持主题切换

---

## 版本历史

### v1.4 - 小节标记和拍型配置
- ✅ 新增拍型设置（Beats Per Bar）支持 2/4、3/4、4/4、5/4 等各种拍型
- ✅ 波形界面增加小节可视化标记
- ✅ 小节用白色粗边框框选，顶部显示小节号
- ✅ 信息栏显示当前小节和音符在小节内的位置
- ✅ 优化数据结构，添加小节相关计算方法

### v1.3.2 - 代码精简
- ✅ 移除 fallback 降级方案代码（不再需要）
- ✅ 简化 CreateGUI 逻辑，减少代码量 133 行

### v1.3.1 - 进度条拖拽修复
- ✅ 修复进度条拖拽后不再自动更新的问题
- ✅ 使用 TrickleDown 捕获阶段确保鼠标抬起事件被正确捕获
- ✅ 添加全局鼠标抬起监听作为安全措施
- ✅ 添加鼠标离开事件处理，防止拖拽状态卡住

### v1.3 - 滚动逻辑优化
- ✅ 精简滚动逻辑，移除复杂的平滑插值
- ✅ 修复暂停后再次播放时界面不同步滚动的问题
- ✅ 滚动位置直接跟踪播放位置，更加可靠

### v1.2 - 用户体验优化
- ✅ 新增当前播放音符的黄色高亮显示
- ✅ 修复暂停后拖动进度条再播放时的滚动问题

### v1.1 - 快速标记功能
- ✅ 新增"标记当前音符"大按钮
- ✅ 点击留白区域快速标记
- ✅ 支持空格键快捷标记

### v1.0 - 初始版本
- ✅ Editor 模式音频播放
- ✅ 音频擦洗功能
- ✅ 波形可视化
- ✅ 音符标记
- ✅ 性能优化
- ✅ 点击留白选中当前音符

### Bug 修复
- 🐛 修复拖拽到开始位置导致播放停止的问题
- 🐛 修复拖拽后按钮状态不同步的问题
- 🐛 修复点击音符时卡顿的性能问题
- 🐛 添加点击留白区域选中当前音符的功能

---

## 开发者备注

基于原有 UGUI 版本（`MusicSamplingArchived`）重新设计，采用 UI Toolkit 实现：
- 更现代的 UI 架构
- 更好的性能表现
- Editor 专用，无需运行时依赖
- 专注于音频采样，移除 Video 和 Timeline 支持

**对比 UGUI 版本的改进**:
1. 使用 UI Toolkit 的声明式 UI
2. 音频擦洗功能更流畅
3. 增量更新优化点击性能
4. 更简洁的代码结构

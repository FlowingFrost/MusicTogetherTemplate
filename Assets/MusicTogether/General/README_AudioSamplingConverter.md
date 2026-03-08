# 采音数据转换工具使用说明

## 概述
这个工具可以将 `AudioSamplingData` 中标记的音符自动转换到 `InputNoteData` 格式，方便在游戏中使用。

## 使用步骤

### 1. 准备采音数据
首先需要有一个 `AudioSamplingData` 资源，并且已经标记好音符：
- 设置好BPM（每分钟节拍数）
- 设置好beatDivision（节拍细分，如16分音符则为4）
- 标记好所有需要的音符位置

### 2. 设置InputNoteData
1. 在Project窗口右键选择 `Create > MusicTogether > NoteData` 创建一个新的InputNoteData资源
2. 在Inspector中设置：
   - **Audio Sampling Data**: 拖入你的AudioSamplingData资源
   - **Target Note Type**: 选择目标音符类型（默认为Semi，即16分音符）
     - `Quarter`: 四分音符（每拍1个）
     - `Eighth`: 八分音符（每拍2个）
     - `Semi`: 十六分音符（每拍4个）
     - `ThirtySecond`: 三十二分音符（每拍8个）

### 3. 执行转换
点击Inspector底部的 **"从采音数据转换"** 按钮，即可自动转换。

转换按钮会显示：
- 源数据的BPM
- 源数据的节拍细分
- 标记的音符数量
- 目标音符类型

### 4. 确认结果
转换完成后，会在Console窗口显示转换结果信息。
可以在`noteLists`中查看生成的音符数据。

## 转换原理

### 音符索引转换
AudioSamplingData使用`beatDivision`来定义音符细分，而InputNoteData使用`NoteType`枚举。转换时会自动计算转换比例：

```
转换比例 = 目标每拍音符数 / 源每拍音符数
目标索引 = 源索引 × 转换比例（四舍五入）
```

### 示例
假设AudioSamplingData设置：
- BPM = 120
- beatDivision = 4（16分音符）
- 标记的音符索引: [0, 4, 8, 12, 16]

转换到NoteType.Eighth（8分音符）：
- 转换比例 = 2 / 4 = 0.5
- 目标索引 = [0, 2, 4, 6, 8]

转换到NoteType.Semi（16分音符）：
- 转换比例 = 4 / 4 = 1.0
- 目标索引 = [0, 4, 8, 12, 16]（保持不变）

## 注意事项

1. **数据会被清空**: 转换操作会清空现有的所有`noteLists`数据，请确认后再执行
2. **默认4/4拍**: 工具默认使用4/4拍，如需其他拍号需在AudioSamplingData中设置好beatsPerBar
3. **索引精度**: 转换时会四舍五入到最近的整数索引
4. **BPM保持**: 转换后的BPM与源AudioSamplingData保持一致

## 右键菜单
在InputNoteData资源上右键，可以看到以下菜单：
- **Convert From Audio Sampling Data**: 执行转换（同按钮功能）
- **Print Debug Info**: 打印当前数据的调试信息

## 快捷键
无快捷键，请使用按钮或右键菜单。

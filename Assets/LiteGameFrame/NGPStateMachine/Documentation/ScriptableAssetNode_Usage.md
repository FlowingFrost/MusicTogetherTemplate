# ScriptableAsset Node 使用指南

## 概述

ScriptableAsset Node 允许你在状态机中引用和使用 ScriptableObject 资源，实现数据驱动的游戏逻辑。

## 核心特性

- **资源引用**：直接在节点中引用 ScriptableObject 资源
- **数据驱动**：通过配置文件控制游戏逻辑，无需修改代码
- **易于扩展**：继承 `ScriptableAssetNode<T>` 即可创建自定义节点
- **项目资源**：可以引用项目中的任何 ScriptableObject 资源

## 工作流程

### 1. 创建 ScriptableObject 资源

在 Project 窗口右键：
- `Create > State Machine > Example > Game Data`（示例数据）
- 或创建你自己的 ScriptableObject 类型

### 2. 配置资源数据

- 选中创建的资源文件
- 在 Inspector 中配置参数

### 3. 在图中使用

- 打开状态机图编辑器
- 添加 ScriptableAsset Node
- 将资源拖拽到节点的 `Asset` 输入端口或字段

### 4. 读取数据

- 节点会在执行时自动读取资源数据
- 可以将数据写入黑板供其他节点使用
- 或直接在节点内部使用资源数据

## 内置 ScriptableAsset Node

### LoadGameDataNode

**功能**：加载游戏数据并写入黑板

**参数**：
- `Asset` (ExampleGameData): 游戏数据资源
- `Print Info` (bool): 是否打印数据信息
- `Write To Blackboard` (bool): 是否写入黑板

**写入的黑板键**：
- `levelName` (string): 关卡名称
- `requiredScore` (int): 需要的分数
- `timeLimit` (float): 时间限制
- `enableHardMode` (bool): 是否启用困难模式

**示例**：
```
Entry → LoadGameData(asset=Level1Data) → Condition(timeLimit > 0) → StartLevel
```

### LoadScriptableAssetNode

**功能**：加载任意 ScriptableObject 到黑板

**参数**：
- `Asset` (ScriptableObject): 任意 SO 资源
- `Blackboard Key` (string): 存入黑板的键名

**示例**：
```
Entry → LoadAsset(asset=PlayerConfig, key="playerConfig") → InitPlayer
```

## 创建自定义 ScriptableAsset Node

### 示例 1：创建技能数据节点

#### 1. 定义数据类型

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public float cooldown;
    public int manaCost;
    public GameObject effectPrefab;
    
    public bool CanUse(int currentMana)
    {
        return currentMana >= manaCost;
    }
}
```

#### 2. 创建节点类

```csharp
using System;
using GraphProcessor;
using UnityEngine;

namespace LiteGameFrame.NGPStateMachine.Nodes
{
    [Serializable, NodeMenuItem("State Machine/ScriptableAsset/Use Skill")]
    public class UseSkillNode : ScriptableAssetNode<SkillData>
    {
        [Input("Current Mana")]
        [Tooltip("当前法力值")]
        public int currentMana = 100;
        
        [Output("Success")]
        [Tooltip("是否成功使用技能")]
        public bool success;
        
        public override string name => "Use Skill";
        public override Color color => new Color(0.8f, 0.3f, 0.5f);
        
        public override void OnEnterSignal(string sourceId)
        {
            // 拉取输入数据
            inputPorts.PullDatas();
            
            if (!TryGetAsset(out var skillData))
            {
                success = false;
                TriggerSignal();
                StopRunning();
                return;
            }
            
            // 检查是否可以使用
            if (skillData.CanUse(currentMana))
            {
                Debug.Log($"[UseSkillNode] Using skill: {skillData.skillName}");
                
                // 扣除法力值
                StateMachine.Set("currentMana", currentMana - skillData.manaCost);
                
                // 设置冷却时间
                StateMachine.Set($"skill_{skillData.skillName}_cooldown", Time.time + skillData.cooldown);
                
                success = true;
            }
            else
            {
                Debug.LogWarning($"[UseSkillNode] Not enough mana for {skillData.skillName}");
                success = false;
            }
            
            // 瞬时节点，立即完成
            TriggerSignal();
            StopRunning();
        }
        
        public override void OnExitSignal(string sourceId)
        {
            StopRunning();
        }
    }
}
```

### 示例 2：对话数据节点

#### 1. 定义对话数据

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string speaker;
        [TextArea(2, 4)]
        public string text;
        public float duration;
    }
    
    public string dialogueId;
    public Line[] lines;
}
```

#### 2. 创建对话节点

```csharp
[Serializable, NodeMenuItem("State Machine/ScriptableAsset/Play Dialogue")]
public class PlayDialogueNode : ScriptableAssetNode<DialogueData>
{
    private int _currentLineIndex;
    private float _lineStartTime;
    private bool _isPlaying;
    
    public override string name => "Play Dialogue";
    public override Color color => new Color(0.9f, 0.7f, 0.3f);
    
    public override void OnEnterSignal(string sourceId)
    {
        inputPorts.PullDatas();
        
        if (!TryGetAsset(out var dialogue))
            return;
        
        _currentLineIndex = 0;
        _isPlaying = true;
        ShowCurrentLine(dialogue);
    }
    
    public override void OnUpdate()
    {
        if (!_isPlaying) return;
        
        if (!TryGetAsset(out var dialogue))
        {
            _isPlaying = false;
            StopRunning();
            return;
        }
        
        var currentLine = dialogue.lines[_currentLineIndex];
        
        if (Time.time - _lineStartTime >= currentLine.duration)
        {
            _currentLineIndex++;
            
            if (_currentLineIndex >= dialogue.lines.Length)
            {
                // 对话结束
                _isPlaying = false;
                Debug.Log($"[PlayDialogueNode] Dialogue completed: {dialogue.dialogueId}");
                TriggerSignal();
                StopRunning();
            }
            else
            {
                ShowCurrentLine(dialogue);
            }
        }
    }
    
    private void ShowCurrentLine(DialogueData dialogue)
    {
        var line = dialogue.lines[_currentLineIndex];
        _lineStartTime = Time.time;
        
        Debug.Log($"[Dialogue] {line.speaker}: {line.text}");
        
        // 这里可以触发 UI 显示对话
        StateMachine.Set("dialogue_speaker", line.speaker);
        StateMachine.Set("dialogue_text", line.text);
    }
    
    public override void OnExitSignal(string sourceId)
    {
        _isPlaying = false;
        StopRunning();
    }
}
```

## 数据流与控制流结合

ScriptableAsset Node 的强大之处在于可以将数据端口与控制流结合：

```
┌─────────────────┐
│  LoadGameData   │
│  (asset=Level1) │
└────────┬────────┘
         │ Signal
         ↓
┌─────────────────┐     ┌──────────────┐
│   Condition     │────→│  StartLevel  │
│  (timeLimit>0)  │ Yes │              │
└────────┬────────┘     └──────────────┘
         │ No
         ↓
┌─────────────────┐
│   ShowError     │
│  "Time's up!"   │
└─────────────────┘
```

## 与 Component Node 的对比

| 特性 | Component Node | ScriptableAsset Node |
|------|----------------|----------------------|
| 引用对象 | 场景中的组件 | 项目中的资源 |
| 存储位置 | NGPStateMachine | 节点内部 |
| 运行时修改 | 不建议 | 可以（更换资源） |
| Prefab 支持 | 场景绑定 | 完全支持 |
| 使用场景 | 操作场景对象 | 配置数据 |

## 最佳实践

### 1. 数据分层

```
GameData (Base)
├── LevelData
├── EnemyData
└── ItemData
```

### 2. 使用工厂模式

```csharp
public class EnemyFactory : ScriptableObject
{
    public EnemyData[] templates;
    
    public GameObject CreateEnemy(string enemyId, Vector3 position)
    {
        var template = Array.Find(templates, t => t.id == enemyId);
        if (template != null)
        {
            return Instantiate(template.prefab, position, Quaternion.identity);
        }
        return null;
    }
}
```

### 3. 配置表

使用 ScriptableObject 存储游戏配置表，在状态机中读取：

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    public float playerMoveSpeed = 5f;
    public float jumpForce = 10f;
    public int maxHealth = 100;
    
    [Header("Difficulty Settings")]
    public float easyModeDamageMultiplier = 0.5f;
    public float hardModeDamageMultiplier = 2.0f;
}
```

## 运行时动态资源

如果需要运行时根据条件选择不同的资源：

```csharp
[Serializable, NodeMenuItem("State Machine/ScriptableAsset/Load Dynamic")]
public class LoadDynamicAssetNode : BaseStateNode
{
    [Tooltip("资源路径（Resources 文件夹）")]
    public string resourcePath;
    
    [Tooltip("黑板键名")]
    public string blackboardKey;
    
    public override void OnEnterSignal(string sourceId)
    {
        inputPorts.PullDatas();
        
        var asset = Resources.Load<ScriptableObject>(resourcePath);
        if (asset != null)
        {
            StateMachine.Set(blackboardKey, asset);
            Debug.Log($"[LoadDynamicAssetNode] Loaded {resourcePath}");
        }
        else
        {
            Debug.LogError($"[LoadDynamicAssetNode] Failed to load {resourcePath}");
        }
        
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

## 注意事项

1. **资源路径**：确保 ScriptableObject 资源在项目中可访问
2. **空引用检查**：始终使用 `TryGetAsset()` 检查资源是否存在
3. **资源加载**：大型资源建议使用 Addressables 或 AssetBundle
4. **序列化**：ScriptableObject 的引用会自动序列化到图资源中

## 故障排除

**问题**：资源拖拽到节点后丢失

**解决**：
1. 确保资源在 Assets 文件夹中（不在 Resources 外部）
2. 保存图资源（Ctrl+S）
3. 检查 Console 是否有序列化错误

**问题**：运行时资源为 null

**解决**：
1. 检查节点的 Asset 字段是否赋值
2. 使用 `TryGetAsset()` 而不是直接访问 `asset`
3. 检查资源是否被删除或移动

**问题**：无法拖拽自定义 ScriptableObject

**解决**：
1. 确保类继承自 `ScriptableObject`
2. 确保节点继承自 `ScriptableAssetNode<YourType>`
3. 重新编译脚本


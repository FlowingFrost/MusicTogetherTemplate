# 创建自定义 ScriptableAssetNode

> 扩展 ScriptableAssetNode 实现数据驱动的节点

---

## 概述

`ScriptableAssetNode<T>` 是一个泛型基类，用于创建引用 ScriptableObject 资源的节点。当你需要：

- 使用数据文件驱动游戏逻辑
- 配置化的游戏内容（技能、物品、关卡数据等）
- 在不修改代码的情况下调整游戏参数

可以创建自定义 ScriptableAssetNode。

---

## 基础示例

### 1. 创建 ScriptableObject 数据类

首先定义数据结构：

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game Data/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public float damage;
    public float cooldown;
    public float range;
    public AnimationClip animation;
    public AudioClip soundEffect;
    public GameObject effectPrefab;
}
```

### 2. 创建 ScriptableAssetNode

```csharp
using System;
using UnityEngine;
using GraphProcessor;
using LiteGameFrame.NGPStateMachine;

[Serializable]
[NodeMenuItem("Game/Execute Skill")]
public class ExecuteSkillNode : ScriptableAssetNode<SkillData>
{
    [Output("Damage")]
    public float outputDamage;
    
    [Output("Range")]
    public float outputRange;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (Asset == null)
        {
            Debug.LogError("未设置技能数据！");
            TriggerSignal();
            StopRunning();
            return;
        }
        
        // 读取资源数据
        outputDamage = Asset.damage;
        outputRange = Asset.range;
        
        // 写入黑板供其他节点使用
        StateMachine.Set("skill_damage", Asset.damage);
        StateMachine.Set("skill_range", Asset.range);
        StateMachine.Set("skill_cooldown", Asset.cooldown);
        
        Debug.Log($"执行技能: {Asset.skillName}, 伤害: {Asset.damage}");
        
        // 完成
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

### 3. 在图中使用

1. 创建技能数据资源：`Create > Game Data > Skill Data`
2. 配置技能参数（伤害、冷却等）
3. 在图编辑器中添加 `Game > Execute Skill` 节点
4. 将技能数据资源拖到节点的 `Asset` 字段
5. 连接控制流并运行

---

## 进阶示例

### 加载关卡配置

```csharp
[CreateAssetMenu(fileName = "NewLevelConfig", menuName = "Game Data/Level Config")]
public class LevelConfig : ScriptableObject
{
    public string levelName;
    public int enemyCount;
    public float timeLimit;
    public Vector3 playerStartPos;
    public GameObject[] spawnPrefabs;
}

[Serializable]
[NodeMenuItem("Game/Load Level")]
public class LoadLevelNode : ScriptableAssetNode<LevelConfig>
{
    public bool writeToBlackboard = true;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (Asset == null) return;
        
        if (writeToBlackboard)
        {
            StateMachine.Set("level_name", Asset.levelName);
            StateMachine.Set("enemy_count", Asset.enemyCount);
            StateMachine.Set("time_limit", Asset.timeLimit);
            StateMachine.Set("player_start_pos", Asset.playerStartPos);
        }
        
        Debug.Log($"加载关卡: {Asset.levelName}");
        Debug.Log($"敌人数量: {Asset.enemyCount}, 时间限制: {Asset.timeLimit}s");
        
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

### 角色属性系统

```csharp
[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Game Data/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    public int maxHealth;
    public int maxMana;
    public float moveSpeed;
    public float attackPower;
    public float defense;
    
    [Header("技能列表")]
    public SkillData[] skills;
}

[Serializable]
[NodeMenuItem("Game/Apply Character Stats")]
public class ApplyCharacterStatsNode : ScriptableAssetNode<CharacterStats>
{
    [Input("Target GameObject")]
    public GameObject targetObject;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (Asset == null || targetObject == null) return;
        
        // 应用属性到目标对象
        // 假设目标有 CharacterController 组件
        var controller = targetObject.GetComponent<CharacterController>();
        if (controller != null)
        {
            // controller.SetStats(Asset);
        }
        
        // 写入黑板
        StateMachine.Set("character_name", Asset.characterName);
        StateMachine.Set("max_health", Asset.maxHealth);
        StateMachine.Set("max_mana", Asset.maxMana);
        StateMachine.Set("move_speed", Asset.moveSpeed);
        
        Debug.Log($"应用角色数据: {Asset.characterName}");
        
        TriggerSignal();
        StopRunning();
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

### 对话系统

```csharp
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game Data/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
        public AudioClip voiceClip;
    }
    
    public DialogueLine[] lines;
}

[Serializable]
[NodeMenuItem("Game/Play Dialogue")]
public class PlayDialogueNode : ScriptableAssetNode<DialogueData>
{
    private int currentLine = 0;
    
    public override void OnEnterSignal(string sourceId)
    {
        if (Asset == null || Asset.lines.Length == 0)
        {
            TriggerSignal();
            StopRunning();
            return;
        }
        
        currentLine = 0;
        // 开始播放第一行对话
        PlayLine(currentLine);
    }
    
    public override void OnUpdate()
    {
        // 检测用户输入（例如按空格继续）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentLine++;
            
            if (currentLine < Asset.lines.Length)
            {
                PlayLine(currentLine);
            }
            else
            {
                // 对话结束
                TriggerSignal();
                StopRunning();
            }
        }
    }
    
    private void PlayLine(int index)
    {
        var line = Asset.lines[index];
        Debug.Log($"{line.speaker}: {line.text}");
        
        // 显示 UI、播放语音等
        StateMachine.Set("current_speaker", line.speaker);
        StateMachine.Set("current_text", line.text);
    }
    
    public override void OnExitSignal(string sourceId)
    {
        StopRunning();
    }
}
```

---

## ScriptableAssetNode API

### 核心属性

```csharp
// 引用的资源（自动序列化）
protected T Asset { get; }

// 访问状态机
protected IStateMachine StateMachine { get; }
```

### 核心方法

```csharp
// 触发输出信号
protected void TriggerSignal()

// 停止节点运行
protected void StopRunning()
```

### 生命周期方法

```csharp
// 接收 OnEnter 信号时调用（必须实现）
public abstract void OnEnterSignal(string sourceId);

// 接收 OnExit 信号时调用（必须实现）
public abstract void OnExitSignal(string sourceId);

// 每帧调用（仅当节点活跃时）
public virtual void OnUpdate() { }

// 清理资源
public virtual void Cleanup() { }
```

---

## 最佳实践

### ✅ 推荐做法

1. **数据与逻辑分离**
   - ScriptableObject 只存储数据
   - 节点负责读取和应用数据

2. **提供默认值**
   - 在 ScriptableObject 中设置合理的默认值
   - 防止空引用错误

3. **使用多个资源文件**
   - 每个配置单独一个文件
   - 便于版本控制和团队协作

4. **验证数据完整性**
   ```csharp
   public override void OnEnterSignal(string sourceId)
   {
       if (Asset == null)
       {
           Debug.LogError("未设置资源！");
           TriggerSignal();
           StopRunning();
           return;
       }
       
       // 验证必要字段
       if (string.IsNullOrEmpty(Asset.skillName))
       {
           Debug.LogWarning("技能名称为空");
       }
       
       // ... 执行逻辑
   }
   ```

### ❌ 避免做法

1. **不要在 ScriptableObject 中写业务逻辑**
   - 数据类保持纯数据
   - 逻辑在节点中实现

2. **不要修改 ScriptableObject 实例**
   - 运行时对资源的修改会持久化
   - 使用黑板或临时变量存储运行时数据

3. **不要过度嵌套**
   - 保持数据结构简单明了
   - 复杂关系用引用而非嵌套

---

## 数据驱动设计模式

### 模式 1：配置表驱动

```csharp
[CreateAssetMenu(menuName = "Game Data/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public int waveNumber;
    public EnemySpawnData[] enemies;
    public float timeBetweenSpawns;
}

[Serializable]
public class EnemySpawnData
{
    public GameObject prefab;
    public int count;
    public Vector3 spawnPosition;
}
```

### 模式 2：策略模式

```csharp
[CreateAssetMenu(menuName = "Game Data/AI Behavior")]
public abstract class AIBehaviorData : ScriptableObject
{
    public abstract void Execute(GameObject target);
}

// 具体策略
public class AggressiveBehavior : AIBehaviorData
{
    public override void Execute(GameObject target)
    {
        // 攻击行为
    }
}
```

### 模式 3：组合模式

```csharp
[CreateAssetMenu(menuName = "Game Data/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public EffectData[] effects; // 多个效果组合
}
```

---

## 内置 ScriptableAssetNode 参考

查看源码了解更多实现细节：

- **LoadGameDataNode** (`Runtime/Nodes/ScriptableAssetNodes.cs`)
  - 加载示例游戏数据
  - 演示如何读取和使用 ScriptableObject

---

## 相关文档

- [ScriptableAssetNode 使用指南](../Documentation/ScriptableAssetNode_Usage.md)
- [ScriptableObject 官方文档](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [数据驱动设计模式](https://unity.com/how-to/architect-game-code-scriptable-objects)

---

**提示**：ScriptableObject 非常适合用于游戏配置、关卡设计和内容管理，充分利用它可以大大提高开发效率。

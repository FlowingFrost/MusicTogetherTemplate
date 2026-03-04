# 单例ScriptableObject系统 - 终极简化版

## 🎯 **核心理念**

将单例模式直接内置到ScriptableObject中，实现真正的跨场景配置管理，无需任何GameObject依赖。

## ✨ **关键优势**

### 1. **零场景依赖**
```csharp
// 任何地方直接访问，自动初始化
var config = DeepSeekConfig.Config;
var gameConfig = GameConfig.Config;
```

### 2. **完全静态化**
```csharp
// 不需要Manager组件
DeepSeekConfig.SetApiKey("your-key");
GameConfig.ToggleDebugMode();

// 直接保存和重置
DeepSeekConfig.SaveToJson();
GameConfig.ResetToDefault();
```

### 3. **线程安全初始化**
- 使用双重检查锁定模式
- 懒加载，第一次访问时自动初始化
- 自动DontDestroyOnLoad

### 4. **完整生命周期管理**
```csharp
// 检查初始化状态
if (DeepSeekConfig.IsInitialized) { }

// 强制初始化
DeepSeekConfig.EnsureInitialized();

// 重置到默认状态
DeepSeekConfig.ResetToDefault();
```

## 🏗️ **系统架构**

```
SingletonScriptableObject<T>           # 基础单例类
├── 自动从Resources加载默认配置
├── 可选从JSON文件更新配置（运行时）
├── 线程安全的单例管理
└── 完整的生命周期控制
```

## 📋 **使用方法**

### 1. **直接访问配置**
```csharp
// 获取配置（自动初始化）
var aiConfig = DeepSeekConfig.Config;
var gameConfig = GameConfig.Config;

// 直接修改字段
aiConfig.defaultTemperature = 0.8f;
gameConfig.enableDebugMode = true;
```

### 2. **使用静态便捷方法**
```csharp
// AI配置
DeepSeekConfig.SetApiKey("your-api-key");
DeepSeekConfig.SetDefaultModel("deepseek-coder");
DeepSeekConfig.SetDefaultTemperature(0.7f);

// 游戏配置
GameConfig.SetVolume(0.8f, 0.6f, 0.9f);
GameConfig.ToggleDebugMode();
GameConfig.ApplyGameSettings();
```

### 3. **配置管理**
```csharp
// 保存到JSON
DeepSeekConfig.SaveToJson();
GameConfig.SaveToJson();

// 重置为默认值
DeepSeekConfig.ResetToDefault(); 
GameConfig.ResetToDefault();

// 检查状态
Debug.Log(DeepSeekConfig.GetConfigSummary());
Debug.Log(GameConfig.GetConfigSummary());
```

### 4. **验证和检查**
```csharp
// 检查是否已初始化
if (DeepSeekConfig.IsInitialized)
{
    // 验证配置有效性
    if (DeepSeekConfig.IsValidConfig())
    {
        // 使用配置
    }
}
```

## 🔧 **创建自定义配置**

```csharp
[CreateAssetMenu(fileName = "MyConfig", menuName = "MyGame/My Config")]
public class MyConfig : SingletonScriptableObject<MyConfig>
{
    public string myValue = "default";

    // 静态访问
    public static MyConfig Config => Instance;
    
    // 便捷方法
    public static void SetMyValue(string value)
    {
        if (Instance != null) Instance.myValue = value;
    }
}
```

## 🚀 **性能优势**

1. **懒加载** - 只有访问时才初始化
2. **零开销** - 运行时没有MonoBehaviour组件
3. **内存高效** - 单例模式确保只有一个实例
4. **加载快速** - 启动时一次性加载，后续直接访问

## ⚠️ **注意事项**

1. **Resources文件必须存在** - 系统依赖Resources/Data/目录下的ScriptableObject文件
2. **JSON文件可选** - 如果不存在JSON文件，使用默认配置
3. **线程安全** - 多线程环境下安全访问
4. **编辑器友好** - 在Inspector中修改会反映到运行时
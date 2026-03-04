# Unity 单例服务系统

一个简洁、高效的Unity单例服务系统，支持跨场景持久化和继承设计。

## 🎯 特性

- ✅ **线程安全的单例实现**
- ✅ **跨场景自动持久化**
- ✅ **统一的生命周期管理**
- ✅ **简洁的继承设计，无需接口**
- ✅ **自动服务注册和管理**
- ✅ **延迟初始化，按需创建**
- ✅ **内存安全，防止重复实例**

## 📁 文件结构

```
Services/
├── Core/
│   ├── ServiceBase.cs      # 泛型单例基类
│   └── ServiceManager.cs   # 服务管理器
├── Examples/
│   ├── AudioService.cs     # 音频服务示例
│   └── GameDataService.cs  # 数据服务示例
└── README.md              # 使用说明
```

## 🚀 快速开始

### 1. 创建自定义服务

继承 `ServiceBase<T>` 创建你的服务：

```csharp
using LightGameFrame.Services;

public class MyCustomService : ServiceBase<MyCustomService>
{
    protected override void OnInitialize()
    {
        // 服务初始化逻辑
        Debug.Log("MyCustomService initialized!");
    }

    protected override void OnCleanup()
    {
        // 清理资源
        Debug.Log("MyCustomService cleaned up!");
    }

    // 你的服务方法
    public void DoSomething()
    {
        Debug.Log("Doing something...");
    }
}
```

### 2. 使用服务

```csharp
// 获取服务实例（自动创建）
MyCustomService.Instance.DoSomething();

// 检查服务是否已创建（不会自动创建）
if (MyCustomService.HasInstance)
{
    MyCustomService.Instance.DoSomething();
}

// 安全获取（不会自动创建，可能返回null）
var service = MyCustomService.GetInstanceSafe();
if (service != null)
{
    service.DoSomething();
}
```

## 📖 核心组件详解

### ServiceBase<T>

泛型单例基类，提供以下功能：

#### 重要属性
- `Instance`: 获取服务实例（自动创建）
- `HasInstance`: 检查实例是否存在（布尔值）
- `IsInitialized`: 服务是否已初始化

#### 重要方法
- `GetInstanceSafe()`: 安全获取实例，不触发创建
- `DestroyInstance()`: 强制销毁实例

#### 生命周期方法（可重写）
- `OnAwake()`: Awake时调用
- `OnInitialize()`: 初始化时调用（推荐在此设置服务功能）
- `OnCleanup()`: 销毁时调用（清理资源）

### ServiceManager

服务管理器，自动管理所有服务的生命周期：

#### 主要功能
- 自动注册和管理服务
- 跨场景持久化
- 统一的服务查询接口
- 调试信息显示

#### 常用方法
```csharp
// 获取已注册服务数量
int count = ServiceManager.RegisteredServiceCount;

// 检查特定服务是否已注册
bool isRegistered = ServiceManager.IsServiceRegistered<MyService>();

// 获取特定服务（通过ServiceManager）
var service = ServiceManager.GetService<MyService>();

// 获取调试信息
string debugInfo = ServiceManager.Instance.GetDebugInfo();
```

## 💡 示例服务

### AudioService - 音频管理服务

```csharp
// 播放背景音乐
AudioService.Instance.PlayMusic(musicClip, fadeIn: true);

// 播放音效
AudioService.Instance.PlaySfx(sfxClip);

// 调整音量
AudioService.Instance.MasterVolume = 0.8f;
AudioService.Instance.MusicVolume = 0.6f;

// 停止音乐
AudioService.Instance.StopMusic(fadeOut: true);
```

### GameDataService - 数据管理服务

```csharp
// 设置和获取数据
GameDataService.Instance.SetData("PlayerName", "Alice");
string playerName = GameDataService.Instance.GetData("PlayerName", "Unknown");

// 便捷方法
GameDataService.Instance.SetPlayerLevel(10);
GameDataService.Instance.AddPlayerCoins(100);
int level = GameDataService.Instance.GetPlayerLevel();
int coins = GameDataService.Instance.GetPlayerCoins();

// 手动保存数据
GameDataService.Instance.SaveGameData();
```

## ⚡ 最佳实践

### 1. 初始化顺序
- 服务会在首次访问时自动创建
- `OnInitialize()` 在实例创建后立即调用
- 避免在 `OnInitialize()` 中访问其他可能未初始化的服务

### 2. 内存管理
- 服务会自动在场景切换时保持持久化
- 应用退出时会自动清理所有服务
- 在 `OnCleanup()` 中释放重要资源

### 3. 线程安全
- 实例创建是线程安全的
- 服务方法的线程安全需要自己保证

### 4. 调试技巧
```csharp
// 检查服务状态
Debug.Log($"Service initialized: {MyService.Instance.IsInitialized}");

// 查看所有注册的服务
Debug.Log(ServiceManager.Instance.GetDebugInfo());

// 在场景中可以看到ServiceManager对象
// 所有服务都会作为其子对象显示
```

## 🔧 扩展建议

### 1. 添加服务优先级
可以在ServiceBase中添加Priority属性来控制服务初始化顺序。

### 2. 添加服务依赖
可以添加依赖检查机制，确保依赖服务先初始化。

### 3. 添加配置支持
可以通过ScriptableObject为服务提供配置支持。

### 4. 添加事件系统
可以在服务中集成事件系统，方便服务间通信。

## ❓ 常见问题

### Q: 服务什么时候被创建？
A: 当首次访问 `Instance` 属性时自动创建。

### Q: 服务会在场景切换时被销毁吗？
A: 不会，所有服务都标记为 `DontDestroyOnLoad`，会跨场景保持。

### Q: 如何确保服务的初始化顺序？
A: 可以在需要依赖的服务的 `OnInitialize()` 中先访问依赖服务的 `Instance`。

### Q: 可以在编辑器中预先创建服务对象吗？
A: 可以，系统会自动检测现有实例并使用，避免重复创建。

### Q: 如何在构建时优化服务？
A: 可以使用 `[RuntimeInitializeOnLoadMethod]` 预初始化重要服务。

---

## 💻 使用示例项目

查看 `Examples` 文件夹中的示例服务来了解具体实现方式。每个示例都展示了不同的使用场景和最佳实践。
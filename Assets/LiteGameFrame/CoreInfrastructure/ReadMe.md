# Singleton如何接入
### 变量
- public override bool Persistent => *value* ;
### 函数
- `protected override void Awake()`
  - 从内部注册实例
- `protected override void OnDuplicate(Singleton\<T> duplicate)`
  - 处理多实例冲突
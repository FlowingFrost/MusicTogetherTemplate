# 表达式解析器技术设计文档 - 补充（逻辑运算符）

## 1. 扩展运算符优先级定义

### 1.1 完整优先级层级表
| 优先级 | 运算符 | 描述 | 结合性 |
|--------|---------|------|---------|
//括号
| 1 | `()` | 括号 | 从左到右 |
//一元运算符
| 2 | `!` | 取反 | 无 |
//算数运算符
| 2 | `^` | 指数 | 从右到左 |
| 2 | `*` `/` | 乘除运算 | 从左到右 |
| 3 | `+` | 加运算 | 从左到右 |
| 3 | `-` | 减运算 | 从左到右 |
| 4 | `==` `!=` `<` `>` `<=` `>=` | 比较运算 | 从左到右 |
//逻辑运算符
| 5 | `&&` | 逻辑与 | 从左到右 |
| 6 | `\|\|` | 逻辑或 | 从左到右 |

### 1.2 扩展优先级数值定义
```cpp
enum Precedence {
    PREC_PAREN = 6,    // 括号最高优先级
    PREC_MULT_DIV = 5, // 乘除
    PREC_ADD_SUB = 4,  // 加减
    PREC_COMPARE = 3,  // 比较运算符
    PREC_AND = 2,      // 逻辑与
    PREC_OR = 1,       // 逻辑或
    PREC_LOWEST = 0    // 最低优先级
};
```

## 2. 逻辑运算符类型系统设计

### 2.1 逻辑运算类型兼容性矩阵

#### 2.1.1 乘除法 (`* /`)
| 存在操作数 | 其余操作数 | 结果类型 | 操作语义 |
|----------|----------|----------|----------|
| String | any | 0 | 异常情况 |
| int/float | 强制转换为number(bool) | float | 相加 |
| Boolean | 强制转换为bool | Boolean | 逻辑与 |

#### 2.1.1 加法 (`+`)
| 存在操作数 | 其余操作数 | 结果类型 | 操作语义 |
|----------|----------|----------|----------|
| String | 强制转换为String | String | 将String合并 |
| int/float | 强制转换为number(bool) | float | 相加 |
| Boolean | 强制转换为bool | Boolean | 逻辑或 |

#### 2.1.2 减法 (`-`)
| 存在操作数 | 其余操作数 | 结果类型 | 操作语义 |
|----------|----------|----------|----------|
| String | any | 0 | 异常情况 |
| int/float/bool(转int) | 强制转换为int(bool) | float | 相减 |

#### 2.1.1 相等比较运算 (`==`, `!=`)
| 存在操作数 | 其余操作数 | 操作语义 |
|----------|----------|----------|
| string | 强制转换为string | 字符串相等比较 |
| int/float | 强制转换为int | 数值相等比较 |
| Boolean | 强制转换为bool | 布尔值相等比较 |
| 任意类型 | 任意类型 | 先查看引用然后是类型，然后比较 |

#### 2.1.2 关系比较运算 (`<`, `>`, `<=`, `>=`)
| 存在操作数 | 其余操作数 | 操作语义 |
|----------|----------|----------|
| String | 转换为String | 字典序比较 |
| Number | 转换为Number | 数值大小比较 |
| Boolean | 转换为Boolean | 布尔值比较(false < true) |

#### 2.1.3 逻辑与运算 (`&&`)
| 操作数 | 转换类型 |
|----------|----------|
| Boolean | bool |
| Number | 非零为true，零为false |
| String | 非空为true，空为false |
| 混合类型 | 转换为布尔后逻辑与 |

#### 2.1.4 逻辑或运算 (`||`)
| 操作数 | 转换类型 |
|----------|----------|
| Boolean | bool |
| Number | 非零为true，零为false |
| String | 非空为true，空为false |
| 混合类型 | 转换为布尔后逻辑与 |



## 5. 错误处理扩展

### 5.1 类型不匹配错误（新增）
- 字符串与数值的关系比较
- 不支持的类型参与逻辑运算
- 比较运算符的类型兼容性检查

### 5.2 语法错误（新增）
- 逻辑运算符缺失操作数
- 比较运算符的连续使用（如 `a < b < c`）

## 6. 运算符函数映射表

### 6.1 比较运算符实现
```cpp
// 比较运算符的函数映射
std::unordered_map<TokenType, std::function<bool(Value, Value)>> comparators = {
    {TokenType::Equal, [](Value a, Value b) { return equals(a, b); }},
    {TokenType::NotEqual, [](Value a, Value b) { return !equals(a, b); }},
    {TokenType::Less, [](Value a, Value b) { return lessThan(a, b); }},
    {TokenType::Greater, [](Value a, Value b) { return greaterThan(a, b); }},
    {TokenType::LessEqual, [](Value a, Value b) { return !greaterThan(a, b); }},
    {TokenType::GreaterEqual, [](Value a, Value b) { return !lessThan(a, b); }}
};
```

### 6.2 相等性比较算法
```pseudocode
function equals(Value a, Value b):
    if a.type != b.type:
        return false
    switch a.type:
        case TYPE_NUMBER: return a.numberValue == b.numberValue
        case TYPE_BOOLEAN: return a.boolValue == b.boolValue
        case TYPE_STRING: return a.stringValue == b.stringValue
        default: return false

function lessThan(Value a, Value b):
    if a.type == TYPE_NUMBER and b.type == TYPE_NUMBER:
        return a.numberValue < b.numberValue
    if a.type == TYPE_STRING and b.type == TYPE_STRING:
        return a.stringValue < b.stringValue
    if a.type == TYPE_BOOLEAN and b.type == TYPE_BOOLEAN:
        return a.boolValue < b.boolValue  // false < true
    // 类型不兼容，抛出错误
```

## 7. 测试用例设计

### 7.1 逻辑运算测试用例
```
// 基本逻辑运算
true && false          // false
true || false          // true

// 短路求值
false && (1/0)        // false，不计算除零错误
true || (1/0)         // true，不计算除零错误

// 类型转换
1 && "hello"          // true
0 || ""               // false
```

### 7.2 比较运算测试用例
```
// 数值比较
5 > 3                 // true
"abc" < "def"         // true

// 混合类型
1 == true             // true (1 == 1)
0 == false            // true (0 == 0)
"1" == 1              // false (类型不同)
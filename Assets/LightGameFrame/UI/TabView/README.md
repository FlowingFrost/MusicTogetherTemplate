# TabView (UI Toolkit)

这是一个不需要手动挂脚本的 Tab 组件：在 UXML 里设置属性即可自动生成 Tab、切换内容区，并支持可选关闭按钮与选中/未选中样式。

## 使用方式

1. 在 UI Builder 中打开 `TabViewExample.uxml` 或你的 UXML。
2. 把 `TabView.uss` 绑定到根节点（或在 UI Builder 的 StyleSheets 面板里添加）。
3. 选择以下任一方式创建 Tab。

### 方式一：直接添加内容容器（推荐）

在 `TabView` 下直接添加子元素，按顺序自动分配到不同的 Tab 面板内容区。

```xml
<lgf:TabView name="sample-tabview" tab-titles="主页,设置" allow-none-selected="false">
    <ui:VisualElement>
        <ui:Label text="主页内容" />
    </ui:VisualElement>
    <ui:VisualElement>
        <ui:Label text="设置内容" />
    </ui:VisualElement>
</lgf:TabView>
```

### 方式二：TabPage 插槽

在 UXML 中声明 `TabPage`，可直接在 UI Builder 中编辑子内容，并支持自定义标题/关闭/选中。

```xml
<lgf:TabView name="sample-tabview" allow-none-selected="false">
    <lgf:TabPage title="主页" selected="true">
        <ui:Label text="主页内容" />
    </lgf:TabPage>
    <lgf:TabPage title="设置" closable="true">
        <ui:Label text="设置内容" />
    </lgf:TabPage>
</lgf:TabView>
```

### 方式二：属性自动生成

只填写数量与标题，由组件自动生成空面板。

```xml
<lgf:TabView
    name="sample-tabview"
    tab-count="3"
    tab-titles="主页,设置,日志"
    closable-tabs="2,3"
    selected-index="0" />
```

> 提示：UI Builder 对动态子元素刷新不一定实时，修改 `tab-count` 后可尝试重新打开 UXML 或触发一次刷新。

## 属性说明

- `tab-count`：Tab 数量（默认 3）。
- `tab-titles`：Tab 标题，逗号分隔（可少于 `tab-count`）。
- `closable`：全部 Tab 显示关闭按钮（默认 false）。
- `closable-tabs`：指定可关闭 Tab 的序号（从 1 开始），逗号分隔。
- `selected-index`：初始选中的 Tab（从 0 开始，默认 0）。
- `allow-none-selected`：允许无选中（默认 false）。

## 内容区说明

每个 Tab 都会生成内容面板 `tab-panel-1`、`tab-panel-2`……
面板内部包含实际可承载子元素的容器：`tab-panel-1-content`、`tab-panel-2-content`……

如果使用直接添加方式，每个子元素会按顺序移动到对应的 `tab-panel-x-content` 容器中。

如果使用 `TabPage` 插槽方式，`TabPage` 的子元素会自动移动到对应的 `tab-panel-x-content` 容器中。

如果需要在运行时填充内容，可通过脚本调用：

- `GetContentPanel(index)` 获取指定面板的内容容器（`index` 从 0 开始）。

## 文件列表

- `TabView.cs`：自定义 UI Toolkit 元素与自动生成逻辑。
- `TabView.uss`：默认样式（选中/未选中、关闭按钮样式）。
- `TabViewExample.uxml`：可直接打开的示例布局。

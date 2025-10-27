# FolderSelector 设计模式对比

## 当前设计：Reusable EditorGUI Component

### 特征
- 普通 C# 类
- 维护状态字段（`_currentPath`, `_prefsKey` 等）
- 提供 GUI 绘制方法（`DrawGUI()`, `DrawAsPropertyField()` 等）
- 在其他 Editor 中被实例化和使用

### 使用方式
```csharp
public class MyEditor : Editor
{
    private FolderSelector _folderSelector;
    
    private void OnEnable()
    {
        _folderSelector = new FolderSelector("MyKey", "Assets");
    }
    
    public override void OnInspectorGUI()
    {
        _folderSelector.DrawGUI(); // 直接嵌入到 Inspector 中
    }
}
```

### 优点 ✅
1. **简洁** - `new FolderSelector()` 即可使用
2. **可嵌入** - 可以在任何 CustomEditor 中使用
3. **状态持久** - 通过 EditorPrefs 保存
4. **用户体验好** - 直接在 Inspector 中操作，无需打开新窗口
5. **灵活** - 可以在一个 Editor 中使用多个 FolderSelector
6. **轻量** - 不需要窗口管理的开销

### 缺点 ❌
1. 无法独立显示（但这不是缺点，因为不需要）

---

## 替代设计：EditorWindow

### 特征
- 继承自 `EditorWindow`
- 独立的窗口，有自己的生命周期
- 通过 `EditorWindow.GetWindow<T>()` 显示

### 使用方式
```csharp
public class FolderSelectorWindow : EditorWindow
{
    private string _currentPath;
    
    [MenuItem("Tools/Folder Selector")]
    public static void ShowWindow()
    {
        GetWindow<FolderSelectorWindow>("Folder Selector");
    }
    
    private void OnGUI()
    {
        // 绘制 GUI
    }
}

// 在其他地方使用
public class MyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("选择文件夹"))
        {
            FolderSelectorWindow.ShowWindow(); // 打开一个新窗口
        }
    }
}
```

### 优点 ✅
1. **独立显示** - 可以作为独立的工具窗口
2. **可停靠** - 可以停靠到 Unity Editor 的任何位置
3. **持久化** - 窗口状态可以在 Unity 重启后保留

### 缺点 ❌
1. **使用复杂** - 需要 `GetWindow<T>().Show()`
2. **无法嵌入** - 无法直接嵌入到 Inspector 中（这是最大的问题！）
3. **用户体验差** - 每次选择文件夹都要打开一个新窗口
4. **状态管理复杂** - 需要处理窗口关闭、重新打开等情况
5. **不灵活** - 难以在一个 Editor 中使用多个实例

---

## Unity 官方的类似例子

### 1. `ReorderableList` - GUI Component ✅

```csharp
// Unity 的 ReorderableList 也是一个 GUI Component，不是 EditorWindow
public class MyEditor : Editor
{
    private ReorderableList _list;
    
    private void OnEnable()
    {
        _list = new ReorderableList(serializedObject, serializedObject.FindProperty("items"));
    }
    
    public override void OnInspectorGUI()
    {
        _list.DoLayoutList(); // 直接嵌入到 Inspector 中
    }
}
```

### 2. `ColorField` - EditorGUI 静态方法 ✅

```csharp
// Unity 的 ColorField 是一个静态方法，可以直接调用
public override void OnInspectorGUI()
{
    Color color = EditorGUILayout.ColorField("Color", Color.white);
}
```

### 3. `ColorPicker` - EditorWindow ❌

```csharp
// Unity 的 ColorPicker 是一个 EditorWindow，用于独立的颜色选择窗口
ColorPicker.Show(GUIView.current, color, showAlpha, hdr);
```

**注意**：Unity 的 `ColorPicker` 是 EditorWindow，但它是一个**独立的工具窗口**，不是嵌入式组件。

---

## 设计模式分析

### FolderSelector 使用的设计模式

1. **Component Pattern（组件模式）**
   - 将 GUI 功能封装成可复用的组件
   - 可以在多个地方使用

2. **Facade Pattern（门面模式）**
   - 提供简单的 API（`DrawGUI()`）隐藏复杂的实现细节
   - 用户无需关心拖拽、点击、路径验证等细节

3. **Strategy Pattern（策略模式）**
   - 提供多种绘制方式（`DrawGUI()`, `DrawAsPropertyField()`, `DrawDragArea()`）
   - 用户可以根据需要选择不同的绘制方式

---

## 何时使用 EditorWindow？

### 适合使用 EditorWindow 的场景 ✅

1. **独立的工具窗口**
   - 例如：Profiler、Animation、Lighting 等
   - 需要独立显示，可以停靠

2. **复杂的编辑器**
   - 例如：Terrain Editor、Particle System Editor
   - 需要大量的 GUI 空间

3. **持久化的工具**
   - 例如：Console、Hierarchy、Project 窗口
   - 需要在 Unity 重启后保留状态

### 不适合使用 EditorWindow 的场景 ❌

1. **嵌入式 GUI 组件**
   - 例如：FolderSelector、ColorPicker、PropertyField
   - 需要在 Inspector 中直接使用

2. **可复用的 GUI 元素**
   - 例如：ReorderableList、Foldout、HelpBox
   - 需要在多个 Editor 中使用

3. **简单的 GUI 控件**
   - 例如：Button、TextField、Slider
   - 不需要独立的窗口

---

## 命名建议

根据功能和用途，可以使用以下命名规范：

| 功能 | 命名后缀 | 示例 |
|------|---------|------|
| 选择器 | `Selector` | `FolderSelector`, `AssetSelector` |
| 拾取器 | `Picker` | `ColorPicker`, `ObjectPicker` |
| 编辑器 | `Editor` | `PropertyEditor`, `CurveEditor` |
| 绘制器 | `Drawer` | `PropertyDrawer`, `DecoratorDrawer` |
| 控件 | `Control` | `SliderControl`, `ToggleControl` |
| 组件 | `Component` | `GUIComponent`, `EditorComponent` |

**FolderSelector** 的命名是合适的，因为它是一个"文件夹选择器"。

---

## 总结

### FolderSelector 的当前设计是 **完全正确** 的！✅

1. **术语**：Reusable EditorGUI Component（可复用的编辑器 GUI 组件）
2. **设计模式**：Component Pattern + Facade Pattern + Strategy Pattern
3. **不应该改成 EditorWindow**：因为它需要嵌入到 Inspector 中使用
4. **类似的 Unity 官方例子**：`ReorderableList`, `PropertyField`, `ColorField`

### 关键区别

| 特性 | EditorGUI Component | EditorWindow |
|------|---------------------|--------------|
| 显示方式 | 嵌入到其他 Editor 中 | 独立的窗口 |
| 使用方式 | `new Component()` | `GetWindow<T>()` |
| 生命周期 | 由父 Editor 管理 | 独立管理 |
| 适用场景 | 可复用的 GUI 元素 | 独立的工具窗口 |
| 示例 | `ReorderableList`, `FolderSelector` | `Profiler`, `Animation` |

---

## 参考资料

- [Unity Manual: Editor Windows](https://docs.unity3d.com/Manual/editor-EditorWindows.html)
- [Unity Manual: Custom Editors](https://docs.unity3d.com/Manual/editor-CustomEditors.html)
- [Unity Scripting API: EditorGUILayout](https://docs.unity3d.com/ScriptReference/EditorGUILayout.html)
- [Unity Scripting API: ReorderableList](https://docs.unity3d.com/ScriptReference/UnityEditorInternal.ReorderableList.html)


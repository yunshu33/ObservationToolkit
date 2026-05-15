# ObservationToolkit 中文说明

ObservationToolkit 是一个用于 Unity 的轻量 MVVM / 数据绑定工具包。它提供可观察数据模型、链式绑定 API、UGUI 常用控件绑定、Command 命令绑定，以及基于 IL Post Processor 的属性通知自动织入能力。

项目适合用在希望把 UI 更新逻辑从 MonoBehaviour 中拆出来的场景，例如面板状态同步、表单输入、设置页、背包/属性面板等。

## 功能概览

- 可观察对象：通过 `IObservable`、`BindingHandler` 和 `SetField` 触发属性变更通知。
- 链式绑定：使用 `model.For(m => m.Value).To(...).OneWay()` 建立单向绑定。
- UGUI 绑定：支持 `Text`、`InputField`、`Toggle`、`Slider`、`Dropdown`、`Image`、`RawImage`。
- 双向绑定：支持从 UI 事件回写到 ViewModel，例如 `InputField.onValueChanged`。
- 生命周期管理：通过 `.AddTo(this)`、`.AddTo(gameObject)` 或 `BindingContext` 自动解绑。
- Command：提供 `ICommand`、`RelayCommand` 和 Button 命令绑定。
- IL 自动织入：通过 `[Observation]`、`[IgnoreObservation]` 和编辑器配置，对指定程序集注入属性通知代码。

## 目录结构

```text
ObservationToolkit/
├── Runtime/      # 运行时代码：绑定核心、ViewModel、UGUI Binder、Command、Converter
├── CodeGen/      # IL Post Processor：属性通知和 ViewModel 代理织入
├── Editor/       # 编辑器窗口：Observation Weaver 配置界面
├── Plugins/      # Mono.Cecil 相关依赖
└── Sample/       # 示例场景和示例脚本
```

## 快速开始

1. 将 `ObservationToolkit` 放到 Unity 项目的 `Assets` 目录下。
2. 打开菜单 `Voyage/Observation Settings`。
3. 开启 `Enable Weaver`。
4. 勾选 `Assembly-CSharp`，或将自己的 asmdef 程序集添加到列表中。
5. 编写实现 `IObservable` 的 Model / ViewModel。
6. 在 UI 脚本中使用链式 API 建立绑定。

## 基础 Model 写法

手动写法是最直接、最稳定的方式：字段变化时调用 `SetField`，它会在值真正变化后触发绑定通知。

```csharp
using System;
using UnityEngine;
using Voyage.ObservationToolkit.Runtime;

[Serializable]
public class PlayerModel : IObservable
{
    public BindingHandler BindingHandler { get; set; }

    [SerializeField] private int _score;

    public int Score
    {
        get => _score;
        set => this.SetField(ref _score, value);
    }
}
```

绑定到普通回调：

```csharp
model.For(m => m.Score)
    .To(value => scoreText.text = value.ToString())
    .OneWay()
    .AddTo(this);
```

也可以接收源对象和属性值：

```csharp
model.For(m => m.Score)
    .To((source, value) => Debug.Log($"{source}: {value}"))
    .OneWay()
    .AddTo(this);
```

## ViewModel 写法

`ViewModel<TData>` 用于包装一份可序列化数据。属性可以代理到 `Data` 中的字段。

```csharp
using System;
using UnityEngine;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.ViewModel;

[Serializable]
public class PlayerData
{
    public float hp;
    public bool isAlive;
}

[Serializable]
public class PlayerViewModel : ViewModel<PlayerData>
{
    public float Hp
    {
        get => Data.hp;
        set => this.SetField(ref Data.hp, value);
    }

    public bool IsAlive
    {
        get => Data.isAlive;
        set => this.SetField(ref Data.isAlive, value);
    }
}
```

如果开启 IL Weaver，也可以使用 `[Observation]` 标记属性或类，让编译后处理器自动注入通知逻辑。

```csharp
[Observation]
public bool IsOn { get; set; }
```

规则说明：

- `[Observation]` 标记在属性上：只处理该属性。
- `[Observation]` 标记在类上：默认处理类中的属性。
- `[IgnoreObservation]`：跳过指定属性。
- `BindingHandler` 和 `Data` 已被标记为忽略观察。

## UGUI 绑定

引入命名空间：

```csharp
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.UGUI;
```

常用控件示例：

```csharp
// Text：单向绑定，数值会转换为字符串
model.For(m => m.Value)
    .To(text)
    .OneWay()
    .AddTo(this);

// InputField：双向绑定，UI 输入会回写到 model.Value
model.For(m => m.Value)
    .To(inputField)
    .TwoWay(input => input.onValueChanged)
    .AddTo(this);

// Toggle：双向绑定
model.For(m => m.IsOn)
    .To(toggle)
    .TwoWay(t => t.onValueChanged)
    .AddTo(this);

// Slider：双向绑定
model.For(m => m.Value)
    .To(slider)
    .TwoWay(s => s.onValueChanged)
    .AddTo(this);

// Dropdown：双向绑定
model.For(m => m.Index)
    .To(dropdown)
    .TwoWay(d => d.onValueChanged)
    .AddTo(this);

// Image / RawImage：单向绑定资源
model.For(m => m.Icon)
    .To(image)
    .OneWay()
    .AddTo(this);

model.For(m => m.Texture)
    .To(rawImage)
    .OneWay()
    .AddTo(this);
```

当前内置 UGUI Binder：

| 控件 | 绑定方向 | 属性类型 |
| --- | --- | --- |
| `Text` | 单向 | `string` |
| `InputField` | 单向 / 双向 | `string` |
| `Toggle` | 单向 / 双向 | `bool` |
| `Slider` | 单向 / 双向 | `float` |
| `Dropdown` | 单向 / 双向 | `int` |
| `Image` | 单向 | `Sprite` |
| `RawImage` | 单向 | `Texture` |

## 自定义转换器

当 Model 属性类型和 UI 目标类型不一致，或者需要自定义显示格式时，可以传入转换器。转换器既可以用匿名函数快速声明，也可以实现 `IConvert<TSource, TTarget>` 方便复用。

### 匿名函数写法

单向绑定时，只需要提供 `Model -> UI` 的转换函数。

```csharp
// float -> string，保留 1 位小数并追加单位
model.For(m => m.Value)
    .To(text)
    .OneWay(value => $"{value:0.0} kg")
    .AddTo(this);
```

双向绑定时，需要提供 `Model -> UI` 和 `UI -> Model` 两个方向的转换函数。

```csharp
// float <-> string
model.For(m => m.Value)
    .To(inputField)
    .TwoWay(
        input => input.onValueChanged,
        value => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
        textValue => float.TryParse(
            textValue,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var result)
                ? result
                : 0f)
    .AddTo(this);
```

### 委托转换器写法

如果同一组匿名函数需要在多个绑定中复用，可以使用 `DelegateConvert<TSource, TTarget>`。

```csharp
using Voyage.ObservationToolkit.Runtime.Converter;

IConvert<float, string> percentTextConvert = new DelegateConvert<float, string>(
    sourceToTarget: value => $"{value:P0}",
    targetToSource: textValue =>
    {
        textValue = textValue.Replace("%", "");
        return float.TryParse(textValue, out var percent) ? percent / 100f : 0f;
    });

model.For(m => m.Value)
    .To(text, percentTextConvert)
    .OneWay()
    .AddTo(this);

model.For(m => m.Value)
    .To(inputField, percentTextConvert)
    .TwoWay(input => input.onValueChanged)
    .AddTo(this);
```

### 自定义转换类

复杂转换逻辑建议写成独立类，实现 `IConvert<TSource, TTarget>`，或者继承 `ConvertBase<TSource, TTarget>`。继承 `ConvertBase` 时通常只需要实现正向和反向转换。

```csharp
using System.Globalization;
using Voyage.ObservationToolkit.Runtime.Converter;

public sealed class FloatPercentConvert : ConvertBase<float, string>
{
    public override string SourceConvertTarget(float source)
    {
        return source.ToString("P0", CultureInfo.InvariantCulture);
    }

    public override float TargetConvertSource(string target)
    {
        target = target.Replace("%", "");
        return float.TryParse(
            target,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var value)
                ? value / 100f
                : 0f;
    }
}
```

使用时推荐按接口保存，便于替换实现或在多个 UI 绑定中复用。

```csharp
IConvert<float, string> percentConvert = new FloatPercentConvert();

model.For(m => m.Value)
    .To(text, percentConvert)
    .OneWay()
    .AddTo(this);

model.For(m => m.Value)
    .To(inputField, percentConvert)
    .TwoWay(input => input.onValueChanged)
    .AddTo(this);
```

Slider 这类 UI 值固定为 `float` 的控件，也可以传入接口转换器来处理 `int <-> float` 或特殊映射。

```csharp
IConvert<int, float> sliderConvert = new DelegateConvert<int, float>(
    sourceToTarget: value => value,
    targetToSource: value => ConversionUtility.ToInt(value));

model.For(m => m.Index)
    .To(slider, sliderConvert)
    .TwoWay(s => s.onValueChanged)
    .AddTo(this);
```

## Command 绑定

`RelayCommand` 可用于把 Button 点击行为绑定到 ViewModel。

```csharp
using Voyage.ObservationToolkit.Runtime.Command;

private ICommand _addCommand;

public ICommand AddCommand => _addCommand ??= new RelayCommand(
    execute: () => Value++,
    canExecute: () => Value < 100
);
```

绑定 Button：

```csharp
button.BindCommand(model, m => m.AddCommand);
```

当 `CanExecute` 条件变化时，调用：

```csharp
_addCommand?.RaiseCanExecuteChanged();
```

Button 的 `interactable` 会跟随命令可执行状态变化。

## 生命周期管理

推荐所有绑定都挂到一个生命周期上，避免对象销毁后仍然持有回调。

```csharp
model.For(m => m.Value)
    .To(text)
    .OneWay()
    .AddTo(this);
```

`.AddTo(this)` 会在当前组件所在的 `GameObject` 上自动添加 `BindingLifecycleBehavior`，并在 `OnDestroy` 时统一解绑。

也可以手动使用 `BindingContext`：

```csharp
private readonly BindingContext _bindingContext = new();

private void Start()
{
    model.For(m => m.Value)
        .To(UpdateValue)
        .OneWay()
        .AddTo(_bindingContext);
}

private void OnDestroy()
{
    _bindingContext.Dispose();
}
```

## 性能实现细节

ObservationToolkit 的绑定 API 使用表达式、反射和委托来换取更好的调用体验。为了避免这些机制在高频绑定场景里反复产生额外开销，运行时对两个关键位置做了缓存：属性表达式解析缓存，以及双向绑定 setter 全局缓存。

### 表达式到属性名缓存

用户创建绑定时通常会写：

```csharp
model.For(m => m.Value)
    .To(text)
    .OneWay()
    .AddTo(this);
```

这里的 `m => m.Value` 是表达式树。绑定系统需要从表达式树里解析出属性名 `Value`，再用这个名字把绑定注册到 `BindingHandler` 的字典中。旧实现每次调用 `For` 都会重新解析表达式：

```csharp
if (propertyExpression.Body is not MemberExpression memberExpression ||
    memberExpression.Member.MemberType != MemberTypes.Property)
{
    throw new ArgumentException("属性指定错误，必须为属性表达式。");
}

var propertyName = memberExpression.Member.Name;
```

现在 `BindingHandler` 内部增加了全局缓存：

```csharp
private static readonly Dictionary<string, string> PropertyNameCache = new();
private static readonly object PropertyNameCacheLock = new();
```

解析流程变成：

```csharp
private static string GetPropertyName<S, SProperty>(
    Expression<Func<S, SProperty>> propertyExpression)
{
    var cacheKey = $"{typeof(S).FullName}:{propertyExpression.Body}";

    lock (PropertyNameCacheLock)
    {
        if (PropertyNameCache.TryGetValue(cacheKey, out var cachedName))
        {
            return cachedName;
        }
    }

    // 第一次遇到这个表达式时才解析表达式树。
    var propertyName = memberExpression.Member.Name;

    lock (PropertyNameCacheLock)
    {
        PropertyNameCache[cacheKey] = propertyName;
    }

    return propertyName;
}
```

这个缓存的使用者不需要写额外代码，仍然按原来的方式创建绑定即可：

```csharp
model.For(m => m.Value).To(text).OneWay();
model.For(m => m.Value).To(slider).TwoWay(s => s.onValueChanged);
model.For(m => m.Value).To(inputField).TwoWay(i => i.onValueChanged);
```

第一次解析 `m => m.Value` 时会写入缓存，后面相同类型和相同属性表达式再次绑定时会直接复用属性名。它优化的是“建立绑定”的成本，不改变属性变化后的分发逻辑。

注意事项：

- 表达式必须直接指向属性，例如 `m => m.Value`。
- 不支持字段、方法调用或复杂表达式，例如 `m => m.Value + 1`。
- 缓存 key 包含源类型和表达式文本，避免不同 ViewModel 上同名属性互相干扰。

### 双向绑定 setter 全局缓存

双向绑定需要把 UI 的值回写到 ViewModel：

```csharp
model.For(m => m.Value)
    .To(slider)
    .TwoWay(s => s.onValueChanged)
    .AddTo(this);
```

当 `Slider.onValueChanged` 触发时，绑定系统会把 `float` 转成模型属性类型，然后写回 `model.Value`。直接用反射 `PropertyInfo.SetValue` 会在频繁 UI 事件中产生额外开销，所以 `TwoWayUGUIBinderBase` 会在绑定建立时生成 setter 委托。

进一步优化后，setter 委托被提升为全局缓存：

```csharp
private static readonly Dictionary<string, Action<object, SProperty>> SetterCache = new();
private static readonly object SetterCacheLock = new();
```

缓存 key 由三部分组成：

```text
源对象真实类型 | 属性名 | 源属性类型
```

例如同一个 `PlayerViewModel.Value` 被绑定到多个 UI 控件时，只需要编译一次 setter：

```csharp
model.For(m => m.Value).To(inputField).TwoWay(i => i.onValueChanged);
model.For(m => m.Value).To(slider).TwoWay(s => s.onValueChanged);
```

内部创建 setter 的流程是：

```csharp
protected UnityAction<UProperty> CreateSetter()
{
    var source = _binding.Source;
    var property = source.GetType().GetProperty(_binding.PropertyName, flags);

    var setter = GetOrCreateSetter(source.GetType(), property);

    return value => setter(source, TargetConvertSource(value));
}
```

真正缓存的是 open setter：

```csharp
private static Action<object, SProperty> BuildSetter(Type sourceType, PropertyInfo property)
{
    var sourceParam = Expression.Parameter(typeof(object), "source");
    var valueParam = Expression.Parameter(typeof(SProperty), "value");

    var typedSource = Expression.Convert(sourceParam, sourceType);
    var body = Expression.Assign(
        Expression.Property(typedSource, property),
        valueParam);

    return Expression
        .Lambda<Action<object, SProperty>>(body, sourceParam, valueParam)
        .Compile();
}
```

`CreateSetter` 返回给 UI 事件的闭包仍然保留当前绑定实例，因为每个绑定可能有不同的转换器：

```csharp
return value => setter(source, TargetConvertSource(value));
```

这意味着：

- setter 委托可以跨同类型、同属性的多个绑定复用。
- `TargetConvertSource` 仍然按当前绑定实例执行，支持默认转换、自定义函数转换器和 `IConvert<TSource, TTarget>`。
- Slider 的 `float -> int`、`int -> float` 等特殊转换仍然在写回前处理。

使用方式不变：

```csharp
// 默认转换。
model.For(m => m.Value)
    .To(slider)
    .TwoWay(s => s.onValueChanged)
    .AddTo(this);

// 自定义转换函数。
model.For(m => m.Value)
    .To(inputField)
    .TwoWay(
        i => i.onValueChanged,
        value => value.ToString("0.0"),
        text => float.TryParse(text, out var result) ? result : 0f)
    .AddTo(this);

// 自定义转换器接口。
model.For(m => m.Index)
    .To(slider, sliderConvert)
    .TwoWay(s => s.onValueChanged)
    .AddTo(this);
```

这个缓存优化的是“UI 事件回写模型”的路径，尤其适合多个页面或列表项中大量重复绑定同类 ViewModel 属性的场景。

## IL Weaver 配置

编辑器菜单：

```text
Voyage/Observation Settings
```

配置会保存到：

```text
ProjectSettings/ObservationWeaverSettings.json
```

配置项：

- `enableWeaver`：是否启用 IL 织入。
- `weaveAssemblyCSharp`：是否处理 `Assembly-CSharp`。
- `extraAssemblies`：额外处理的 asmdef 程序集名称。

默认情况下，CodeGen 中的 `ObservationWeaver` 会在编译后扫描目标程序集，并执行两类处理：

- `PropertyWeaver`：对被 `[Observation]` 命中的自动属性或可识别 backing field 的属性注入 `SetField`。
- `ViewModelWeaver`：对继承 `ViewModel<TData>` 的类型，将属性代理到 `Data` 中同名字段，并在 setter 中注入通知。

## 示例

项目内置示例位于：

```text
Sample/
├── Scene/ObservationToolkit.unity
├── Scene/UGUI.unity
└── Scripts/
```

重点脚本：

- `Sample/Scripts/Test.cs`：基础 Model、ViewModel、Command 示例。
- `Sample/Scripts/UGUI/UGUITest.cs`：Text、InputField、Toggle、Slider、Dropdown、Image、RawImage、Button Command 的绑定示例。

## 注意事项

- 绑定表达式必须指向属性，例如 `m => m.Value`，不能传字段或复杂表达式。
- 双向绑定依赖目标属性有可写 setter，否则无法从 UI 回写。
- `InputField`、`Text` 等字符串转换默认使用 `InvariantCulture`，适合避免不同系统区域设置导致的解析差异。
- `OneWay()` 只会在属性变化后推送值；如果需要初始同步，请在绑定后手动刷新属性，或在业务初始化阶段设置一次值。
- IL Weaver 只会处理配置中启用的程序集。使用 asmdef 时，需要把程序集名称加入 `extraAssemblies`。
- 当前项目包含 Mono.Cecil 插件依赖，请保留 `Plugins/` 目录。

## 一个完整示例

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;
using Voyage.ObservationToolkit.Runtime.UGUI;
using Voyage.ObservationToolkit.Runtime.ViewModel;

[Serializable]
public class CounterData
{
    public float value;
}

[Serializable]
public class CounterViewModel : ViewModel<CounterData>
{
    public float Value
    {
        get => Data.value;
        set
        {
            if (this.SetField(ref Data.value, value))
            {
                _addCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new RelayCommand(
        () => Value++,
        () => Value < 100
    );
}

public class CounterPanel : MonoBehaviour
{
    public CounterViewModel model;
    public Text valueText;
    public Slider valueSlider;
    public Button addButton;

    private void Start()
    {
        model.For(m => m.Value)
            .To(valueText)
            .OneWay()
            .AddTo(this);

        model.For(m => m.Value)
            .To(valueSlider)
            .TwoWay(s => s.onValueChanged)
            .AddTo(this);

        addButton.BindCommand(model, m => m.AddCommand);
    }
}
```

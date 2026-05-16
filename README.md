# ObservationToolkit 中文说明

ObservationToolkit 是一个用于 Unity 的轻量 MVVM / 数据绑定工具包。它提供可观察数据模型、链式绑定 API、UGUI 常用控件绑定、Command 命令绑定，以及基于 IL Post Processor 的属性通知自动织入能力。

项目适合用在希望把 UI 更新逻辑从 MonoBehaviour 中拆出来的场景，例如面板状态同步、表单输入、设置页、背包/属性面板等。

## 功能概览

- 可观察对象：通过 `IObservable`、`BindingHandler` 和 `SetField` 触发属性变更通知。
- 链式绑定：使用 `model.For(m => m.Value).To(...).OneWay()` 建立单向绑定。
- UGUI 绑定：支持 `Text`、`InputField`、`Toggle`、`Slider`、`Dropdown`、`Image`、`RawImage`。
- 双向绑定：支持从 UI 事件回写到 ViewModel，例如 `InputField.onValueChanged`。
- 生命周期管理：通过 `.AddTo(this)`、`.AddTo(gameObject)` 或 `BindingContext` 自动解绑。
- Command：提供 `ICommand`、`RelayCommand`、Button 命令绑定，以及 `.Observes(...)` 自动刷新 `CanExecute` 状态。
- IL 自动织入：通过 `[Observation]`、`[IgnoreObservation]` 和编辑器配置，对指定程序集注入属性通知代码。

## 目录结构

```text
ObservationToolkit/
├── Runtime/      # 运行时代码：绑定核心、ViewModel、UGUI Binder、Command、Converter
├── CodeGen/      # IL Post Processor：属性通知和 ViewModel 代理织入
├── Editor/       # 编辑器窗口：Observation Weaver 配置界面
├── Plugins/      # Mono.Cecil 相关依赖
├── Sample/       # 示例场景和示例脚本
├── Tests/        # EditMode 测试
├── package.json  # UPM 包元信息
├── CHANGELOG.md  # 版本变更记录
└── LICENSE.md    # 许可证
```

## 快速开始

1. 将 `ObservationToolkit` 放到 Unity 项目的 `Assets` 目录下，或通过 `package.json` 作为本地 UPM 包引用。
2. 打开菜单 `VoyageForge/Observation Settings`。
3. 开启 `Enable Weaver`。
4. 勾选 `Assembly-CSharp`，或将自己的 asmdef 程序集添加到列表中。
5. 编写实现 `IObservable` 的 Model / ViewModel。
6. 在 UI 脚本中使用链式 API 建立绑定。

## 命名空间与程序集

工作室命名统一为 VoyageForge 后，公开 API 使用以下命名空间：

```csharp
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using VoyageForge.ObservationToolkit.Runtime.UGUI;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;
```

主要程序集：

| 程序集 | 用途 |
| --- | --- |
| `VoyageForge.ObservationToolkit.Runtime` | 运行时绑定、UGUI、Command、Converter、ViewModel |
| `VoyageForge.ObservationToolkit.Editor` | 编辑器窗口和 Inspector 支持 |
| `Unity.VoyageForge.ObservationToolkit.CodeGen` | IL Post Processor 织入逻辑。程序集名前缀保留 `Unity.`，用于兼容 Unity ILPP 编译管线 |
| `VoyageForge.ObservationToolkit.Sample` | 示例脚本 |
| `VoyageForge.ObservationToolkit.Tests.EditMode` | EditMode 测试 |

## 测试

EditMode 测试位于 `Tests/EditMode`：

- `CommandTests.cs`：覆盖 Command 观察、参数转换、Button 绑定和 UnityEvent 绑定。
- `AutoWeaveTests.cs`：覆盖 Sample 程序集中的 `[Observation]` 自动属性是否真的被静态织入。
- `PerformanceTests.cs`：轻量性能基线，覆盖绑定通知分发、Command `.Observes(...)` 扇出刷新和参数命令转换。

性能测试当前使用 NUnit + `Stopwatch`，不依赖 `com.unity.test-framework.performance`。阈值设置得较宽，主要用于捕获核心路径的明显性能回归。

## 基础 Model 写法

手动写法是最直接、最稳定的方式：字段变化时调用 `SetField`，它会在值真正变化后触发绑定通知。

```csharp
using System;
using UnityEngine;
using VoyageForge.ObservationToolkit.Runtime;

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
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;

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

普通模型可以用类级 `[Observation]` 减少重复标记：

```csharp
[Serializable]
[Observation]
public class AutoWeavePlayerModel : IObservable
{
    public BindingHandler BindingHandler { get; set; }

    public int Score { get; set; }
    public int Level { get; set; } = 1;

    [IgnoreObservation]
    public int RuntimeOnlyChangeCount { get; set; }
}
```

继承 `ViewModel<TData>` 时，也可以让 Weaver 把属性代理到 `Data` 中的同名字段：

```csharp
[Serializable]
public class AutoWeaveSettingsData
{
    public float volume = 0.5f;
    public bool hardMode;
}

[Serializable]
[Observation]
public class AutoWeaveSettingsViewModel : ViewModel<AutoWeaveSettingsData>
{
    public float Volume { get; set; }
    public bool HardMode { get; set; }
}
```

规则说明：

- `[Observation]` 标记在属性上：只处理该属性。
- `[Observation]` 标记在类上：默认处理类中的属性。
- `[IgnoreObservation]`：跳过指定属性。
- `BindingHandler` 和 `Data` 已被标记为忽略观察。
- 完整示例见 `Sample/Scripts/AutoWeave/AutoWeaveSample.cs`。
- 可运行示例场景见 `Sample/Scene/AutoWeaveSample.unity`。进入 Play Mode 后点击按钮，文本刷新来自编织后的属性通知。

## UGUI 绑定

引入命名空间：

```csharp
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.UGUI;
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
using VoyageForge.ObservationToolkit.Runtime.Converter;

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
using VoyageForge.ObservationToolkit.Runtime.Converter;

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
using VoyageForge.ObservationToolkit.Runtime.Command;

private ICommand _addCommand;

public ICommand AddCommand => _addCommand ??= new RelayCommand(
    execute: () => Value++,
    canExecute: () => Value < 100
).Observes(this, m => m.Value);
```

绑定 Button：

```csharp
button.BindCommand(model, m => m.AddCommand);
```

当 `CanExecute` 条件依赖某个可观察属性时，推荐使用 `.Observes(...)` 登记依赖。属性通过 `SetField`、`OnPropertyChanged` 或 IL Weaver 通知变化后，命令会自动触发 `RaiseCanExecuteChanged()`，Button 的 `interactable` 会跟随命令可执行状态变化。

```csharp
public ICommand AddCommand => _addCommand ??= new RelayCommand(
    execute: () => Value++,
    canExecute: () => Value < 100
).Observes(this, m => m.Value);
```

如果命令依赖外部状态，或者不方便声明属性依赖，也可以手动刷新：

```csharp
_addCommand?.RaiseCanExecuteChanged();
```

同一个命令可以观察多个属性。相同命令和相同属性的重复观察会被去重：

```csharp
public ICommand SubmitCommand => _submitCommand ??= new RelayCommand(
    execute: Submit,
    canExecute: () => !string.IsNullOrEmpty(Name) && Age >= 18
).Observes(this, m => m.Name)
 .Observes(this, m => m.Age);
```

带参数命令可以使用 `RelayCommand<T>`。绑定时传入的参数会同时用于 `CanExecute` 和 `Execute`，并复用绑定系统的类型转换规则，例如字符串 `"5"` 可以转换为 `int` 参数。

```csharp
private ICommand _spendCommand;

public ICommand SpendCommand => _spendCommand ??= new RelayCommand<int>(
    execute: amount => Score -= amount,
    canExecute: amount => amount > 0 && Score >= amount
).Observes(this, m => m.Score);
```

```csharp
spendFiveButton.BindCommand(model, m => m.SpendCommand, 5);
spendTwentyButton.BindCommand(model, m => m.SpendCommand, "20");
```

`BindCommand` 会返回绑定句柄，调用方可以手动释放；同时默认也会挂到 Button 生命周期上，在 Button 销毁时自动解绑。

```csharp
var binding = button.BindCommand(model, m => m.AddCommand);
binding.Dispose();
```

Button 以外的无参数 `UnityEvent` 也可以绑定到命令。绑定默认会挂到目标组件生命周期上；如果目标组件继承自 `Selectable`，也会同步 `interactable`：

```csharp
var binding = toggle.BindCommand(
    toggle.onValueChanged,
    model.ToggleCommand);
```

如果命令参数不想直接使用事件值，可以传入参数选择器：

```csharp
slider.BindCommand(
    slider.onValueChanged,
    model.SetPercentCommand,
    value => Mathf.RoundToInt(value * 100f));
```

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
VoyageForge/Observation Settings
```

配置会保存到：

```text
ProjectSettings/ObservationWeaverSettings.json
```

配置项：

- `enableWeaver`：是否启用 IL 织入。
- `enableLogging`：是否输出静态编织日志。
- `weaveAssemblyCSharp`：是否处理 `Assembly-CSharp`。
- `extraAssemblies`：额外处理的 asmdef 程序集名称。

静态编织日志会写入：

```text
Library/ObservationToolkit/ObservationWeaver.log
```

也可以在 `VoyageForge/Observation Settings` 窗口中通过 `打开日志` 和 `清空日志` 按钮查看或清理日志。

默认情况下，CodeGen 中的 `ObservationWeaver` 会在编译后扫描目标程序集，并执行两类处理：

- `PropertyWeaver`：对被 `[Observation]` 命中的自动属性或可识别 backing field 的属性注入 `SetField`。
- `ViewModelWeaver`：对继承 `ViewModel<TData>` 的类型，将属性代理到 `Data` 中同名字段，并在 setter 中注入通知。

## 架构定位与待改进项

ObservationToolkit 的当前定位是“MVVM 风格的属性级绑定工具包”，而不是完整的 MVVM 应用框架。项目会保留统一、强类型、免显式泛型输入的绑定入口：

```csharp
model.For(m => m.Value)
    .To(text)
    .OneWay();

viewModel.For(vm => vm.Value)
    .To(slider)
    .TwoWay(s => s.onValueChanged);
```

`For(m => m.Value)` 是绑定 API，不用于区分对象属于 Model 还是 ViewModel。Model 和 ViewModel 都可以实现可观察能力，但推荐通过不同的语义、特性和编织规则表达它们的职责差异。

### Model 与 ViewModel 的可观察语义

当前的 `IObservable` 采用接口而不是基类，主要是为了避免 C# 单继承限制，并支持已经有基类或无法改继承链的类型。它表示“这个对象支持属性级变更通知”，不强制说明该对象一定是 Model 或 ViewModel。

推荐语义：

- Model 可以实现 `IObservable`，用于发布数据变化通知，避免通用 `Changed` 事件导致监听方每次都重新读取所有字段。
- ViewModel 也可以实现 `IObservable`，用于面向 View 暴露可绑定属性、Command、组合状态、格式化状态和 UI 交互状态。
- Model 不强制保持完全纯净；但推荐不要在 Model 中直接持有 UGUI 控件、View 生命周期、显示文案、颜色样式或按钮命令等 View 语义。

### 特性拆分方向

当前 `[Observation]` 同时覆盖普通 Model 属性织入和 `ViewModel<TData>` 代理织入，语义较宽。后续计划拆分为更明确的特性，例如：

```csharp
[ObservableModel]
public partial class PlayerModel : IObservable
{
    public int Value { get; set; }
}

[ObservableViewModel]
public partial class PlayerViewModel : IObservableViewModel<PlayerModel>
{
    public int Value { get; set; }
}
```

其中 `[ObservableModel]` 和 `[ObservableViewModel]` 的含义不同：

- `[ObservableModel]`：处理对象自身属性，将自动属性或 backing field 改写为属性级通知。
- `[ObservableViewModel]`：处理面向 View 的代理属性，可将属性代理到 `Data` / `Model` 的同名字段或属性，并触发 ViewModel 属性通知。
- `[Observation]` 可作为兼容旧代码的通用特性保留一段时间。

### 静态编织拆分方向

后续 `ObservationWeaver` 可以逐步从单一编织器拆为调度器：

```text
ObservationWeaver
├── ModelWeaver
├── ViewModelWeaver
└── LegacyObservationWeaver
```

建议规则：

- 标记 `[ObservableModel]` 的类型只进入 `ModelWeaver`。
- 标记 `[ObservableViewModel]` 的类型只进入 `ViewModelWeaver`。
- 标记旧 `[Observation]` 的类型走兼容逻辑。
- 如果一个类型同时标记 Model 和 ViewModel 语义，应输出诊断日志或编译诊断，提示职责冲突。

Model 静态编织目标：

```text
self property -> self backing field -> notify same property
```

ViewModel 静态编织目标：

```text
viewModel property -> Data/Model 同名字段或属性 -> notify viewModel property
```

二者都支持 `model.For(m => m.Value)` 这种强类型绑定入口，但底层织入逻辑不应混为一谈。

### 源代码生成器方向

IL Post Processor 可以减少运行时代码样板，但 IDE 可见性和调试体验有限。后续可以增加 Source Generator，用于生成强类型、可补全、免字符串输入的辅助 API。

目标：

- 不手写字符串属性名。
- 不显式填写泛型参数。
- 不为每个属性手写大量 `SetField` 样板。
- 尽量让生成代码可被 IDE 跳转、补全和重构。

可能的生成效果：

```csharp
viewModel.ObserveValue()
    .To(text)
    .OneWay();
```

或继续保留当前表达式入口：

```csharp
viewModel.For(vm => vm.Value)
    .To(text)
    .OneWay();
```

Source Generator 的重点不是替代 `For(m => m.Value)`，而是在需要时提供更强的补全体验和更少的手写样板。

### 推荐演进原则

- 保留 `model.For(m => m.Value)` 作为统一核心绑定入口。
- 保留接口式可观察能力，避免基类侵入。
- 将“Model 可观察”和“ViewModel 可观察”的语义通过特性、接口和编织规则区分。
- Model 侧推荐专注数据变化通知；ViewModel 侧推荐承接 View 适配、Command、组合状态和显示语义。
- 静态编织和源代码生成器都应围绕强类型、免字符串、少样板、可诊断这几个目标演进。

## 示例

项目内置示例位于：

```text
Sample/
├── Scene/AutoWeaveSample.unity
├── Scene/CommandSample.unity
├── Scene/ObservationToolkit.unity
├── Scene/UGUI.unity
└── Scripts/
    ├── AutoWeave/AutoWeaveSample.cs
    └── UGUI/
```

重点脚本：

- `Sample/Scripts/Test.cs`：基础 Model、ViewModel、Command 示例。
- `Sample/Scripts/AutoWeave/AutoWeaveSample.cs`：`[Observation]`、`[IgnoreObservation]`、普通 Model 自动织入和 `ViewModel<TData>` 自动代理示例。
- `Sample/Scene/AutoWeaveSample.unity`：静态编织运行示例，包含自动属性绑定文本和按钮交互。
- `Sample/Scene/CommandSample.unity`：Command 运行示例，包含无参命令、带参数命令、Button 绑定和 `.Observes(...)` 自动刷新 `CanExecute`。
- `Sample/Scripts/UGUI/CommandSample.cs`：Command 示例的 ViewModel、命令属性和场景绑定脚本。
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
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using VoyageForge.ObservationToolkit.Runtime.UGUI;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;

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
        set => this.SetField(ref Data.value, value);
    }

    private ICommand _addCommand;
    public ICommand AddCommand => _addCommand ??= new RelayCommand(
        () => Value++,
        () => Value < 100
    ).Observes(this, m => m.Value);
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

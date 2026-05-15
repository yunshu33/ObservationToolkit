using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using Debug = UnityEngine.Debug;

namespace VoyageForge.ObservationToolkit.Tests.EditMode
{
    /// <summary>
    /// Observation Toolkit 的 EditMode 性能基线测试。
    /// 这些测试不替代 Unity Performance Testing 包的统计采样能力，而是提供轻量、无额外依赖的回归护栏，
    /// 用于及时发现绑定分发、命令刷新和参数转换等核心路径出现数量级退化。
    /// </summary>
    public class PerformanceTests
    {
        /// <summary>
        /// 绑定通知分发测试的属性写入次数。
        /// 该数量足够覆盖热路径循环，又不会让普通编辑器测试运行时间过长。
        /// </summary>
        private const int BindingIterations = 20_000;

        /// <summary>
        /// Command 扇出测试中登记到同一属性上的命令数量。
        /// 该值模拟一个页面中多个按钮同时依赖同一个 ViewModel 状态的常见场景。
        /// </summary>
        private const int ObservedCommandCount = 64;

        /// <summary>
        /// Command 扇出测试中的属性变化次数。
        /// 总刷新次数为 <see cref="ObservedCommandCount"/> 与该值的乘积。
        /// </summary>
        private const int CommandRefreshIterations = 1_000;

        /// <summary>
        /// 参数命令转换测试的执行次数。
        /// 用于覆盖 UI 参数从 object 或 string 进入强类型命令时的转换开销。
        /// </summary>
        private const int ParameterCommandIterations = 20_000;

        /// <summary>
        /// 绑定通知分发允许的最大耗时，单位为毫秒。
        /// 阈值刻意设置得较宽，目标是捕获明显回归，而不是让测试依赖具体机器性能。
        /// </summary>
        private const long BindingBudgetMilliseconds = 1_000;

        /// <summary>
        /// Command 扇出刷新允许的最大耗时，单位为毫秒。
        /// 该路径包含弱引用清理、快照复制和事件触发，因此预算比普通绑定分发更宽。
        /// </summary>
        private const long CommandRefreshBudgetMilliseconds = 2_000;

        /// <summary>
        /// 参数命令转换允许的最大耗时，单位为毫秒。
        /// 字符串到数值的转换比直接调用更重，因此这里使用单独预算。
        /// </summary>
        private const long ParameterCommandBudgetMilliseconds = 1_000;

        /// <summary>
        /// 验证单个属性绑定在高频变化时仍能在预算内完成分发。
        /// 该测试覆盖 <see cref="BindingHandler.OnPropertyChanged{V}"/> 到绑定回调调用的核心链路。
        /// </summary>
        [Test]
        public void BindingNotificationDispatchesRepeatedUpdatesWithinBudget()
        {
            var model = new PerformanceTestModel();
            var observedValue = 0;

            model.For(m => m.Value)
                .To(value => observedValue = value)
                .OneWay();

            for (var i = 1; i <= 100; i++)
            {
                model.Value = i;
            }

            var elapsedMilliseconds = MeasureMilliseconds(() =>
            {
                for (var i = 1; i <= BindingIterations; i++)
                {
                    model.Value = i;
                }
            });

            Debug.Log(elapsedMilliseconds);
            
            Assert.AreEqual(BindingIterations, observedValue);
            Assert.Less(
                elapsedMilliseconds,
                BindingBudgetMilliseconds,
                $"Binding dispatch took {elapsedMilliseconds}ms for {BindingIterations} updates.");
        }

        /// <summary>
        /// 验证多个命令同时观察同一个属性时，属性变化可以在预算内刷新所有命令。
        /// 该测试重点覆盖 <see cref="CommandManager.Observes{TCommand,TSource,TProperty}"/> 登记后的扇出刷新路径。
        /// </summary>
        [Test]
        public void CommandObservesRefreshesManyCommandsWithinBudget()
        {
            var model = new PerformanceTestModel();
            var commands = new List<ICommand>(ObservedCommandCount);
            var refreshCount = 0;

            for (var i = 0; i < ObservedCommandCount; i++)
            {
                var command = new RelayCommand(() => { }, () => model.Value >= 0)
                    .Observes(model, m => m.Value);
                command.CanExecuteChanged += () => refreshCount++;
                commands.Add(command);
            }

            model.Value = 1;
            refreshCount = 0;

            var elapsedMilliseconds = MeasureMilliseconds(() =>
            {
                for (var i = 2; i < CommandRefreshIterations + 2; i++)
                {
                    model.Value = i;
                }
            });
            Debug.Log(elapsedMilliseconds);
            Assert.AreEqual(ObservedCommandCount * CommandRefreshIterations, refreshCount);
            Assert.AreEqual(ObservedCommandCount, commands.Count);
            Assert.Less(
                elapsedMilliseconds,
                CommandRefreshBudgetMilliseconds,
                $"Command refresh took {elapsedMilliseconds}ms for {ObservedCommandCount * CommandRefreshIterations} notifications.");
        }

        /// <summary>
        /// 验证带参数命令在高频执行字符串参数转换时仍保持在预算内。
        /// 该测试覆盖 <see cref="RelayCommand{T}"/> 对 UI 传入参数的强类型转换路径。
        /// </summary>
        [Test]
        public void RelayCommandConvertsRepeatedStringParametersWithinBudget()
        {
            var receivedValue = 0;
            var command = new RelayCommand<int>(value => receivedValue += value);

            command.Execute("1");
            receivedValue = 0;

            var elapsedMilliseconds = MeasureMilliseconds(() =>
            {
                for (var i = 0; i < ParameterCommandIterations; i++)
                {
                    command.Execute("1");
                }
            });
            Debug.Log(elapsedMilliseconds);
            Assert.AreEqual(ParameterCommandIterations, receivedValue);
            Assert.Less(
                elapsedMilliseconds,
                ParameterCommandBudgetMilliseconds,
                $"Parameter command conversion took {elapsedMilliseconds}ms for {ParameterCommandIterations} executions.");
        }

        /// <summary>
        /// 执行一段同步代码并返回耗时毫秒数。
        /// 单独封装可以让每个测试只描述业务路径，避免重复秒表样板代码。
        /// </summary>
        /// <param name="action">需要测量的同步代码块。</param>
        /// <returns>代码块执行耗时，单位为毫秒。</returns>
        private static long MeasureMilliseconds(System.Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 性能测试使用的最小可观察模型。
        /// 该模型只暴露一个整数属性，便于把测试噪音集中到绑定和命令系统本身。
        /// </summary>
        private sealed class PerformanceTestModel : IObservable
        {
            /// <summary>
            /// 绑定处理器实例。
            /// 首次创建绑定时由 <see cref="BindingExtensions.Binding"/> 自动初始化。
            /// </summary>
            public BindingHandler BindingHandler { get; set; }

            /// <summary>
            /// <see cref="Value"/> 属性的后备字段。
            /// </summary>
            private int _value;

            /// <summary>
            /// 高频变化的测试属性。
            /// Setter 使用 <see cref="BindingExtensions.SetField{SProperty}"/> 触发绑定通知和命令刷新。
            /// </summary>
            public int Value
            {
                get => _value;
                set => this.SetField(ref _value, value);
            }
        }
    }
}

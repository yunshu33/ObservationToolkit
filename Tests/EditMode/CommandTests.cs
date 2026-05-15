using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using VoyageForge.ObservationToolkit.Runtime.UGUI;

namespace VoyageForge.ObservationToolkit.Tests.EditMode
{
    /// <summary>
    /// Command 系统的 EditMode 测试。
    /// 这些测试覆盖命令可执行状态自动刷新、参数转换，以及 Button 绑定生命周期。
    /// </summary>
    public class CommandTests
    {
        /// <summary>
        /// 验证 Observes 登记的属性变化会触发一次 CanExecuteChanged。
        /// </summary>
        [Test]
        public void ObservesRaisesCanExecuteChangedWhenPropertyChanges()
        {
            var model = new CommandTestModel();
            var raisedCount = 0;
            var command = new RelayCommand(() => { }, () => model.Score > 0)
                .Observes(model, m => m.Score);

            command.CanExecuteChanged += () => raisedCount++;

            model.Score = 1;

            Assert.AreEqual(1, raisedCount);
        }

        /// <summary>
        /// 验证重复观察同一个属性和命令时会被去重，避免一次属性变化触发多次刷新。
        /// </summary>
        [Test]
        public void ObservesIgnoresDuplicateCommandPropertyPairs()
        {
            var model = new CommandTestModel();
            var raisedCount = 0;
            var command = new RelayCommand(() => { }, () => model.Score > 0);

            command.Observes(model, m => m.Score)
                .Observes(model, m => m.Score);
            command.CanExecuteChanged += () => raisedCount++;

            model.Score = 1;

            Assert.AreEqual(1, raisedCount);
        }

        /// <summary>
        /// 验证 ObserveCanExecute 返回的释放句柄可以取消属性观察关系。
        /// </summary>
        [Test]
        public void ObserveCanExecuteDisposeStopsCanExecuteRefresh()
        {
            var model = new CommandTestModel();
            var raisedCount = 0;
            var command = new RelayCommand(() => { }, () => model.Score > 0);
            var observation = command.ObserveCanExecute(model, m => m.Score);

            command.CanExecuteChanged += () => raisedCount++;
            observation.Dispose();

            model.Score = 1;

            Assert.AreEqual(0, raisedCount);
        }

        /// <summary>
        /// 验证 RelayCommand 泛型参数会复用绑定系统的转换规则。
        /// </summary>
        [Test]
        public void RelayCommandConvertsStringParameterToStrongType()
        {
            var receivedValue = 0;
            var command = new RelayCommand<int>(value => receivedValue = value);

            command.Execute("7");

            Assert.AreEqual(7, receivedValue);
        }

        /// <summary>
        /// 验证 Button 命令绑定会同步 interactable，点击时执行命令，并在 Dispose 后移除事件监听。
        /// </summary>
        [Test]
        public void ButtonBindCommandSynchronizesInteractableAndDisposesListener()
        {
            var gameObject = new GameObject("Command Button Test");
            try
            {
                var button = gameObject.AddComponent<Button>();
                var canExecute = false;
                var executeCount = 0;
                var command = new RelayCommand(() => executeCount++, () => canExecute);

                var binding = button.BindCommand(command);

                Assert.IsFalse(button.interactable);

                canExecute = true;
                command.RaiseCanExecuteChanged();
                Assert.IsTrue(button.interactable);

                button.onClick.Invoke();
                Assert.AreEqual(1, executeCount);

                binding.Dispose();
                button.onClick.Invoke();
                Assert.AreEqual(1, executeCount);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// 验证带参数 UnityEvent 可以把事件值传递给泛型命令。
        /// </summary>
        [Test]
        public void GenericUnityEventBindCommandPassesEventValueToCommand()
        {
            var gameObject = new GameObject("Command Event Test");
            try
            {
                var component = gameObject.AddComponent<CommandEventTestComponent>();
                var receivedValue = 0;
                var command = new RelayCommand<int>(value => receivedValue = value);
                var binding = component.BindCommand(component.IntEvent, command);

                component.IntEvent.Invoke(42);

                Assert.AreEqual(42, receivedValue);

                binding.Dispose();
                component.IntEvent.Invoke(7);
                Assert.AreEqual(42, receivedValue);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        /// <summary>
        /// 测试用可观察模型。
        /// </summary>
        private sealed class CommandTestModel : IObservable
        {
            /// <summary>
            /// 绑定处理器。
            /// 测试只依赖命令观察功能，不需要显式初始化普通绑定。
            /// </summary>
            public BindingHandler BindingHandler { get; set; }

            /// <summary>
            /// 当前测试分数的后备字段。
            /// </summary>
            private int _score;

            /// <summary>
            /// 当前测试分数。
            /// 修改该属性会通过 SetField 触发属性通知。
            /// </summary>
            public int Score
            {
                get => _score;
                set => this.SetField(ref _score, value);
            }
        }

        /// <summary>
        /// 测试用组件，提供一个带 int 参数的 UnityEvent。
        /// </summary>
        private sealed class CommandEventTestComponent : MonoBehaviour
        {
            /// <summary>
            /// 用于验证泛型 UnityEvent 命令绑定的事件。
            /// </summary>
            public readonly UnityEvent<int> IntEvent = new();
        }
    }
}

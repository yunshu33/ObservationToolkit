using System;
using System.Linq.Expressions;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Command;
using UnityEngine;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Button 与 ICommand 的便捷绑定扩展。
    /// View 层只负责把按钮事件接到命令，具体业务逻辑仍放在 ViewModel 的 ICommand 中。
    /// </summary>
    public static class ButtonCommandExtensions
    {
        /// <summary>
        /// 直接把一个 ICommand 绑定到 Button.onClick。
        /// 适合命令已经由外部创建好，或者不需要跟随 ViewModel 属性变化的场景。
        /// </summary>
        /// <param name="button">需要绑定命令的目标按钮。</param>
        /// <param name="command">按钮点击时要执行的命令实例。</param>
        /// <param name="parameter">传递给命令 CanExecute 和 Execute 的固定参数。</param>
        /// <returns>本次按钮命令绑定的释放句柄，可用于手动解绑。</returns>
        public static IDisposable BindCommand(this Button button, ICommand command, object parameter = null)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (command == null) throw new ArgumentNullException(nameof(command));

            // 获取或添加 Proxy，用于在 GameObject 销毁时自动释放命令绑定。
            if (!button.gameObject.TryGetComponent<CommandProxy>(out var proxy))
            {
                proxy = button.gameObject.AddComponent<CommandProxy>();
            }

            // 同一个按钮重复绑定时先清理旧绑定，避免多次点击触发多条命令。
            proxy.Clear();

            // 创建新绑定，并同步 CanExecute 到 Button.interactable。
            var binder = new CommandBinder<Button>(button, button.onClick, command, parameter);

            // 交给 Proxy 持有，按钮销毁时统一 Dispose。
            proxy.SetBinder(binder);
            return binder;
        }

        /// <summary>
        /// 把 ViewModel 上的 ICommand 属性绑定到 Button.onClick。
        /// 当命令属性本身发生变化时，Button 会切换到新的命令实例。
        /// </summary>
        /// <typeparam name="S">提供命令属性的可观察源对象类型。</typeparam>
        /// <param name="button">需要绑定命令的目标按钮。</param>
        /// <param name="source">包含 ICommand 属性的 ViewModel 或 Model。</param>
        /// <param name="commandExpression">直接指向 ICommand 属性的表达式，例如 <c>m =&gt; m.SaveCommand</c>。</param>
        /// <param name="parameter">传递给命令 CanExecute 和 Execute 的固定参数。</param>
        /// <returns>本次 ViewModel 命令绑定的释放句柄，可用于手动解绑。</returns>
        public static IDisposableBinding BindCommand<S>(this Button button, S source, Expression<Func<S, ICommand>> commandExpression, object parameter = null) where S : class, IObservable
        {
            if (button == null) throw new ArgumentNullException(nameof(button));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (commandExpression == null) throw new ArgumentNullException(nameof(commandExpression));

            var binding = source.For(commandExpression).To(button, parameter).OneWay();
            binding.AddTo(button);
            return binding;
        }

        /// <summary>
        /// 挂在 Button 所在 GameObject 上的生命周期代理。
        /// 它不参与业务逻辑，只负责持有 IDisposable 并在销毁时释放。
        /// </summary>
        private class CommandProxy : MonoBehaviour
        {
            /// <summary>
            /// 当前按钮的命令绑定。
            /// </summary>
            private IDisposable _binder;

            /// <summary>
            /// 保存新的绑定实例。
            /// </summary>
            public void SetBinder(IDisposable binder)
            {
                _binder = binder;
            }

            /// <summary>
            /// 释放当前绑定。
            /// </summary>
            public void Clear()
            {
                _binder?.Dispose();
                _binder = null;
            }

            /// <summary>
            /// Button 销毁时自动解绑，避免命令继续持有 UI 回调。
            /// </summary>
            private void OnDestroy()
            {
                Clear();
            }
        }
    }
}

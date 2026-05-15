using System;
using UnityEngine;
using UnityEngine.Events;

namespace VoyageForge.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 通用 UnityEvent 到 <see cref="ICommand"/> 的绑定扩展。
    /// 该扩展用于 Button 以外的组件事件，例如菜单项、自定义控件事件或任意无参数 UnityEvent。
    /// </summary>
    public static class CommandBindingExtensions
    {
        /// <summary>
        /// 将任意无参数 <see cref="UnityEvent"/> 绑定到命令。
        /// 如果目标组件继承自 Selectable，绑定器会自动同步其 interactable 状态。
        /// </summary>
        /// <typeparam name="TTarget">持有 UnityEvent 的目标组件类型。</typeparam>
        /// <param name="target">需要执行命令的目标组件。</param>
        /// <param name="unityEvent">触发命令执行的 UnityEvent。</param>
        /// <param name="command">UnityEvent 触发时需要执行的命令。</param>
        /// <param name="parameter">传递给命令 CanExecute 和 Execute 的固定参数。</param>
        /// <param name="addToTargetLifecycle">是否自动把绑定挂到目标组件所在 GameObject 的生命周期上。</param>
        /// <returns>本次 UnityEvent 命令绑定的释放句柄。</returns>
        public static IDisposable BindCommand<TTarget>(
            this TTarget target,
            UnityEvent unityEvent,
            ICommand command,
            object parameter = null,
            bool addToTargetLifecycle = true)
            where TTarget : Component
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var binder = new CommandBinder<TTarget>(target, unityEvent, command, parameter);
            if (addToTargetLifecycle)
            {
                BindingLifecycleExtensions.AddTo(binder, target);
            }

            return binder;
        }

        /// <summary>
        /// 将任意带一个参数的 <see cref="UnityEvent{T0}"/> 绑定到命令。
        /// 默认会把事件值作为命令参数传入，也可以通过 parameterSelector 投影为其它命令参数。
        /// </summary>
        /// <typeparam name="TTarget">持有 UnityEvent 的目标组件类型。</typeparam>
        /// <typeparam name="TEventValue">UnityEvent 传出的事件值类型。</typeparam>
        /// <param name="target">需要执行命令的目标组件。</param>
        /// <param name="unityEvent">触发命令执行的 UnityEvent。</param>
        /// <param name="command">UnityEvent 触发时需要执行的命令。</param>
        /// <param name="parameterSelector">将事件值转换为命令参数的选择器，为 null 时直接使用事件值。</param>
        /// <param name="addToTargetLifecycle">是否自动把绑定挂到目标组件所在 GameObject 的生命周期上。</param>
        /// <returns>本次 UnityEvent 命令绑定的释放句柄。</returns>
        public static IDisposable BindCommand<TTarget, TEventValue>(
            this TTarget target,
            UnityEvent<TEventValue> unityEvent,
            ICommand command,
            Func<TEventValue, object> parameterSelector = null,
            bool addToTargetLifecycle = true)
            where TTarget : Component
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (unityEvent == null) throw new ArgumentNullException(nameof(unityEvent));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var binder = new CommandBinder<TTarget, TEventValue>(
                target,
                unityEvent,
                command,
                parameterSelector);
            if (addToTargetLifecycle)
            {
                BindingLifecycleExtensions.AddTo(binder, target);
            }

            return binder;
        }
    }
}

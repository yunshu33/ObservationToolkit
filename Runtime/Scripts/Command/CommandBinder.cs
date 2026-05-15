using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 通用命令绑定器，用于把任意 UnityEvent 绑定到 ICommand。
    /// </summary>
    /// <typeparam name="TTarget">目标组件类型。</typeparam>
    public class CommandBinder<TTarget> : IDisposable where TTarget : Component
    {
        /// <summary>
        /// 目标组件。
        /// </summary>
        private readonly TTarget _target;

        /// <summary>
        /// 被绑定的命令。
        /// </summary>
        private readonly ICommand _command;

        /// <summary>
        /// 命令参数。
        /// </summary>
        private readonly object _commandParameter;

        /// <summary>
        /// 目标 UI 事件。
        /// </summary>
        private readonly UnityEvent _uiEvent;

        /// <summary>
        /// 注册到 UI 事件上的回调。
        /// </summary>
        private readonly UnityAction _uiAction;

        /// <summary>
        /// 如果目标组件也是 Selectable，则同步 interactable。
        /// </summary>
        private readonly Selectable _selectable;

        /// <summary>
        /// 创建命令绑定器，并立即注册事件。
        /// </summary>
        public CommandBinder(TTarget target, UnityEvent uiEvent, ICommand command, object parameter = null)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _uiEvent = uiEvent ?? throw new ArgumentNullException(nameof(uiEvent));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _commandParameter = parameter;

            _selectable = _target.GetComponent<Selectable>();
            _uiAction = OnUIEvent;
            _uiEvent.AddListener(_uiAction);

            _command.CanExecuteChanged += OnCanExecuteChanged;
            UpdateInteractable();
        }

        /// <summary>
        /// UI 事件触发时执行命令。
        /// </summary>
        private void OnUIEvent()
        {
            if (_command.CanExecute(_commandParameter))
            {
                _command.Execute(_commandParameter);
            }
        }

        /// <summary>
        /// 命令可执行状态变化时刷新 UI 状态。
        /// </summary>
        private void OnCanExecuteChanged()
        {
            UpdateInteractable();
        }

        /// <summary>
        /// 同步 Selectable.interactable。
        /// </summary>
        private void UpdateInteractable()
        {
            if (_selectable != null)
            {
                _selectable.interactable = _command.CanExecute(_commandParameter);
            }
        }

        /// <summary>
        /// 解除 UI 事件和命令事件订阅。
        /// </summary>
        public void Dispose()
        {
            _uiEvent?.RemoveListener(_uiAction);
            _command.CanExecuteChanged -= OnCanExecuteChanged;
        }
    }

    /// <summary>
    /// 带事件参数的通用命令绑定器，用于把 <see cref="UnityEvent{T0}"/> 绑定到 <see cref="ICommand"/>。
    /// </summary>
    /// <typeparam name="TTarget">目标组件类型。</typeparam>
    /// <typeparam name="TEventValue">UnityEvent 传出的事件值类型。</typeparam>
    public class CommandBinder<TTarget, TEventValue> : IDisposable where TTarget : Component
    {
        /// <summary>
        /// 目标组件。
        /// </summary>
        private readonly TTarget _target;

        /// <summary>
        /// 被绑定的命令。
        /// </summary>
        private readonly ICommand _command;

        /// <summary>
        /// 目标 UI 事件。
        /// </summary>
        private readonly UnityEvent<TEventValue> _uiEvent;

        /// <summary>
        /// 注册到 UI 事件上的回调。
        /// </summary>
        private readonly UnityAction<TEventValue> _uiAction;

        /// <summary>
        /// 将事件值转换为命令参数的选择器。
        /// 当调用方没有提供选择器时，直接把事件值传给命令。
        /// </summary>
        private readonly Func<TEventValue, object> _parameterSelector;

        /// <summary>
        /// 如果目标组件也是 Selectable，则同步 interactable。
        /// </summary>
        private readonly Selectable _selectable;

        /// <summary>
        /// 创建带事件参数的命令绑定器，并立即注册事件。
        /// </summary>
        /// <param name="target">持有 UnityEvent 的目标组件。</param>
        /// <param name="uiEvent">触发命令执行的 UnityEvent。</param>
        /// <param name="command">事件触发时需要执行的命令。</param>
        /// <param name="parameterSelector">将事件值转换为命令参数的选择器。</param>
        public CommandBinder(
            TTarget target,
            UnityEvent<TEventValue> uiEvent,
            ICommand command,
            Func<TEventValue, object> parameterSelector = null)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _uiEvent = uiEvent ?? throw new ArgumentNullException(nameof(uiEvent));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _parameterSelector = parameterSelector ?? (value => value);

            _selectable = _target.GetComponent<Selectable>();
            _uiAction = OnUIEvent;
            _uiEvent.AddListener(_uiAction);

            _command.CanExecuteChanged += OnCanExecuteChanged;
            UpdateInteractable(default);
        }

        /// <summary>
        /// UI 事件触发时执行命令。
        /// </summary>
        /// <param name="eventValue">UnityEvent 传出的事件值。</param>
        private void OnUIEvent(TEventValue eventValue)
        {
            var commandParameter = _parameterSelector(eventValue);
            if (_command.CanExecute(commandParameter))
            {
                _command.Execute(commandParameter);
            }
        }

        /// <summary>
        /// 命令可执行状态变化时刷新 UI 状态。
        /// 由于泛型 UnityEvent 的当前事件值不可从事件本身读取，这里使用 default(TEventValue) 作为状态刷新参数。
        /// </summary>
        private void OnCanExecuteChanged()
        {
            UpdateInteractable(default);
        }

        /// <summary>
        /// 同步 Selectable.interactable。
        /// </summary>
        /// <param name="eventValue">用于计算命令参数的事件值。</param>
        private void UpdateInteractable(TEventValue eventValue)
        {
            if (_selectable != null)
            {
                _selectable.interactable = _command.CanExecute(_parameterSelector(eventValue));
            }
        }

        /// <summary>
        /// 解除 UI 事件和命令事件订阅。
        /// </summary>
        public void Dispose()
        {
            _uiEvent?.RemoveListener(_uiAction);
            _command.CanExecuteChanged -= OnCanExecuteChanged;
        }
    }
}

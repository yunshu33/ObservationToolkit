using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.Command
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
}

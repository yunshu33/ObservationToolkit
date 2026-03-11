using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 命令绑定器
    /// </summary>
    public class CommandBinder<TTarget> : IDisposable where TTarget : Component
    {
        private readonly TTarget _target;
        private readonly ICommand _command;
        private readonly object _commandParameter;
        private readonly UnityEvent _uiEvent;
        private readonly UnityAction _uiAction;
        private readonly Selectable _selectable;

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
            
            // 初始化执行状态
            UpdateInteractable();
        }

        private void OnUIEvent()
        {
            if (_command.CanExecute(_commandParameter))
            {
                _command.Execute(_commandParameter);
            }
        }

        private void OnCanExecuteChanged()
        {
            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            if (_selectable != null)
            {
                _selectable.interactable = _command.CanExecute(_commandParameter);
            }
        }

        public void Dispose()
        {
            _uiEvent?.RemoveListener(_uiAction);
            if (_command != null)
            {
                _command.CanExecuteChanged -= OnCanExecuteChanged;
            }
        }
    }
}

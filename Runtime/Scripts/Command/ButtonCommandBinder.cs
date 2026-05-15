using System;
using Voyage.ObservationToolkit.Runtime.Command;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Button 的 ICommand 绑定器。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    public class ButtonCommandBinder<S> : Binder<S, ICommand>, IDisposableBinding
    {
        /// <summary>
        /// 绑定释放时触发，用于 BindingContext 自动移除。
        /// </summary>
        public event Action OnDisposed;

        /// <summary>
        /// 目标按钮。
        /// </summary>
        private readonly Button _target;

        /// <summary>
        /// 当前绑定的命令。
        /// </summary>
        private ICommand _currentCommand;

        /// <summary>
        /// 命令参数。
        /// </summary>
        private object _commandParameter;

        /// <summary>
        /// 缓存的 Button 点击回调，解绑时必须用同一个委托实例移除。
        /// </summary>
        private readonly UnityEngine.Events.UnityAction _onClickAction;

        /// <summary>
        /// 创建 Button 命令绑定器。
        /// </summary>
        public ButtonCommandBinder(Button target, Binding<S, ICommand> binding, object parameter = null) : base(binding)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _commandParameter = parameter;
            _onClickAction = OnClick;
            _target.onClick.AddListener(_onClickAction);
        }

        /// <summary>
        /// 绑定目标方法名。
        /// </summary>
        public override string MethodName => "BindCommand";

        /// <summary>
        /// 缓存后的绑定哈希。
        /// </summary>
        private int _hashCode;

        /// <summary>
        /// 绑定唯一哈希。
        /// </summary>
        public override int HashCode => _hashCode != 0
            ? _hashCode
            : (_hashCode = _target.GetHashCode() ^ "Command".GetHashCode());

        /// <summary>
        /// Button 点击时执行命令。
        /// </summary>
        private void OnClick()
        {
            if (_currentCommand != null && _currentCommand.CanExecute(_commandParameter))
            {
                _currentCommand.Execute(_commandParameter);
            }
        }

        /// <summary>
        /// 命令可执行状态变化时刷新 Button.interactable。
        /// </summary>
        private void OnCanExecuteChanged()
        {
            if (_target != null && _currentCommand != null)
            {
                _target.interactable = _currentCommand.CanExecute(_commandParameter);
            }
        }

        /// <summary>
        /// 接收新的 ICommand，并更新事件订阅。
        /// </summary>
        public override void Invoke(S source, ICommand command)
        {
            if (_currentCommand != null)
            {
                _currentCommand.CanExecuteChanged -= OnCanExecuteChanged;
            }

            _currentCommand = command;

            if (_currentCommand != null)
            {
                _currentCommand.CanExecuteChanged += OnCanExecuteChanged;
                OnCanExecuteChanged();
            }
            else
            {
                _target.interactable = false;
            }
        }

        /// <summary>
        /// Binding 移除此绑定器时释放 UI 和命令事件。
        /// </summary>
        public override void OnUnbind()
        {
            if (_target != null)
            {
                _target.onClick.RemoveListener(_onClickAction);
            }

            if (_currentCommand != null)
            {
                _currentCommand.CanExecuteChanged -= OnCanExecuteChanged;
                _currentCommand = null;
            }
        }

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        public void Unbind()
        {
            try
            {
                _binding.Unbind(HashCode);
                OnDisposed?.Invoke();
                OnDisposed = null;
            }
            catch (Exception)
            {
                // 重复解绑时保持幂等，避免对象销毁阶段抛异常。
            }
        }

        /// <summary>
        /// 释放资源，等同于 Unbind。
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        /// <summary>
        /// 建立单向命令绑定，并立即同步一次当前命令状态。
        /// </summary>
        public IDisposableBinding OneWay()
        {
            _binding.Bind(this);
            try
            {
                if (_binding.TryGetCurrentValue(out var command))
                {
                    Invoke(default, command);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ButtonCommandBinder] Failed to get initial command value: {e}");
            }

            return this;
        }
    }
}

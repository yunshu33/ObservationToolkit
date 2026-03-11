using System;
using System.Reflection;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Button 的命令绑定器
    /// </summary>
    /// <typeparam name="S"></typeparam>
    public class ButtonCommandBinder<S> : Binder<S, ICommand>, IDisposableBinding
    {
        public event Action OnDisposed;

        private readonly Button _target;
        private ICommand _currentCommand;
        private object _commandParameter;
        private readonly UnityEngine.Events.UnityAction _onClickAction;

        public ButtonCommandBinder(Button target, Binding<S, ICommand> binding, object parameter = null) : base(binding)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _commandParameter = parameter;
            _onClickAction = OnClick;
            
            _target.onClick.AddListener(_onClickAction);
        }

        public override string MethodName => "BindCommand";

        private int _hashCode;
        public override int HashCode => _hashCode != 0 ? _hashCode : (_hashCode = _target.GetHashCode() ^ "Command".GetHashCode());

        private void OnClick()
        {
            if (_currentCommand != null && _currentCommand.CanExecute(_commandParameter))
            {
                _currentCommand.Execute(_commandParameter);
            }
        }

        private void OnCanExecuteChanged()
        {
            if (_target != null && _currentCommand != null)
            {
                _target.interactable = _currentCommand.CanExecute(_commandParameter);
            }
        }

        public override void Invoke(S source, ICommand command)
        {
            // 清理旧命令
            if (_currentCommand != null)
            {
                _currentCommand.CanExecuteChanged -= OnCanExecuteChanged;
            }

            _currentCommand = command;

            // 绑定新命令
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
                // ignore
            }
        }

        public void Dispose()
        {
            Unbind();
        }
        
        public IDisposableBinding OneWay()
        {
            _binding.Bind(this);
            // 立即执行一次 Invoke 以初始化状态
            // 注意：这里使用反射获取初始值可能性能不佳，但为了初始化状态是必要的
            // 如果 Binding 对象能缓存当前值会更好
            try 
            {
                var prop = _binding.Source.GetType().GetProperty(_binding.PropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (prop != null)
                {
                    Invoke(default, prop.GetValue(_binding.Source) as ICommand);
                }
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError($"[ButtonCommandBinder] Failed to get initial command value: {e}");
            }
            return this;
        }
    }
}

using System;

namespace Voyage.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 无参数命令的通用实现。
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// 命令执行逻辑。
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// 命令可执行判断。
        /// </summary>
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 命令可执行状态变化事件。
        /// </summary>
        public event Action CanExecuteChanged;

        /// <summary>
        /// 创建无参数命令。
        /// </summary>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可执行。
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        /// <summary>
        /// 执行命令。
        /// </summary>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute();
            }
        }

        /// <summary>
        /// 主动通知可执行状态变化。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }

    /// <summary>
    /// 带参数命令的通用实现。
    /// </summary>
    /// <typeparam name="T">命令参数类型。</typeparam>
    public class RelayCommand<T> : ICommand
    {
        /// <summary>
        /// 命令执行逻辑。
        /// </summary>
        private readonly Action<T> _execute;

        /// <summary>
        /// 命令可执行判断。
        /// </summary>
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// 命令可执行状态变化事件。
        /// </summary>
        public event Action CanExecuteChanged;

        /// <summary>
        /// 创建带参数命令。
        /// </summary>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可执行。
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) return true;
            if (parameter == null && typeof(T).IsValueType) return _canExecute(default);
            return _canExecute((T)parameter);
        }

        /// <summary>
        /// 执行命令。
        /// </summary>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute((T)parameter);
            }
        }

        /// <summary>
        /// 主动通知可执行状态变化。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }
}

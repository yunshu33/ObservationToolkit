using System;
using VoyageForge.ObservationToolkit.Runtime.Converter;

namespace VoyageForge.ObservationToolkit.Runtime.Command
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
    /// 适合把按钮、菜单、快捷键等 UI 事件携带的参数传回 ViewModel。
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
            return _canExecute(ConvertParameter(parameter));
        }

        /// <summary>
        /// 执行命令。
        /// </summary>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute(ConvertParameter(parameter));
            }
        }

        /// <summary>
        /// 主动通知可执行状态变化。
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }

        /// <summary>
        /// 将 ICommand 的 object 参数转换为强类型参数。
        /// Unity Inspector 或手写绑定可能传入 null；当 T 是值类型时使用 default(T)，保持 CanExecute 和 Execute 行为一致。
        /// 其它类型转换复用绑定系统的 ConversionUtility，让 Command 参数和普通 UI 绑定保持一致。
        /// </summary>
        /// <param name="parameter">ICommand 入口收到的原始 object 参数。</param>
        /// <returns>转换为 T 后的强类型命令参数。</returns>
        private static T ConvertParameter(object parameter)
        {
            if (parameter == null)
            {
                return default;
            }

            if (parameter is T typedParameter)
            {
                return typedParameter;
            }

            try
            {
                return ConversionUtility.Convert<T>(parameter);
            }
            catch (Exception e)
            {
                throw new InvalidCastException(
                    $"命令参数类型错误，期望 {typeof(T).Name}，实际 {parameter.GetType().Name}。", e);
            }
        }
    }
}

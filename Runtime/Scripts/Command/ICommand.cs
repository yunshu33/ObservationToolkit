using System;

namespace VoyageForge.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 命令接口，用于把 UI 事件抽象为可执行业务行为。
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 命令可执行状态变化事件。
        /// </summary>
        event Action CanExecuteChanged;

        /// <summary>
        /// 判断命令当前是否可以执行。
        /// </summary>
        bool CanExecute(object parameter);

        /// <summary>
        /// 执行命令。
        /// </summary>
        void Execute(object parameter);

        /// <summary>
        /// 主动触发可执行状态变化通知。
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}

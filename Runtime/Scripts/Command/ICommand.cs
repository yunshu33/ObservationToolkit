using System;

namespace Voyage.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 是否可以执行的状态变更事件
        /// </summary>
        event Action CanExecuteChanged;

        /// <summary>
        /// 是否可以执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        bool CanExecute(object parameter);

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        void Execute(object parameter);
        
        /// <summary>
        /// 刷新执行状态
        /// </summary>
        void RaiseCanExecuteChanged();
    }
}

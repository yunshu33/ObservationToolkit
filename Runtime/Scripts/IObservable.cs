using System.Runtime.CompilerServices;
using VoyageForge.ObservationToolkit.Runtime.Command;

namespace VoyageForge.ObservationToolkit.Runtime
{
    /// <summary>
    /// 可观察对象接口。
    /// 实现该接口后，对象即可通过 BindingHandler 分发属性变化通知。
    /// </summary>
    public interface IObservable
    {
        /// <summary>
        /// 绑定处理器。
        /// 该属性不应参与 IL 观察织入，否则会造成递归通知。
        /// </summary>
        [IgnoreObservation]
        public BindingHandler BindingHandler { get; set; }

        /// <summary>
        /// 触发属性变化通知。
        /// 该入口会先分发普通属性绑定，再刷新通过 CommandManager 观察该属性的命令可执行状态。
        /// </summary>
        /// <typeparam name="V">发生变化的属性值类型。</typeparam>
        /// <param name="value">属性变化后的新值。</param>
        /// <param name="propertyName">发生变化的属性名，默认由调用方成员名自动填充。</param>
        public void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            BindingHandler?.OnPropertyChanged(value, propertyName);
            CommandManager.RaiseCanExecuteChanged(this, propertyName);
        }
    }

    /// <summary>
    /// 带数据模型的可观察对象接口。
    /// </summary>
    /// <typeparam name="TData">被包装的数据模型类型。</typeparam>
    public interface IObservable<TData> : IObservable
    {
        /// <summary>
        /// 被包装的数据模型。
        /// 该属性不应参与 IL 观察织入。
        /// </summary>
        [IgnoreObservation]
        public TData Data { get; set; }
    }
}

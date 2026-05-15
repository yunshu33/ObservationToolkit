using System.Runtime.CompilerServices;

namespace Voyage.ObservationToolkit.Runtime
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
        /// </summary>
        public void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            BindingHandler?.OnPropertyChanged(value, propertyName);
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

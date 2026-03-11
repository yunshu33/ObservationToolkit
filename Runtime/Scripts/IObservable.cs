using System.Runtime.CompilerServices;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 可观察
    /// </summary>
    public interface IObservable
    {
        /// <summary>
        /// 绑定处理程序
        /// </summary>
        [IgnoreObservation] 
        public BindingHandler BindingHandler { get; set; }

        public void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            BindingHandler?.OnPropertyChanged(value, propertyName);
        }
    }

    /// <summary>
    /// 泛型可观察接口，用于包装 Model 数据
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IObservable<TData> : IObservable
    {
        /// <summary>
        /// 包装的 Model 数据
        /// </summary>
        [IgnoreObservation]
        public TData Data { get; set; }
    }
}
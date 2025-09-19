using System.Runtime.CompilerServices;

namespace LJVoyage.ObservationToolkit.Runtime
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
}
using System.Runtime.CompilerServices;

namespace LJVoyage.Game.Runtime.Mvvm
{
    /// <summary>
    /// 绑定句柄持有者
    /// </summary>
    public interface IBindingHolder
    {
        public BindingHandler BindingHandler { get; set; }

        public void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            BindingHandler?.OnPropertyChanged(value, propertyName);
        }
    }
}
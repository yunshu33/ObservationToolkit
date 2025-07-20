using MvvmToolkit.Converter;
using MvvmToolkit.Proxy;
using UnityEngine.EventSystems;

namespace MvvmToolkit.UGUI
{
    public abstract class TwoWayUGUIBinderBase<T, TProperty, U, UProperty> : OneWayUGUIBinderBase<T, TProperty, U, UProperty>,
        ITwoWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        protected TwoWayUGUIBinderBase(BindingSource<T, TProperty> bindingSource, U target) : base(bindingSource, target)
        {
            
        }

        public virtual void TwoWay()
        {
            
        }
        
        public abstract void TwoWay(IConvert<TProperty, UProperty> convert, IConvert<UProperty, TProperty> convert2);
    }
}
using MvvmToolkit.Converter;
using MvvmToolkit.Proxy;
using UnityEngine.EventSystems;

namespace MvvmToolkit.UGUI
{
    public abstract class OneWayUGUIBinderBase<T, TProperty, U, UProperty> : UGUIBinder<T, TProperty, U,UProperty>,
        IOneWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        protected OneWayUGUIBinderBase(BindingSource<T, TProperty> bindingSource, U target) : base(bindingSource,
            target)
        {
        }

        

        public override void OneWay()
        {
            
        }
    }
}
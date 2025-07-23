using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class UGUIBinder<S, SProperty, U,UProperty> : Binder<S, SProperty,UProperty> where U : UIBehaviour
    {
        protected readonly BindingSource<S, SProperty> _bindingSource;

        protected readonly U _target;

        protected UGUIBinder(BindingSource<S, SProperty> bindingSource, U target)
        {
            _bindingSource = bindingSource;

            _target = target;
        }
    }
}
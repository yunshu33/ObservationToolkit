using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class UGUIBinder<S, SProperty, U, UProperty> : Binder<S, SProperty, UProperty> where U : UIBehaviour
    {
        protected readonly BindingSource<S, SProperty> _bindingSource;

        protected readonly U _target;

        protected readonly Binding<S, SProperty> _binding;

        protected bool isBinding = false;

        protected IConvert<SProperty, UProperty> _converter;
        
        private string _hashCode;


        protected UGUIBinder(BindingSource<S, SProperty> bindingSource, U target, Binding<S, SProperty> binding)
        {
            _bindingSource = bindingSource;

            _target = target;
            
            _binding = binding;
        }

        public override string HashCode => _target.GetHashCode().ToString();
    }
}
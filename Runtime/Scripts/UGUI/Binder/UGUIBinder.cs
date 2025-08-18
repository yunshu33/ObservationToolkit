using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class UGUIBinder<S, SProperty, U, UProperty> : Binder<S, SProperty, UProperty>
        where U : UIBehaviour 
    {
        protected readonly U _target;


        protected bool isBinding = false;

        protected IConvert<SProperty, UProperty> _converter;

        private string _hashCode;

        protected UGUIBinder(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(handler,
            binding)
        {
            _target = target;
        }


        public override string HashCode => _target.GetHashCode().ToString();
    }
}
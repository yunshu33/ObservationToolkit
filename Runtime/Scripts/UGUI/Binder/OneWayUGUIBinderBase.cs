using System;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class OneWayUGUIBinderBase<T, TProperty, U, UProperty> : UGUIBinder<T, TProperty, U, UProperty>,
        IOneWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        protected OneWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<T, TProperty> binding)
            : base(target, handler, binding)
        {
        }

        public override void OneWay()
        {
            if (!isBinding)
            {
                _binding.Bind(this);
                isBinding = true;
            }
        }
    }
}
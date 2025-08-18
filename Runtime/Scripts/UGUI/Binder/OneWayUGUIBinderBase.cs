using System;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class OneWayUGUIBinderBase<S, SProperty, U, UProperty> : UGUIBinder<S, SProperty, U, UProperty>,
        IOneWayBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        protected OneWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding)
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
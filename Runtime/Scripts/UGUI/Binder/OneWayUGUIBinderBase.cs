using System;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class OneWayUGUIBinderBase<S, SProperty, U, UProperty> : UGUIBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        protected OneWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding)
            : base(target, handler, binding)
        {
            
        }

        public override IDisposableBinding OneWay()
        {
            if (!isBinding)
            {
                _binding.Bind(this);
                isBinding = true;
            }
            return this;
        }
        
        public override IDisposableBinding OneWay(IConvert<SProperty, UProperty> convert)
        {
           _convert = convert;
           return OneWay();
        }

        public override void Unbind()
        {
            if (isBinding)
            {
                try
                {
                    _binding.Unbind(this.HashCode);
                    isBinding = false;
                    NotifyDisposed();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
        
        public override void OnUnbind()
        {
            isBinding = false;
        }
    }
}
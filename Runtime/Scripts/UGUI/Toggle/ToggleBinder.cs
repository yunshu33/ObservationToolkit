using System;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public class ToggleBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Toggle, bool>
    {
        public ToggleBinder(Toggle target, Action<bool> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    public class ToggleBindingEventProxy : UIBindingEventProxy<Toggle, bool>
    {
        public override void SetValue(bool value)
        {
            Target.isOn = value;
        }
    }
}
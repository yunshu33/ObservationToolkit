using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
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
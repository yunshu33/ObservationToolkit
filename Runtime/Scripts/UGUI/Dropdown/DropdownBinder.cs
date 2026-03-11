using System;
using Voyage.ObservationToolkit.Runtime;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Dropdown 双向绑定器 (int)
    /// </summary>
    public class DropdownBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Dropdown, int>
    {
        public DropdownBinder(Dropdown target, Action<int> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    public class DropdownBindingEventProxy : UIBindingEventProxy<Dropdown, int>
    {
        public override void SetValue(int value)
        {
            Target.value = value;
        }
    }
}

using System;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class SliderBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Slider, float>
    {
        public SliderBinder(Slider target, Action<float> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    public class SliderBindingEventProxy : UIBindingEventProxy<Slider, float>
    {
        public override void SetValue(float value)
        {
            Target.value = value;
        }
    }
}
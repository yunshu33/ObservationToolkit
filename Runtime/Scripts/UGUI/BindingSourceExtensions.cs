using System.Net.Mime;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public static class BindingSourceExtensions
    {
        public static TextUGUIBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Text target)
        {
            if (target.gameObject.TryGetComponent<TextBindingEventProxy>(out var proxy))
            {
                
            }
            else
            {
                proxy = target.gameObject.AddComponent<TextBindingEventProxy>();
            }

            return new TextUGUIBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        public static InputFieldUGUIBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            InputField target)
        {
            if (target.gameObject.TryGetComponent<InputFieldBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<InputFieldBindingEventProxy>();
            }

            return new InputFieldUGUIBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }
        
        
        public static ToggleBinder<S,SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Toggle target)
        {
            if (target.gameObject.TryGetComponent<ToggleBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<ToggleBindingEventProxy>();
            }

            return new ToggleBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }
        
        public static SliderBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Slider target)
        {
            if (target.gameObject.TryGetComponent<SliderBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<SliderBindingEventProxy>();
            }

            return new SliderBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }
    }
}
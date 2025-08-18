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
    }
}
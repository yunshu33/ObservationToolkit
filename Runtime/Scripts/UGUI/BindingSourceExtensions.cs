using System.Net.Mime;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public static class BindingSourceExtensions
    {
        public static TextUGUIBinder<S,SProperty> To<S,SProperty>(this BindingSource<S,SProperty> binder, Text target)
        {
          return new TextUGUIBinder<S, SProperty>(binder,target,binder.Binding);
        }

        public static InputFieldUGUIBinder<T,TProperty> To<T,TProperty>(this BindingSource<T,TProperty> binder, InputField target)
        {
          return new InputFieldUGUIBinder<T, TProperty>(binder,target,binder.Binding);
        }
    }
}
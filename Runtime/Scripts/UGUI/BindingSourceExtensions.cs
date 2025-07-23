using System.Net.Mime;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public static class BindingSourceExtensions
    {
        public static TextUGUIBinder<T,TProperty> To<T,TProperty>(this BindingSource<T,TProperty> binder, Text target)
        {
          return new TextUGUIBinder<T, TProperty>(binder,target);
        }

        public static InputFieldUGUIBinder<T,TProperty> To<T,TProperty>(this BindingSource<T,TProperty> binder, InputField target)
        {
          return new InputFieldUGUIBinder<T, TProperty>(binder,target);
        }
    }
}
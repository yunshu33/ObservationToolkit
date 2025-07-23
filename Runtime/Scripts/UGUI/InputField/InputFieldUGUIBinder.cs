
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class InputFieldUGUIBinder<T, TProperty> : TwoWayUGUIBinderBase<T, TProperty, InputField, string>
    {
        public InputFieldUGUIBinder(BindingSource<T, TProperty> bindingSource, InputField target) : base(bindingSource,
            target)
        {
            
        }

        public override void Unbind()
        {
            
        }

        public override void TwoWay(IConvert<TProperty, string> convert, IConvert<string, TProperty> convert2)
        {
            throw new System.NotImplementedException();
        }

        public override void OneWay(IConvert<TProperty, string> convert)
        {
            throw new System.NotImplementedException();
        }
    }
}
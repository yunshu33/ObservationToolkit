using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class TextUGUIBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Text, string>
    {
        private string _hashCode;


        public TextUGUIBinder(BindingSource<S, SProperty> bindingSource, Text target, Binding<S, SProperty> binding) :
            base(bindingSource, target, binding)
        {
        }

        protected override SProperty Convert(object value)
        {
            return default;
        }

        public override void Invoke(S source,  SProperty property)
        {
            if (_converter != null)
            {
                _target.text = _converter.Convert(property);
            }
            else
            {
                _target.text = property.ToString();
            }
        }

        public override void Unbind()
        {
            _binding.Unbind(this);
        }

        public override void OneWay(IConvert<SProperty, string> convert)
        {
            if (!isBinding)
            {
                _converter = convert;
                OneWay();
            }
        }
    }

    public class TextBindingEventProxy : UIBindingEventProxy<Text>
    {
        public override void SetValue(object value)
        {
            _target.text = value.ToString();
        }
    }
}
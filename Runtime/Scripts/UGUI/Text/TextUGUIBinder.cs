using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class TextUGUIBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Text, string>
    {
        private string _hashCode;

        public TextUGUIBinder(Text target, Action<string> handler, Binding<S, SProperty> binding) : base(target,
            handler, binding)
        {
        }

        public override void Invoke(S source, SProperty property)
        {
            if (_converter != null)
            {
                Handler?.Invoke(_converter.SourceConvertTarget(property));
            }
            else
            {
                Handler?.Invoke(property.ToString());
            }
        }


        public override void Unbind()
        {
            _binding.Unbind(this);
        }

        public override void OnUnbind()
        {
            
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

    public class TextBindingEventProxy : UIBindingEventProxy<Text, string>
    {
        public override void SetValue(string value)
        {
            Target.text = value;
        }
    }
}
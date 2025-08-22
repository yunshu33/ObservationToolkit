using System;
using System.Linq.Expressions;
using System.Reflection;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class InputFieldUGUIBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, InputField, string>
    {
        public InputFieldUGUIBinder(InputField target, Action<string> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }


        public override void Invoke(S source, SProperty property)
        {
            if (_convert != null)
            {
                Handler?.Invoke(_convert.SourceConvertTarget(property));
            }
            else
            {
                Handler?.Invoke(property.ToString());
            }
        }

        // public override void Unbind()
        // {
        //     throw new NotImplementedException();
        // }


      

        
        

       

        protected override SProperty TargetConvertSource(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return base.TargetConvertSource(value);
        }

        public override void OneWay(IConvert<SProperty, string> convert)
        {
            _convert = convert;
            OneWay();
        }

       
    }


    public class InputFieldBindingEventProxy : UIBindingEventProxy<InputField, string>
    {
        public override void SetValue(string value)
        {
            Target.text = value;
        }
    }
}
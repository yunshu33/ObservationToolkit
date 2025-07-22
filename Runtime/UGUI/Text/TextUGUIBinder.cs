using System;
using MvvmToolkit.Converter;
using MvvmToolkit.Proxy;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace MvvmToolkit.UGUI
{
    public class TextUGUIBinder<S, SProperty> :OneWayUGUIBinderBase<S, SProperty, Text, string>
    {
        private TextBindingEventProxy _eventProxy;

        public TextUGUIBinder(BindingSource<S, SProperty> bindingSource, Text target) : base(bindingSource, target)
        {
        }


        public override void OneWay(IConvert<SProperty, string> convert)
        {
            if (_eventProxy == null)
            {
                if (_target.TryGetComponent<TextBindingEventProxy>(out var eventProxy))
                {
                }
                else
                {
                    _eventProxy = _target.AddComponent<TextBindingEventProxy>();
                }
            }

            _bindingSource.ActionWrapper.Bind((Action<object>)_eventProxy.SetValue);
        }


        

       

        public override void Unbind()
        {
            if (_eventProxy == null)
            {
                if (_target.TryGetComponent<TextBindingEventProxy>(out var eventProxy))
                {
                    _bindingSource.ActionWrapper.Unbind((Action<object>)eventProxy.SetValue);
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
            else
            {
                _bindingSource.ActionWrapper.Unbind((Action<object>)_eventProxy.SetValue);
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
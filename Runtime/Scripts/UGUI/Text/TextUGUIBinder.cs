using System;
using System.Collections.Generic;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public class TextUGUIBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Text, string>
    {
        public TextUGUIBinder(Text target, Action<string> handler, Binding<S, SProperty> binding) : base(target,
            handler, binding)
        {
        }

        public override void Invoke(S source, SProperty property)
        {
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                // UnityEngine.Debug.Log($"[TextBinder] 触发重复值过滤: {property}");
                return;
            }

            _lastSValue = property;
            _hasLastSValue = true;

            if (_convert != null)
            {
                Handler?.Invoke(_convert.SourceConvertTarget(property));
            }
            else
            {
                // 特殊处理：如果没有转换器，直接调用 ToString()
                Handler?.Invoke(Convert.ToString(property, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
            }
        }
        
        // 移除 Unbind 和 OnUnbind，直接使用基类 OneWayUGUIBinderBase 的实现
        
        public override IDisposableBinding OneWay(IConvert<SProperty, string> convert)
        {
            return base.OneWay(convert);
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
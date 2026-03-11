using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public class InputFieldUGUIBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, InputField, string>
    {
        public InputFieldUGUIBinder(InputField target, Action<string> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }


        public override void Invoke(S source, SProperty property)
        {
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                // UnityEngine.Debug.Log($"[InputFieldBinder] 触发重复值过滤: {property}");
                return;
            }

            // 防覆盖检查：如果 InputField 的值（解析后）与新值相等，就不更新 UI，防止打断用户输入（如 "1." -> "1"）
            if (IsInputContentEqual(property))
            {
                _lastSValue = property;
                _hasLastSValue = true;
                return;
            }

            _lastSValue = property;
            _hasLastSValue = true;

            try
            {
                _isUpdatingUI = true;

                if (_convert != null)
                {
                    Handler?.Invoke(_convert.SourceConvertTarget(property));
                }
                else
                {
                    Handler?.Invoke(Convert.ToString(property, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
                }
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }

        private bool IsInputContentEqual(SProperty newValue)
        {
            try
            {
                // 使用基类的 TargetConvertSource 进行解析（包含 CultureInfo 处理）
                var currentVal = TargetConvertSource(_target.text);
                return EqualityComparer<SProperty>.Default.Equals(currentVal, newValue);
            }
            catch
            {
                return false;
            }
        }

        protected override SProperty TargetConvertSource(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return base.TargetConvertSource(value);
        }

        public override IDisposableBinding OneWay(IConvert<SProperty, string> convert)
        {
            return base.OneWay(convert);
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
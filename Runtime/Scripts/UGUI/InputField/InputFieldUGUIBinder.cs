using System;
using System.Collections.Generic;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// InputField 绑定器，支持字符串显示和 onValueChanged 双向回写。
    /// </summary>
    public class InputFieldUGUIBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, InputField, string>
    {
        /// <summary>
        /// 创建 InputField 绑定器。
        /// </summary>
        public InputFieldUGUIBinder(InputField target, Action<string> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }

        /// <summary>
        /// 执行 Model -> InputField 更新。
        /// </summary>
        public override void Invoke(S source, SProperty property)
        {
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                return;
            }

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
                Handler?.Invoke(_convert != null
                    ? _convert.SourceConvertTarget(property)
                    : ConversionUtility.Convert<string>(property) ?? string.Empty);
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }

        /// <summary>
        /// 判断当前输入内容解析后是否已经等于模型值。
        /// 这样可以避免用户输入 "1." 时被立即格式化成 "1"。
        /// </summary>
        private bool IsInputContentEqual(SProperty newValue)
        {
            try
            {
                var currentVal = TargetConvertSource(_target.text);
                return EqualityComparer<SProperty>.Default.Equals(currentVal, newValue);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将 InputField 字符串转换回模型值。
        /// </summary>
        protected override SProperty TargetConvertSource(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return base.TargetConvertSource(value);
        }

        /// <summary>
        /// 建立带转换器的单向绑定。
        /// </summary>
        public override IDisposableBinding OneWay(IConvert<SProperty, string> convert)
        {
            return base.OneWay(convert);
        }
    }

    /// <summary>
    /// InputField 事件代理，统一封装 InputField.text 写入。
    /// </summary>
    public class InputFieldBindingEventProxy : UIBindingEventProxy<InputField, string>
    {
        /// <summary>
        /// 设置 InputField.text。
        /// </summary>
        public override void SetValue(string value)
        {
            Target.text = value;
        }
    }
}

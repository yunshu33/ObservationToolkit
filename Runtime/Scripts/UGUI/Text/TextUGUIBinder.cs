using System;
using System.Collections.Generic;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Text 组件绑定器，负责将模型值显示为字符串。
    /// </summary>
    public class TextUGUIBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Text, string>
    {
        /// <summary>
        /// 创建 Text 绑定器。
        /// </summary>
        public TextUGUIBinder(Text target, Action<string> handler, Binding<S, SProperty> binding) : base(target,
            handler, binding)
        {
        }

        /// <summary>
        /// 执行 Text 更新。
        /// </summary>
        public override void Invoke(S source, SProperty property)
        {
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                return;
            }

            _lastSValue = property;
            _hasLastSValue = true;

            Handler?.Invoke(_convert != null
                ? _convert.SourceConvertTarget(property)
                : ConversionUtility.Convert<string>(property) ?? string.Empty);
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
    /// Text 事件代理，统一封装 Text.text 写入。
    /// </summary>
    public class TextBindingEventProxy : UIBindingEventProxy<Text, string>
    {
        /// <summary>
        /// 设置 Text.text。
        /// </summary>
        public override void SetValue(string value)
        {
            Target.text = value;
        }
    }
}

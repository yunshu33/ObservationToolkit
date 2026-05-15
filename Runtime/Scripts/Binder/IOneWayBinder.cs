using System;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 普通 C# 回调的单向绑定接口。
    /// </summary>
    public interface IOneWayBinder<S, SProperty, TProperty>
    {
        /// <summary>
        /// 建立单向绑定，使用默认类型转换。
        /// </summary>
        IDisposableBinding OneWay();

        /// <summary>
        /// 建立单向绑定，使用自定义转换器。
        /// </summary>
        IDisposableBinding OneWay(IConvert<SProperty, TProperty> convert);

        /// <summary>
        /// 建立单向绑定，使用模型到目标的转换函数。
        /// </summary>
        IDisposableBinding OneWay(Func<SProperty, TProperty> sourceToTarget);

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        void Unbind();
    }
}

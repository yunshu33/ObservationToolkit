using System;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 组件的单向绑定接口。
    /// </summary>
    public interface IOneWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        /// <summary>
        /// 建立单向绑定，使用默认类型转换。
        /// </summary>
        IDisposableBinding OneWay();

        /// <summary>
        /// 建立单向绑定，使用自定义转换器。
        /// </summary>
        IDisposableBinding OneWay(IConvert<TProperty, UProperty> convert);

        /// <summary>
        /// 建立单向绑定，使用模型到 UI 的转换函数。
        /// </summary>
        IDisposableBinding OneWay(Func<TProperty, UProperty> sourceToTarget);
    }
}

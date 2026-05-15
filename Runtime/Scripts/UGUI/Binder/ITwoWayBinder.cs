using System;
using System.Linq.Expressions;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 组件的双向绑定接口。
    /// </summary>
    public interface ITwoWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        /// <summary>
        /// 建立双向绑定，使用默认类型转换。
        /// </summary>
        IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);

        /// <summary>
        /// 建立双向绑定，使用自定义转换器。
        /// </summary>
        IDisposableBinding TwoWay(
            Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            IConvert<TProperty, UProperty> convert);

        /// <summary>
        /// 建立双向绑定，使用一对转换函数。
        /// </summary>
        IDisposableBinding TwoWay(
            Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            Func<TProperty, UProperty> sourceToTarget,
            Func<UProperty, TProperty> targetToSource);

        /// <summary>
        /// 按指定 UI 事件解除双向绑定。
        /// </summary>
        void Unbind(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);
    }
}

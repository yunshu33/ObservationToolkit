using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 可观察对象的绑定扩展方法。
    /// </summary>
    public static class BindingExtensions
    {
        /// <summary>
        /// 获取或创建对象的 BindingHandler。
        /// </summary>
        public static BindingHandler Binding(this IObservable bindingHandler)
        {
            bindingHandler.BindingHandler ??= new BindingHandler(bindingHandler);
            return bindingHandler.BindingHandler;
        }

        /// <summary>
        /// 创建来自属性的绑定源。
        /// </summary>
        public static BindingSource<S, SProperty> For<S, SProperty>(this S binding,
            Expression<Func<S, SProperty>> propertyExpression) where S : class, IObservable
        {
            return Binding(binding).ObserveValue(propertyExpression);
        }

        /// <summary>
        /// 创建来自属性的绑定源，并在源头设置 object 入口转换器。
        /// 这个重载面向 IConvert 接口，适合把同一个转换器实例复用到多个绑定链。
        /// </summary>
        public static BindingSource<S, SProperty> For<S, SProperty>(this S binding,
            Expression<Func<S, SProperty>> propertyExpression,
            IConvert<SProperty, object> converter) where S : class, IObservable
        {
            return Binding(binding).ObserveValue(propertyExpression).With(converter);
        }

        /// <summary>
        /// 设置字段并在值变化时发送属性通知。
        /// </summary>
        public static bool SetField<SProperty>(this IObservable binding, ref SProperty field, SProperty value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<SProperty>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            binding.OnPropertyChanged(value, propertyName);
            return true;
        }
    }
}

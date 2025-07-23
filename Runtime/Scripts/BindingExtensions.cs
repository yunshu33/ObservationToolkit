using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace LJVoyage.ObservationToolkit.Runtime
{
    public static class BindingExtensions
    {
        public static BindingHandler Binding(this IBindingHolder bindingHandler)
        {
            bindingHandler.BindingHandler ??= new BindingHandler(bindingHandler);

            return bindingHandler.BindingHandler;
        }


        /// <summary>
        /// 来自 属性的绑定
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="propertyExpression"></param>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="SProperty"></typeparam>
        /// <returns></returns>
        public static BindingSource<S, SProperty> For<S, SProperty>(this S binding,
            Expression<Func<S, SProperty>> propertyExpression) where S : class, IBindingHolder
        {
            return Binding(binding).ObserveValue(propertyExpression);
        }

        /// <summary>
        /// 来自 字段的绑定
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <typeparam name="SProperty"></typeparam>
        /// <returns></returns>
        public static bool SetField<SProperty>(this IBindingHolder binding, ref SProperty field, SProperty value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<SProperty>.Default.Equals(field, value)) return false;
            field = value;
            binding.OnPropertyChanged(value, propertyName);
            return true;
        }
    }
}
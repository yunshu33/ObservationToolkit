using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MvvmToolkit;
using MvvmToolkit.Proxy;

namespace LJVoyage.Game.Runtime.Mvvm
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
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        public static BindingSource<T, TProperty> For<T, TProperty>(this T binding,
            Expression<Func<T, TProperty>> propertyExpression) where T : class, IBindingHolder
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
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static bool SetField<V>(this IBindingHolder binding, ref V field, V value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<V>.Default.Equals(field, value)) return false;
            field = value;
            binding.OnPropertyChanged(value, propertyName);
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MvvmToolkit;
using MvvmToolkit.Proxy;
using UnityEngine;

namespace LJVoyage.Game.Runtime.Mvvm
{
    /// <summary>
    /// 绑定句柄
    /// </summary>
    public class BindingHandler
    {
        protected readonly Dictionary<string, Binding> _binders = new();

        private readonly WeakReference<object> _source;

        public BindingHandler(object source)
        {
            _source = new WeakReference<object>(source);
        }

        public BindingSource<T, TProperty> ObserveValue<T, TProperty>(
            Expression<Func<T, TProperty>> propertyExpression) where T : class
        {
            if (propertyExpression.Body is not MemberExpression memberExpression ||
                memberExpression.Member.MemberType != System.Reflection.MemberTypes.Property)
            {
                throw new ArgumentException("属性指定错误，必须为属性表达式。");
            }

            var propertyName = memberExpression.Member.Name;

            if (_binders.TryGetValue(propertyName, out var binder))
                return new BindingSource<T, TProperty>(((Binding<T, TProperty>)binder).ActionWrapper);

            binder = new Binding<T, TProperty>(propertyName, _source);

            _binders[propertyName] = binder;

            return new BindingSource<T, TProperty>(((Binding<T, TProperty>)binder).ActionWrapper);
        }

        public virtual void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (_binders.TryGetValue(propertyName, out var proxy))
            {
                proxy.Invoke(value);
            }
        }
    }
}
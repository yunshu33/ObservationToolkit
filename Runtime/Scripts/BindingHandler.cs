using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定句柄
    /// </summary>
    public class BindingHandler
    {
        protected readonly Dictionary<string, Binding> _bindings = new();

        private readonly WeakReference<object> _source;

        public BindingHandler(object source)
        {
            _source = new WeakReference<object>(source);
        }

        public BindingSource<S, SProperty> ObserveValue<S, SProperty>(Expression<Func<S, SProperty>> propertyExpression)
            where S : class
        {
            if (propertyExpression.Body is not MemberExpression memberExpression ||
                memberExpression.Member.MemberType != System.Reflection.MemberTypes.Property)
            {
                throw new ArgumentException("属性指定错误，必须为属性表达式。");
            }

            var propertyName = memberExpression.Member.Name;

            if (_bindings.TryGetValue(propertyName, out var binding))
                return new BindingSource<S, SProperty>((Binding<S, SProperty>)binding);

            binding = new Binding<S, SProperty>(propertyName, _source);

            _bindings[propertyName] = binding;

            return new BindingSource<S, SProperty>((Binding<S, SProperty>)binding);
        }

        public virtual void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (_bindings.TryGetValue(propertyName, out var binding))
            {
                binding.Invoke(value);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定处理器，负责按属性名管理 Binding，并在属性变化时分发通知。
    /// </summary>
    public class BindingHandler
    {
        /// <summary>
        /// 属性名到绑定集合的映射。
        /// </summary>
        protected readonly Dictionary<string, Binding> _bindings = new();

        /// <summary>
        /// 源对象弱引用，避免绑定系统持有源对象导致无法回收。
        /// </summary>
        private readonly WeakReference<object> _source;

        /// <summary>
        /// 创建绑定处理器。
        /// </summary>
        public BindingHandler(object source)
        {
            _source = new WeakReference<object>(source);
        }

        /// <summary>
        /// 获取某个属性的绑定源。
        /// </summary>
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
            {
                return new BindingSource<S, SProperty>((Binding<S, SProperty>)binding);
            }

            binding = new Binding<S, SProperty>(propertyName, _source);
            _bindings[propertyName] = binding;

            return new BindingSource<S, SProperty>((Binding<S, SProperty>)binding);
        }

        /// <summary>
        /// 属性变化通知入口。
        /// </summary>
        public virtual void OnPropertyChanged<V>(V value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
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

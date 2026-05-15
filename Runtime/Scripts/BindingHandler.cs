using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Reflection;

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
        /// 表达式到属性名的全局缓存。
        /// 绑定创建时会频繁解析 m => m.Value，这里缓存后可减少重复表达式解析成本。
        /// </summary>
        private static readonly Dictionary<string, string> PropertyNameCache = new();

        /// <summary>
        /// 表达式缓存锁。
        /// </summary>
        private static readonly object PropertyNameCacheLock = new();

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
            var propertyName = GetPropertyName(propertyExpression);

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

        /// <summary>
        /// 从属性表达式中解析属性名。
        /// </summary>
        private static string GetPropertyName<S, SProperty>(Expression<Func<S, SProperty>> propertyExpression)
        {
            if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));

            var cacheKey = $"{typeof(S).FullName}:{propertyExpression.Body}";
            lock (PropertyNameCacheLock)
            {
                if (PropertyNameCache.TryGetValue(cacheKey, out var cachedName))
                {
                    return cachedName;
                }
            }

            if (propertyExpression.Body is not MemberExpression memberExpression ||
                memberExpression.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("属性指定错误，必须为属性表达式。");
            }

            var propertyName = memberExpression.Member.Name;
            lock (PropertyNameCacheLock)
            {
                PropertyNameCache[cacheKey] = propertyName;
            }

            return propertyName;
        }
    }
}

using System;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 属性绑定源，是链式 API 中 For(...) 的返回对象。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    public class BindingSource<S, SProperty>
    {
        /// <summary>
        /// 底层属性绑定集合。
        /// </summary>
        protected readonly Binding<S, SProperty> _binding;

        /// <summary>
        /// 底层属性绑定集合。
        /// </summary>
        public Binding<S, SProperty> Binding => _binding;

        /// <summary>
        /// object 入口的转换器。
        /// </summary>
        public IConvert<SProperty, object> Converter
        {
            get => _binding.Converter;
            set => _binding.Converter = value;
        }

        /// <summary>
        /// 创建绑定源。
        /// </summary>
        public BindingSource(Binding<S, SProperty> binding)
        {
            _binding = binding;
        }

        /// <summary>
        /// 设置 object 入口转换器并返回自身。
        /// 该入口面向 IConvert 接口，便于在多个绑定源之间复用同一个转换器实现。
        /// </summary>
        public BindingSource<S, SProperty> With(IConvert<SProperty, object> converter)
        {
            Converter = converter;
            return this;
        }

        /// <summary>
        /// 绑定到只接收目标值的回调。
        /// </summary>
        public StandardBinder<S, SProperty, TProperty> To<TProperty>(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, TProperty>(handler, _binding);
        }

        /// <summary>
        /// 绑定到只接收目标值的回调，并预设接口转换器。
        /// </summary>
        public StandardBinder<S, SProperty, TProperty> To<TProperty>(
            Action<TProperty> handler,
            IConvert<SProperty, TProperty> converter)
        {
            return To(handler).With(converter);
        }

        /// <summary>
        /// 绑定到同时接收源对象和目标值的回调。
        /// </summary>
        public StandardBinder<S, SProperty, TProperty> To<TProperty>(Action<S, TProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, TProperty>(multiHandler, _binding);
        }

        /// <summary>
        /// 绑定到同时接收源对象和目标值的回调，并预设接口转换器。
        /// </summary>
        public StandardBinder<S, SProperty, TProperty> To<TProperty>(
            Action<S, TProperty> multiHandler,
            IConvert<SProperty, TProperty> converter)
        {
            return To(multiHandler).With(converter);
        }

        /// <summary>
        /// 绑定到同类型单参数回调。
        /// </summary>
        public StandardBinder<S, SProperty, SProperty> To(Action<SProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, SProperty>(handler, _binding);
        }

        /// <summary>
        /// 绑定到同类型双参数回调。
        /// </summary>
        public StandardBinder<S, SProperty, SProperty> To(Action<S, SProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, SProperty>(multiHandler, _binding);
        }
    }
}

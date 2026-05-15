using System;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 泛型绑定器基类，负责把源属性值转换为目标类型，并调用目标回调。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    /// <typeparam name="TProperty">目标回调接收的类型。</typeparam>
    public abstract class Binder<S, SProperty, TProperty> : Binder<S, SProperty>
    {
        /// <summary>
        /// 只接收目标值的回调。
        /// </summary>
        private Action<TProperty> _handler;

        /// <summary>
        /// 同时接收源对象和目标值的回调。
        /// </summary>
        private Action<S, TProperty> _multiHandler;

        /// <summary>
        /// 源类型和目标类型相同时缓存的强类型单参数回调。
        /// </summary>
        private Action<SProperty> _cachedTypedHandler;

        /// <summary>
        /// 源类型和目标类型相同时缓存的强类型双参数回调。
        /// </summary>
        private Action<S, SProperty> _cachedTypedMultiHandler;

        /// <summary>
        /// 只接收目标值的回调。
        /// </summary>
        public Action<TProperty> Handler
        {
            get => _handler;
            set => _handler = value;
        }

        /// <summary>
        /// 同时接收源对象和目标值的回调。
        /// </summary>
        public Action<S, TProperty> MultiHandler
        {
            get => _multiHandler;
            set => _multiHandler = value;
        }

        /// <summary>
        /// 目标回调方法名，主要用于调试。
        /// </summary>
        private readonly string _methodName;

        /// <summary>
        /// 目标回调方法名。
        /// </summary>
        public override string MethodName => _methodName;

        /// <summary>
        /// 绑定唯一哈希，用于快速解绑。
        /// </summary>
        protected int _hash;

        /// <summary>
        /// 绑定唯一哈希。
        /// </summary>
        public override int HashCode => _hash;

        /// <summary>
        /// 自定义转换器。为空时使用默认转换工具。
        /// </summary>
        protected IConvert<SProperty, TProperty> _convert;

        /// <summary>
        /// 源属性类型和目标类型是否完全相同。
        /// </summary>
        protected readonly bool _isTypeEqual;

        /// <summary>
        /// 创建单参数回调绑定器。
        /// </summary>
        protected Binder(Action<TProperty> handler, Binding<S, SProperty> binding) : base(binding)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _methodName = handler.Method.Name;
            _hash = BuildHash(handler);
            _isTypeEqual = typeof(TProperty) == typeof(SProperty);

            if (_isTypeEqual)
            {
                _cachedTypedHandler = _handler as Action<SProperty>;
            }
        }

        /// <summary>
        /// 创建带源对象参数的回调绑定器。
        /// </summary>
        protected Binder(Action<S, TProperty> handler, Binding<S, SProperty> binding) : base(binding)
        {
            _multiHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _methodName = handler.Method.Name;
            _hash = BuildHash(handler);
            _isTypeEqual = typeof(TProperty) == typeof(SProperty);

            if (_isTypeEqual)
            {
                _cachedTypedMultiHandler = _multiHandler as Action<S, SProperty>;
            }
        }

        /// <summary>
        /// 将源属性值转换为目标值。
        /// </summary>
        protected TProperty SourceConvertTarget(SProperty value)
        {
            if (_convert != null)
            {
                return _convert.SourceConvertTarget(value);
            }

            if (_isTypeEqual)
            {
                return (TProperty)(object)value;
            }

            try
            {
                return ConversionUtility.Convert<TProperty>(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"无法将类型 {typeof(SProperty)} 转换为 {typeof(TProperty)}。Value: {value}", ex);
            }
        }

        /// <summary>
        /// 执行绑定回调。
        /// </summary>
        public override void Invoke(S source, SProperty property)
        {
            if (_isTypeEqual)
            {
                if (_cachedTypedMultiHandler != null)
                {
                    _cachedTypedMultiHandler.Invoke(source, property);
                    return;
                }

                if (_cachedTypedHandler != null)
                {
                    _cachedTypedHandler.Invoke(property);
                    return;
                }
            }

            TProperty targetValue = SourceConvertTarget(property);
            _multiHandler?.Invoke(source, targetValue);
            _handler?.Invoke(targetValue);
        }

        /// <summary>
        /// 根据委托构建绑定哈希。
        /// 哈希包含方法、目标对象和绑定器类型，用于区分同一属性上的不同目标回调。
        /// </summary>
        protected int BuildHash(Delegate d)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + (d.Method?.MetadataToken ?? 0);
                h = h * 31 + (d.Method?.GetHashCode() ?? 0);
                h = h * 31 + (d.Target?.GetHashCode() ?? 0);
                h = h * 31 + GetType().GetHashCode();
                return h;
            }
        }
    }

    /// <summary>
    /// 非泛型目标绑定器基类。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    public abstract class Binder<S, SProperty>
    {
        /// <summary>
        /// 绑定源。
        /// </summary>
        protected readonly Binding<S, SProperty> _binding;

        /// <summary>
        /// 绑定目标方法名。
        /// </summary>
        public abstract string MethodName { get; }

        /// <summary>
        /// 创建绑定器基类。
        /// </summary>
        protected Binder(Binding<S, SProperty> binding)
        {
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
        }

        /// <summary>
        /// 执行绑定。
        /// </summary>
        public abstract void Invoke(S source, SProperty property);

        /// <summary>
        /// 绑定被移除时调用。
        /// </summary>
        public abstract void OnUnbind();

        /// <summary>
        /// 绑定唯一哈希。
        /// </summary>
        public abstract int HashCode { get; }
    }
}

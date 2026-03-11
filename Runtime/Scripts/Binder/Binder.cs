using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 泛型绑定器基类，处理源属性到目标属性的转换与分发
    /// </summary>
    /// <typeparam name="S">源对象类型</typeparam>
    /// <typeparam name="SProperty">源属性类型</typeparam>
    /// <typeparam name="TProperty">目标属性类型</typeparam>    
    public abstract class Binder<S, SProperty, TProperty> : Binder<S, SProperty>
    {
        private Action<TProperty> _handler;
        private Action<S, TProperty> _multiHandler;

        private Action<SProperty> _cachedTypedHandler;
        private Action<S, SProperty> _cachedTypedMultiHandler;

        /// <summary>
        /// 单参数回调 (Value)
        /// </summary>
        public Action<TProperty> Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }

        /// <summary>
        /// 双参数回调 (Source, Value)
        /// </summary>
        public Action<S, TProperty> MultiHandler
        {
            get { return _multiHandler; }
            set { _multiHandler = value; }
        }

        private readonly string _methodName;

        /// <summary>
        /// 绑定的目标方法名
        /// </summary>
        public override string MethodName => _methodName;

        protected int _hash;

        /// <summary>
        /// 绑定的哈希值，用于唯一标识
        /// </summary>
        public override int HashCode => _hash;

        protected IConvert<SProperty, TProperty> _convert;

        /// <summary>
        /// 标记源属性类型与目标属性类型是否一致，用于优化调用路径
        /// </summary>
        protected readonly bool _isTypeEqual;

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


        protected TypeConverter _targetPropertyConverter;
        
        /// <summary>
        /// 将源属性值转换为目标属性值
        /// </summary>
        protected TProperty SourceConvertTarget(SProperty value)
        {
            if (_convert != null)
            {
                return _convert.SourceConvertTarget(value);
            }

            if (value == null)
            {
                // 处理值类型空值
                if (default(TProperty) != null)
                {
                    // 如果 TProperty 是值类型且不为空，返回默认值
                    return default;
                }
                return default;
            }

            if (_isTypeEqual)
            {
                return (TProperty)(object)value;
            }

            _targetPropertyConverter ??= TypeDescriptor.GetConverter(typeof(TProperty));
            

            if (_targetPropertyConverter.CanConvertFrom(typeof(SProperty)))
            {
                try
                {
                    return (TProperty)_targetPropertyConverter.ConvertFrom(value);
                }
                catch (Exception ex)
                {
                    throw  ex;
                    
                }
            }

            try
            {
                return (TProperty)System.Convert.ChangeType(value, typeof(TProperty));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"无法将类型 {typeof(SProperty)} 转换为 {typeof(TProperty)}", ex);
            }
        }

        /// <summary>
        /// 执行绑定回调
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="property">源属性值</param>
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
        /// 根据委托构建哈希值，作为绑定的唯一标识
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
    /// 绑定器基类 (非泛型目标)
    /// </summary>
    /// <typeparam name="S">源对象类型</typeparam>
    /// <typeparam name="SProperty">源属性类型</typeparam>
    public abstract class Binder<S, SProperty>
    {
        protected readonly Binding<S, SProperty> _binding;

        /// <summary>
        /// 绑定的方法名
        /// </summary>
        public abstract string MethodName { get; }

        protected Binder(Binding<S, SProperty> binding)
        {
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
        }

      

        /// <summary>
        /// 执行绑定
        /// </summary>
        public abstract void Invoke(S source, SProperty property);

        /// <summary>
        /// 当绑定被解除时调用
        /// </summary>
        public abstract void OnUnbind();

        /// <summary>
        /// 绑定的唯一哈希值
        /// </summary>
        public abstract int HashCode { get; }
    }
}
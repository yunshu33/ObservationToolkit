using System;
using System.ComponentModel;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">源</typeparam>
    /// <typeparam name="SProperty">源属性</typeparam>
    /// <typeparam name="TProperty">目标属性</typeparam>    
    public abstract class Binder<S, SProperty, TProperty> : Binder<S, SProperty>, IOneWayBinder<S, SProperty, TProperty>

    {
        private Action<TProperty> _handler;

        public Action<TProperty> Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }

        private Action<S, TProperty> _multiHandler;

        public Action<S, TProperty> MultiHandler
        {
            get { return _multiHandler; }
            set { _multiHandler = value; }
        }

        private readonly string _methodName;

        public override string MethodName => _methodName;

        private readonly string _hash;

        public override string HashCode => _hash;

        protected IConvert<SProperty, TProperty> _convert;

        /// <summary>
        /// 类型一致
        /// </summary>
        protected readonly bool _isTypeEqual;

        protected Binder(Action<TProperty> handler, Binding<S, SProperty> binding) : base(binding)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            _methodName = handler.Method.Name;

            _hash = BuildHash(handler);

            _isTypeEqual = typeof(TProperty) == typeof(SProperty);
        }

        protected Binder(Action<S, TProperty> handler, Binding<S, SProperty> binding) : base(binding)
        {
            _multiHandler = handler ?? throw new ArgumentNullException(nameof(handler));

            _methodName = handler.Method.Name;

            _hash = BuildHash(handler);

            _isTypeEqual = typeof(TProperty) == typeof(SProperty);
        }


        protected TypeConverter _targetPropertyConverter;
        
        protected TProperty SourceConvertTarget(SProperty value)
        {
            if (_convert != null)
            {
                return _convert.SourceConvertTarget(value);
            }

            if (value == null)
                return default;

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
                catch
                {
                    // 忽略，尝试下一步
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

        public override void Invoke(S source, SProperty property)
        {
            TProperty targetValue = SourceConvertTarget(property);

            _multiHandler?.Invoke(source, targetValue);

            _handler?.Invoke(targetValue);
        }

        public override void OneWay()
        {
            _binding.Bind(this);
        }

        public virtual void OneWay(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
            OneWay();
        }


        public override void OnUnbind()
        {
            
        }
        
        
     

        private static string BuildHash(Delegate d)
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + (d.Method?.MetadataToken ?? 0);
                h = h * 31 + (d.Method?.GetHashCode() ?? 0);
                h = h * 31 + (d.Target?.GetHashCode() ?? 0);
                return h.ToString();
            }
        }
    }


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

        public abstract void OneWay();

        public abstract void Invoke(S source, SProperty property);

        public abstract void Unbind();

        public abstract void OnUnbind();

        public abstract string HashCode { get; }
    }
}
using System;
using System.ComponentModel;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标准的绑定器  
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="SProperty"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class StandardBinder<S, SProperty, TProperty> : Binder<S, SProperty, TProperty>
    {
        private readonly Action<TProperty> _handler;
        private readonly Action<S, TProperty> _multiHandler;
        private readonly Binding<S, SProperty> _binding;
        private readonly string _hash;
        private IConvert<SProperty, TProperty> _convert;

        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _hash = BuildHash(handler);
            _binding.Bind(this);
        }

        public StandardBinder(Action<S, TProperty> handler, Binding<S, SProperty> binding)
        {
            _multiHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _hash = BuildHash(handler);
            _binding.Bind(this);
        }

        private TProperty ConvertTarget(SProperty value)
        {
            if (_convert != null)
            {
                return _convert.Convert(value);
            }

            if (value == null)
                return default;

            var typeConverter = TypeDescriptor.GetConverter(typeof(TProperty));
            
            if (typeConverter != null && typeConverter.CanConvertFrom(typeof(SProperty)))
            {
                try
                {
                    return (TProperty)typeConverter.ConvertFrom(value);
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
            TProperty targetValue = ConvertTarget(property);

            if (_multiHandler != null)
                _multiHandler(source, targetValue);
            else
                _handler?.Invoke(targetValue);
        }

        public override void OneWay()
        {
            // no-op
        }

        public override void OneWay(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
        }

        protected override SProperty Convert(object value)
        {
            return (SProperty)value;
        }

        public override void Unbind()
        {
            // user cleanup if needed
        }

        public override string HashCode => _hash;

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
}
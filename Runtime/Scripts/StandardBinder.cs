using System;
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

        public bool isBinding = false;

        private IConvert<SProperty, TProperty> _convert;

        private string _hashCode;

        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding)
        {
            _handler = handler;
            _binding = binding;
        }

        public StandardBinder(Action<S, TProperty> multiHandler, Binding<S, SProperty> binding)
        {
            _multiHandler = multiHandler;
            _binding = binding;
        }

        /// <summary>
        /// 传入 转化器  则使用转换器 将 SProperty 转化为 TProperty 再传入 处理器
        /// 否则 直接强转 传入 处理器
        /// 转换器分为两段
        /// obj 到 source
        /// 和 source 到 target
        /// </summary>
        public override void OneWay()
        {
            if (!isBinding)
            {
                _binding.Bind(this);

                isBinding = true;
            }
            else
            {
                throw new Exception("已经绑定");
            }
        }

        public override void OneWay(IConvert<SProperty, TProperty> convert)
        {
            if (!isBinding)
            {
                _convert = convert;
                OneWay();
            }
            else
            {
                throw new Exception("已经绑定");
            }
        }

        protected override SProperty Convert(object value)
        {
            return _convert.Convert(value);
        }

        public override void Invoke(S source, object obj, SProperty property)
        {
            TProperty tProperty;

            if (_convert != null)
            {
                var sProperty = _convert.Convert(obj);

                tProperty = _convert.Convert(sProperty);
            }
            else
            {
                tProperty = (TProperty)obj;
            }

            _handler?.Invoke(tProperty);
            _multiHandler?.Invoke(default, tProperty);
        }

        public override void Unbind()
        {
            _binding.Unbind(this);
        }

        public override string HashCode
        {
            get
            {
                var str = _handler != null ? _handler.GetHashCode().ToString() : _multiHandler.GetHashCode().ToString();

                Debug.Log(str);

                return str;
            }
        }
    }
}
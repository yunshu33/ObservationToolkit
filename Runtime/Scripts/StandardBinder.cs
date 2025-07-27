using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;

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
        private Action<TProperty> _handler;

        private Action<S, TProperty> _multiHandler;

        private Binding<S, SProperty> _binding;

        public bool isBinding = false;

        private IConvert<SProperty, TProperty> _convert;

        private string _hashCode;

        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding)
        {
            _handler = handler;
            _binding = binding;
        }

        public StandardBinder(Action<TProperty> handler)
        {
            _handler = handler;
        }

        public StandardBinder(Action<S, TProperty> multiHandler)
        {
            _multiHandler = multiHandler;
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
            _binding.Bind(this);

            isBinding = true;
        }

        public override void OneWay(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
            OneWay();
        }

        protected override SProperty Convert(object value)
        {
            return _convert.Convert(value);
        }

        public override void Invoke(S source, object arg1, SProperty arg2)
        {
            SProperty sProperty = arg2;

            TProperty tProperty;

            if (_convert != null)
            {
                sProperty = _convert.Convert(arg1);

                tProperty = _convert.Convert(sProperty);
            }
            else
            {
                tProperty = sProperty is TProperty compatible ? compatible : default;
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
            get => _handler != null ? _handler.GetHashCode().ToString() : _multiHandler.GetHashCode().ToString();
        }
    }
}
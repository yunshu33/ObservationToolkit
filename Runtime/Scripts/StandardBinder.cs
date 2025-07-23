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

        private ActionWrapper<S, TProperty> _actionWrapper;

        public bool isBinding = false;

        public StandardBinder(ActionWrapper<S, TProperty> actionWrapper, Action<TProperty> handler)
        {
            _handler = handler;
            _actionWrapper = actionWrapper;
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
            if (!isBinding)
            {
                if (_handler != null)
                {
                    _actionWrapper.Bind(_handler);
                }
                else
                {
                    _actionWrapper.Bind(_multiHandler);
                }
            }
        }

        public override void OneWay(IConvert<SProperty, TProperty> convert)
        {
            throw new NotImplementedException();
        }

        public override void Unbind()
        {
            if (_handler != null)
            {
                _actionWrapper.Unbind(_handler);
            }
            else if (_multiHandler != null)
            {
                _actionWrapper.Unbind(_multiHandler);
            }
        }
    }
}
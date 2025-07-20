using System;
using LJVoyage.Game.Runtime.Mvvm;
using MvvmToolkit.Converter;
using MvvmToolkit.Proxy;

namespace MvvmToolkit
{
    public class StandardBinder<S, SProperty ,TProperty> : Binder<S, SProperty ,TProperty>
    {
        private Action<SProperty> _handler;

        private Action<S, SProperty> _multiHandler;

        private ActionWrapper<S, SProperty> _actionWrapper;

        public bool isBinding = false;

        public StandardBinder(ActionWrapper<S, SProperty> actionWrapper, Action<SProperty> handler)
        {
            _handler = handler;
            _actionWrapper = actionWrapper;
        }

        public StandardBinder(ActionWrapper<S, SProperty> actionWrapper, Action<S, SProperty> multiHandler)
        {
            _multiHandler = multiHandler;
            _actionWrapper = actionWrapper;
        }

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
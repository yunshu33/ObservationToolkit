using System;
using LJVoyage.Game.Runtime.Mvvm;

namespace MvvmToolkit.Proxy
{
    public class BindingSource<S, SProperty>
    {
        private readonly ActionWrapper<S, SProperty> _actionWrapper;

        public ActionWrapper<S, SProperty> ActionWrapper => _actionWrapper;

        public BindingSource(ActionWrapper<S, SProperty> actionWrapper)
        {
            _actionWrapper = actionWrapper;
        }


        public Binder<S, SProperty, TProperty> To<TProperty>(Action<SProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, TProperty>(_actionWrapper, handler);
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<S, SProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, TProperty>(_actionWrapper, multiHandler);
        }
    }
}
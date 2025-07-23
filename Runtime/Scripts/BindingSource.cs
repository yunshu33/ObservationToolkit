using System;

namespace LJVoyage.ObservationToolkit.Runtime
{
    public class BindingSource<S, SProperty>
    {
        private readonly ActionWrapper<S, SProperty> _actionWrapper;

        public ActionWrapper<S, SProperty> ActionWrapper => _actionWrapper;

        public BindingSource(ActionWrapper<S, SProperty> actionWrapper)
        {
            _actionWrapper = actionWrapper;
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, TProperty>(handler);
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<S, TProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, TProperty>(multiHandler);
        }

        public Binder<S, SProperty, SProperty> To(Action<SProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, SProperty>(handler);
        }

        public Binder<S, SProperty, SProperty> To(Action<S, SProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, SProperty>(multiHandler);
        }
    }
}
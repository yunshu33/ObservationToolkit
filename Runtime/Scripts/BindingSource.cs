using System;

namespace LJVoyage.ObservationToolkit.Runtime
{
    public class BindingSource<S, SProperty>
    {
        
        private Binding<S, SProperty> _binding;

        public BindingSource(Binding<S, SProperty> binding)
        {
            _binding =  binding;
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, TProperty>(handler, _binding);
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
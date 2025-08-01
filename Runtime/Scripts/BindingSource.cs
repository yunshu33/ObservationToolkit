using System;
using LJVoyage.ObservationToolkit.Runtime.Converter;

namespace LJVoyage.ObservationToolkit.Runtime
{
    public class BindingSource<S, SProperty>
    {
        protected readonly Binding<S, SProperty> _binding;

        public Binding<S, SProperty> Binding
        {
            get { return _binding; }
        }
        
        public IConvert<SProperty, object> Converter
        {
            get => _binding.Converter;

            set => _binding.Converter = value; 
        }

        public BindingSource(Binding<S, SProperty> binding)
        {
            _binding = binding;
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, TProperty>(handler, _binding);
        }

        public Binder<S, SProperty, TProperty> To<TProperty>(Action<S, TProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, TProperty>(multiHandler, _binding);
        }


        public Binder<S, SProperty, SProperty> To(Action<SProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return new StandardBinder<S, SProperty, SProperty>(handler, _binding);
        }


        public Binder<S, SProperty, SProperty> To(Action<S, SProperty> multiHandler)
        {
            if (multiHandler == null) throw new ArgumentNullException(nameof(multiHandler));
            return new StandardBinder<S, SProperty, SProperty>(multiHandler, _binding);
        }
    }
}
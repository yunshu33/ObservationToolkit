using System;

namespace LJVoyage.ObservationToolkit.Runtime
{
    public class Binding<T, TProperty> : Binding 
    {
        private string _propertyName;

        private readonly WeakReference<object> _source;

        private ActionWrapper<T, TProperty> _actionWrapper = new();

        public ActionWrapper<T, TProperty> ActionWrapper => _actionWrapper;
        
        public Binding(string propertyName, WeakReference<object> source)
        {
            _propertyName = propertyName;
            _source = source;
        }

        public void Bind(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _actionWrapper.Bind(handler);
        }

        public void To(Action<TProperty> handler) => Bind(handler);

        public void From(Action<TProperty> handler) => Unbind(handler);

        public void Unbind(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _actionWrapper.Unbind(handler);
        }


        public override void Invoke(object value)
        {
            if (_source.TryGetTarget(out var source))
            {
                _actionWrapper.Invoke(source, value);
            }
        }
    }

    public abstract class Binding
    {
        public abstract void Invoke(object value);
    }
}
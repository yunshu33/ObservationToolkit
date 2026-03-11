using System;
using System.Collections.Generic;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 使用函数 的has code 作为 唯一标识  绑定  解除绑定
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="SProperty"></typeparam>
    public class Binding<S, SProperty> : Binding, IBinding<SProperty>
    {
        private readonly string _propertyName;

        public string PropertyName => _propertyName;

        private IConvert<SProperty, object> _converter;

        public IConvert<SProperty, object> Converter
        {
            get => _converter;
            set => _converter = value;
        }

        private readonly WeakReference<object> _source;

        public S Source
        {
            get
            {
                if (!_source.TryGetTarget(out var obj))
                {
                    throw new Exception("源对象已被释放");
                }

                return (S)obj;
            }
        }

        private readonly Dictionary<int, Binder<S, SProperty>> _binders;

        public Binding(string propertyName, WeakReference<object> source)
        {
            _propertyName = propertyName;
            _source = source;

            _binders = new Dictionary<int, Binder<S, SProperty>>();
        }

        public void Bind(Binder<S, SProperty> binder)
        {
            _binders.Add(binder.HashCode, binder);
        }

        public void Unbind(int hashcode)
        {
            if (_binders.TryGetValue(hashcode, out Binder<S, SProperty> target))
            {
                var count = _binders.Count;

                _binders.Remove(hashcode);

                target.OnUnbind();

                UnityEngine.Debug.Log($"解绑成功 {count}-> {_binders.Count}");
            }
            else
            {
                throw new Exception($"未找到绑定 {hashcode}");
            }
        }

        public void Invoke(SProperty value)
        {
            if (!_source.TryGetTarget(out var obj))
            {
                throw new Exception("源对象已被释放");
            }

            S source = (S)obj;

            foreach (var binder in _binders.Values)
            {
                binder.Invoke(source, value);
            }
        }

        public override void Invoke<V>(V value)
        {
            if (value is SProperty typedValue)
            {
                Invoke(typedValue);
            }
            else
            {
                Invoke((object)value);
            }
        }

        public override void Invoke(object value)
        {
            if (!_source.TryGetTarget(out var obj))
            {
                throw new Exception("源对象已被释放");
            }

            S source = (S)obj;

            SProperty property;

            if (Converter != null)
            {
                property = Converter.ObjectConvertSource(value);
            }
            else
            {
                property = (SProperty)value;
            }

            foreach (var binder in _binders.Values)
            {
                binder.Invoke(source, property);
            }
        }
    }


    public abstract class Binding
    {
        public abstract void Invoke(object value);

        public abstract void Invoke<V>(V value);
    }

    public interface IBinding<in V>
    {
        void Invoke(V value);
    }
}
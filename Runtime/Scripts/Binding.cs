using System;
using System.Collections.Generic;
using LJVoyage.ObservationToolkit.Runtime.Converter;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 使用函数 的has code 作为 唯一标识  绑定  解除绑定
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="SProperty"></typeparam>
    public class Binding<S, SProperty> : Binding
    {
        private string _propertyName;

        private IConvert<SProperty, object> _converter;

        public IConvert<SProperty, object> Converter
        {
            get => _converter;
            set => _converter = value;
        }

        private readonly WeakReference<object> _source;

        private readonly Dictionary<string, Binder<S, SProperty>> _binders;

        public Binding(string propertyName, WeakReference<object> source)
        {
            _propertyName = propertyName;
            _source = source;

            _binders = new Dictionary<string, Binder<S, SProperty>>();
        }

        public void Bind(Binder<S, SProperty> binder)
        {
            _binders.Add(binder.HashCode, binder);
        }

        public void Unbind(Binder<S, SProperty> binder)
        {
            var hashcode = binder.HashCode;

            if (_binders.TryGetValue(binder.HashCode, out Binder<S, SProperty> binder2))
            {
                binder2.Unbind();
                _binders.Remove(hashcode);
            }
            else
            {
                throw new Exception("未找到绑定");
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
                property = Converter.Convert(value);
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
    }
}
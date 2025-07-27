using System;
using System.Collections.Generic;

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

        private readonly WeakReference<object> _source;

        private Dictionary<string, Binder<S, SProperty>> _binders;

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
            if (!_binders.ContainsKey(binder.HashCode))
            {
                _binders.Remove(binder.HashCode);
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

            var source = (S)obj;

            var property = (SProperty)value;

            foreach (var binder in _binders.Values)
            {
                binder.Invoke(source, value, property);
            }
        }
    }


    public abstract class Binding
    {
        public abstract void Invoke(object value);
    }
}
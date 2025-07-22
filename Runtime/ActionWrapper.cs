using System;
using System.Collections.Generic;
using UnityEngine;

namespace LJVoyage.Game.Runtime.Mvvm
{
    public class ActionWrapper<T, TProperty> : ActionWrapper, IDisposable
    {
        private Action<T, TProperty> _multiHandler;

        private Action<TProperty> _singleHandler;

        private Action<object> _objectHandler;

        public int Count => _multiHandler != null ? _multiHandler.GetInvocationList().Length : 0;

        public void Bind(Action<T, TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _multiHandler += handler;
        }

        public void Bind(Action<TProperty> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _singleHandler += handler;
        }

        public void Bind(Action<object> handler)
        {
            _objectHandler += handler;
        }

        public void Unbind(Action<object> handler)
        {
            _objectHandler -= handler;
        }

        public void Unbind(Action<T, TProperty> handler)
        {
            if (handler != null)
                _multiHandler -= handler;
        }

        public void Unbind(Action<TProperty> handler)
        {
            if (handler != null) _singleHandler -= handler;
        }


        // 调用所有注册的处理器
        public void InvokeAction(T arg0, TProperty arg1)
        {
            _multiHandler?.Invoke(arg0, arg1);
            _singleHandler?.Invoke(arg1);
        }

        public override void Invoke(object arg0, object arg1)
        {
            _objectHandler?.Invoke(arg1);

            if (arg1 is TProperty typedArg)
            {
                InvokeAction((T)arg0, typedArg);
            }
            else
            {
                throw new ArgumentException(
                    $"Argument type {arg1.GetType()} does not match expected type {typeof(TProperty)}");
            }
        }

        public void Dispose() => _multiHandler = null;
    }

    public abstract class ActionWrapper
    {
        public abstract void Invoke(object arg0, object arg1);
    }
}
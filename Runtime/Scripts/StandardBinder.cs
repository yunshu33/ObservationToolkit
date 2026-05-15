using System;
using Voyage.ObservationToolkit.Runtime.Converter;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标准绑定器，用于普通 C# 回调绑定。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    /// <typeparam name="TProperty">目标回调接收的类型。</typeparam>
    public class StandardBinder<S, SProperty, TProperty> : Binder<S, SProperty, TProperty>, IDisposableBinding
    {
        /// <summary>
        /// 绑定释放时触发，用于从 BindingContext 中移除自身。
        /// </summary>
        public event Action OnDisposed;

        /// <summary>
        /// 创建单参数回调绑定器。
        /// </summary>
        /// <param name="handler">目标回调。</param>
        /// <param name="binding">绑定源。</param>
        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        /// <summary>
        /// 创建带源对象参数的回调绑定器。
        /// </summary>
        /// <param name="handler">目标回调，参数为源对象和转换后的值。</param>
        /// <param name="binding">绑定源。</param>
        public StandardBinder(Action<S, TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        /// <summary>
        /// 建立单向绑定。
        /// </summary>
        public IDisposableBinding OneWay()
        {
            _binding.Bind(this);
            return this;
        }

        /// <summary>
        /// 建立带转换器的单向绑定。
        /// </summary>
        public virtual IDisposableBinding OneWay(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
            return OneWay();
        }

        /// <summary>
        /// 预先设置转换器，但暂不建立绑定。
        /// 适合在 To(..., converter) 这种面向接口的链式入口中复用转换器对象。
        /// </summary>
        public StandardBinder<S, SProperty, TProperty> With(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
            return this;
        }

        /// <summary>
        /// 建立带转换函数的单向绑定。
        /// </summary>
        public virtual IDisposableBinding OneWay(Func<SProperty, TProperty> sourceToTarget)
        {
            return OneWay(new DelegateConvert<SProperty, TProperty>(sourceToTarget));
        }

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        public void Unbind()
        {
            try
            {
                _binding.Unbind(this.HashCode);
                OnDisposed?.Invoke();
                OnDisposed = null;
            }
            catch (Exception)
            {
                // 重复解绑时保持幂等，避免生命周期清理阶段抛异常。
            }
        }

        /// <summary>
        /// 释放资源，等同于 Unbind。
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        /// <summary>
        /// Binding 移除此绑定器时的回调；标准绑定没有额外资源需要释放。
        /// </summary>
        public override void OnUnbind()
        {
        }
    }
}

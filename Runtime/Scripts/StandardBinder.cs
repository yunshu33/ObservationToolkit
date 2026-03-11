using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标准的绑定器，用于通用的 C# 属性/字段绑定
    /// </summary>
    /// <typeparam name="S">源对象类型</typeparam>
    /// <typeparam name="SProperty">源属性类型</typeparam>
    /// <typeparam name="TProperty">目标属性类型</typeparam>
    public class StandardBinder<S, SProperty, TProperty> : Binder<S, SProperty, TProperty>, IDisposableBinding
    {
        /// <summary>
        /// 当绑定被销毁时触发的事件
        /// </summary>
        public event Action OnDisposed;

        /// <summary>
        /// 构造函数 (单参数回调)
        /// </summary>
        /// <param name="handler">目标属性的回调</param>
        /// <param name="binding">绑定源</param>
        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        /// <summary>
        /// 构造函数 (双参数回调)
        /// </summary>
        /// <param name="handler">包含源对象和属性值的回调</param>
        /// <param name="binding">绑定源</param>
        public StandardBinder(Action<S, TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        /// <summary>
        /// 建立单向绑定
        /// 源属性变化时，自动更新目标属性
        /// </summary>
        /// <returns>返回自身，用于链式调用或生命周期管理</returns>
        public IDisposableBinding OneWay()
        {
            _binding.Bind(this);
            return this;
        }

        /// <summary>
        /// 建立带转换器的单向绑定
        /// </summary>
        /// <param name="convert">值转换器</param>
        /// <returns>返回自身</returns>
        public virtual IDisposableBinding OneWay(IConvert<SProperty, TProperty> convert)
        {
            _convert = convert;
            return OneWay();
        }

        /// <summary>
        /// 手动解除绑定
        /// </summary>
        public void Unbind()
        {
            try
            {
                _binding.Unbind(this.HashCode);
                // 解绑成功后，触发 OnDisposed 事件
                OnDisposed?.Invoke();
                OnDisposed = null; // 清空事件链，防止内存泄漏
            }
            catch (Exception)
            {
                // 忽略已解绑的异常
            }
        }

        /// <summary>
        /// 释放资源 (等同于 Unbind)
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        public override void OnUnbind()
        {
            // 当从 Binding 被移除时，也会触发
        }
    }
}
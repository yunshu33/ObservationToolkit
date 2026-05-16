using System;
using System.Collections.Generic;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 绑定器基类，负责保存目标组件、去重和生命周期释放。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    /// <typeparam name="U">UGUI 组件类型。</typeparam>
    /// <typeparam name="UProperty">UGUI 属性或事件值类型。</typeparam>
    public abstract class UGUIBinder<S, SProperty, U, UProperty> : Binder<S, SProperty, UProperty>,
        IOneWayBinder<S, SProperty, U, UProperty>, IDisposableBinding where U : UIBehaviour
    {
        /// <summary>
        /// 绑定释放时触发，用于 BindingContext 自动移除。
        /// </summary>
        public event Action OnDisposed;

        /// <summary>
        /// 目标 UGUI 组件。
        /// </summary>
        protected readonly U _target;

        /// <summary>
        /// 当前是否已经绑定到 Binding。
        /// </summary>
        protected bool isBinding = false;

        /// <summary>
        /// 上一次收到的源属性值，用于跳过重复 UI 更新。
        /// </summary>
        protected SProperty _lastSValue;

        /// <summary>
        /// 是否已经缓存过源属性值。
        /// </summary>
        protected bool _hasLastSValue = false;

        /// <summary>
        /// 当前绑定使用的转换器。
        /// 暴露为属性是为了让 To(component, converter) 可以先设置转换器，再由调用方选择 OneWay 或 TwoWay。
        /// </summary>
        public IConvert<SProperty, UProperty> Converter
        {
            get => _convert;
            set => _convert = value;
        }

        /// <summary>
        /// 目标组件哈希缓存。
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// 创建 UGUI 绑定器。
        /// </summary>
        protected UGUIBinder(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(handler,
            binding)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _hashCode = target.GetHashCode();
        }

        /// <summary>
        /// 绑定唯一哈希。UGUI 单向绑定默认以目标组件为唯一键。
        /// </summary>
        public override int HashCode => _hashCode;

        /// <summary>
        /// 执行 Model -> UI 更新，并过滤重复值，减少 UI 重绘。
        /// </summary>
        public override void Invoke(S source, SProperty property)
        {
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                return;
            }

            _lastSValue = property;
            _hasLastSValue = true;

            base.Invoke(source, property);
        }

        /// <summary>
        /// 建立默认单向绑定。
        /// </summary>
        public abstract IDisposableBinding OneWay();

        /// <summary>
        /// 建立带转换器的单向绑定。
        /// </summary>
        public abstract IDisposableBinding OneWay(IConvert<SProperty, UProperty> convert);

        /// <summary>
        /// 建立带转换函数的单向绑定。
        /// </summary>
        public abstract IDisposableBinding OneWay(Func<SProperty, UProperty> sourceToTarget);

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        public abstract void Unbind();

        /// <summary>
        /// 释放资源，等同于 Unbind。
        /// </summary>
        public void Dispose()
        {
            Unbind();
        }

        /// <summary>
        /// 通知外部绑定已经释放，并清空事件引用避免泄漏。
        /// </summary>
        protected void NotifyDisposed()
        {
            OnDisposed?.Invoke();
            OnDisposed = null;
        }
    }
}

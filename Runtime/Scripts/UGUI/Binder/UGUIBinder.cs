using System;
using System.Collections.Generic;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 绑定器基类，专门处理 Unity UI 组件的绑定
    /// </summary>
    /// <typeparam name="S">源对象类型</typeparam>
    /// <typeparam name="SProperty">源属性类型</typeparam>
    /// <typeparam name="U">UI 组件类型 (如 Text, Button)</typeparam>
    /// <typeparam name="UProperty">UI 属性类型 (如 string, bool)</typeparam>
    public abstract class UGUIBinder<S, SProperty, U, UProperty> : Binder<S, SProperty, UProperty>,
        IOneWayBinder<S, SProperty, UProperty>, IDisposableBinding where U : UIBehaviour
    {
        public event Action OnDisposed;

        /// <summary>
        /// 绑定的目标 UI 组件
        /// </summary>
        protected readonly U _target;
        
        /// <summary>
        /// 标记当前是否正在进行绑定操作（防止循环更新）
        /// </summary>
        protected bool isBinding = false;

        /// <summary>
        /// 缓存上一次的源属性值，用于去重
        /// </summary>
        protected SProperty _lastSValue;

        /// <summary>
        /// 标记是否已经有了上一次的值
        /// </summary>
        protected bool _hasLastSValue = false;


        private int _hashCode;

        protected UGUIBinder(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(handler,
            binding)
        {
            _target = target;
        }


        /// <summary>
        /// 使用目标 UI 组件的 HashCode 作为绑定的标识
        /// </summary>
        public override int HashCode => _target.GetHashCode();


        /// <summary>
        /// 执行绑定回调，包含防抖/去重逻辑
        /// </summary>
        public override void Invoke(S source, SProperty property)
        {
            // 如果新值与旧值相同，则不触发更新，避免不必要的 UI 重绘
            if (_hasLastSValue && EqualityComparer<SProperty>.Default.Equals(_lastSValue, property))
            {
                return;
            }

            _lastSValue = property;
            _hasLastSValue = true;

            base.Invoke(source, property);
        }

        public abstract IDisposableBinding OneWay();

        public abstract IDisposableBinding OneWay(IConvert<SProperty, UProperty> convert);
        
        public abstract void Unbind();

        public void Dispose()
        {
            Unbind();
        }
        
        protected void NotifyDisposed()
        {
            OnDisposed?.Invoke();
            OnDisposed = null;
        }
    }
}
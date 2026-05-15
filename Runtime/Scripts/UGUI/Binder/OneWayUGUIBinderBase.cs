using System;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 单向绑定基类，负责注册和移除 Model -> UI 的绑定关系。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    /// <typeparam name="U">UGUI 组件类型。</typeparam>
    /// <typeparam name="UProperty">UGUI 属性值类型。</typeparam>
    public abstract class OneWayUGUIBinderBase<S, SProperty, U, UProperty> :
        UGUIBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        /// <summary>
        /// 创建 UGUI 单向绑定基类。
        /// </summary>
        protected OneWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding)
            : base(target, handler, binding)
        {
        }

        /// <summary>
        /// 建立单向绑定。
        /// </summary>
        public override IDisposableBinding OneWay()
        {
            if (!isBinding)
            {
                _binding.Bind(this);
                isBinding = true;
            }

            return this;
        }

        /// <summary>
        /// 建立带转换器的单向绑定。
        /// </summary>
        public override IDisposableBinding OneWay(IConvert<SProperty, UProperty> convert)
        {
            _convert = convert;
            return OneWay();
        }

        /// <summary>
        /// 建立带转换函数的单向绑定。
        /// </summary>
        public override IDisposableBinding OneWay(Func<SProperty, UProperty> sourceToTarget)
        {
            return OneWay(new DelegateConvert<SProperty, UProperty>(sourceToTarget));
        }

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        public override void Unbind()
        {
            if (!isBinding)
            {
                return;
            }

            try
            {
                _binding.Unbind(this.HashCode);
                isBinding = false;
                NotifyDisposed();
            }
            catch (Exception)
            {
                // 重复解绑时保持幂等，避免对象销毁阶段抛异常。
            }
        }

        /// <summary>
        /// Binding 移除此绑定器时调用。
        /// </summary>
        public override void OnUnbind()
        {
            isBinding = false;
        }
    }
}

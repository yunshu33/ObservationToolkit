using System;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 绑定事件代理基类。
    /// 代理挂在目标 GameObject 上，负责缓存组件引用并提供统一 SetValue 入口。
    /// </summary>
    /// <typeparam name="T">目标 UGUI 组件类型。</typeparam>
    /// <typeparam name="TProperty">目标属性值类型。</typeparam>
    public abstract class UIBindingEventProxy<T, TProperty> : UIBehaviour where T : UIBehaviour
    {
        /// <summary>
        /// 缓存的目标组件。
        /// </summary>
        private T _target;

        /// <summary>
        /// 目标组件。
        /// </summary>
        public T Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetComponent<T>();
                    if (_target == null)
                    {
                        UnityEngine.Debug.LogError($"{gameObject.name} 上找不到组件 {typeof(T).Name}");
                    }
                }

                return _target;
            }
        }

        /// <summary>
        /// 代理销毁时触发的回调。
        /// </summary>
        public Action onDestroy;

        /// <summary>
        /// Unity 生命周期：初始化组件缓存。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _target = GetComponent<T>();
        }

        /// <summary>
        /// 设置目标组件的值。
        /// </summary>
        public abstract void SetValue(TProperty value);

        /// <summary>
        /// Unity 生命周期：通知代理销毁。
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            onDestroy?.Invoke();
        }
    }
}

using System;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定生命周期管理组件。
    /// 挂载到 GameObject 上后，会在 GameObject 销毁时自动解绑所有登记的绑定。
    /// </summary>
    public class BindingLifecycleBehavior : MonoBehaviour
    {
        /// <summary>
        /// 当前 GameObject 对应的绑定上下文。
        /// </summary>
        private BindingContext _bindingContext = new();

        /// <summary>
        /// 获取当前绑定上下文。
        /// </summary>
        public BindingContext Context => _bindingContext;

        /// <summary>
        /// 添加绑定到生命周期管理中。
        /// </summary>
        public void Add(IDisposable binding)
        {
            _bindingContext.Add(binding);
        }

        /// <summary>
        /// Unity 生命周期：对象销毁时释放所有绑定。
        /// </summary>
        private void OnDestroy()
        {
            if (_bindingContext != null)
            {
                _bindingContext.Dispose();
                _bindingContext = null;
            }
        }
    }
}

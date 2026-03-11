using System;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定生命周期管理器
    /// 挂载在 GameObject 上，当对象销毁时自动解绑
    /// </summary>
    public class BindingLifecycleBehavior : MonoBehaviour
    {
        private BindingContext _bindingContext = new();

        /// <summary>
        /// 获取或创建绑定上下文
        /// </summary>
        public BindingContext Context => _bindingContext;

        /// <summary>
        /// 添加绑定
        /// </summary>
        /// <param name="binding"></param>
        public void Add(IDisposable binding)
        {
            _bindingContext.Add(binding);
        }

        private void OnDestroy()
        {
            if (_bindingContext != null)
            {
                _bindingContext.Dispose();
                _bindingContext = null;
                Debug.Log($"[{gameObject.name}] BindingLifecycleBehavior: All bindings disposed.");
            }
        }
    }
}

using System;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定生命周期扩展方法
    /// </summary>
    public static class BindingLifecycleExtensions
    {
        /// <summary>
        /// 将绑定添加到 BindingContext 中
        /// </summary>
        public static void AddTo(this IDisposable binding, BindingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.Add(binding);
        }

        /// <summary>
        /// 将绑定绑定到 GameObject 的生命周期 (自动创建 BindingLifecycleBehavior)
        /// 当 GameObject 销毁时自动解绑
        /// </summary>
        public static void AddTo(this IDisposable binding, GameObject gameObject)
        {
            if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

            var behavior = gameObject.GetComponent<BindingLifecycleBehavior>();
            if (behavior == null)
            {
                behavior = gameObject.AddComponent<BindingLifecycleBehavior>();
            }
            
            behavior.Add(binding);
        }

        /// <summary>
        /// 将绑定绑定到 Component 的 GameObject 生命周期
        /// </summary>
        public static void AddTo(this IDisposable binding, Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            AddTo(binding, component.gameObject);
        }
    }
}

using System;
using UnityEngine;

namespace VoyageForge.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定生命周期扩展方法。
    /// </summary>
    public static class BindingLifecycleExtensions
    {
        /// <summary>
        /// 将绑定添加到指定 BindingContext。
        /// </summary>
        public static void AddTo(this IDisposable binding, BindingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.Add(binding);
        }

        /// <summary>
        /// 将绑定添加到 GameObject 生命周期。
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
        /// 将绑定添加到 Component 所在 GameObject 的生命周期。
        /// </summary>
        public static void AddTo(this IDisposable binding, Component component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            AddTo(binding, component.gameObject);
        }
    }
}

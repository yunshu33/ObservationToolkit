using System;
using System.Collections.Generic;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 绑定上下文
    /// 用于管理一组绑定的生命周期，支持批量解绑
    /// </summary>
    public class BindingContext : IDisposable
    {
        private readonly List<IDisposable> _bindings = new();

        /// <summary>
        /// 添加绑定到上下文
        /// </summary>
        /// <param name="binding">绑定对象</param>
        public void Add(IDisposable binding)
        {
            if (binding != null)
            {
                _bindings.Add(binding);

                if (binding is IDisposableBinding disposableBinding)
                {
                    disposableBinding.OnDisposed += () => _bindings.Remove(binding);
                }
            }
        }

        /// <summary>
        /// 解除所有绑定
        /// </summary>
        public void UnbindAll()
        {
            // 创建副本以避免在遍历时因回调移除元素而导致的集合已修改异常
            var bindings = _bindings.ToArray();
            
            // 提前清空列表，避免后续 Dispose 触发的回调进行无意义的 Remove 操作（O(N^2)）
            _bindings.Clear();

            foreach (var binding in bindings)
            {
                binding.Dispose();
            }
        }

        /// <summary>
        /// 释放资源（等同于 UnbindAll）
        /// </summary>
        public void Dispose()
        {
            UnbindAll();
        }
    }
}

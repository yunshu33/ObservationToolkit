using System;

namespace VoyageForge.ObservationToolkit.Runtime
{
    /// <summary>
    /// 可释放绑定接口。
    /// 所有绑定器实现该接口后，就可以交给 BindingContext 或 AddTo 自动管理生命周期。
    /// </summary>
    public interface IDisposableBinding : IDisposable
    {
        /// <summary>
        /// 绑定释放时触发。
        /// </summary>
        event Action OnDisposed;

        /// <summary>
        /// 手动解除绑定。
        /// </summary>
        void Unbind();
    }
}

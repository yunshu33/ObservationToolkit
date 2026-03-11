using System;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 可释放的绑定接口
    /// 提供销毁通知，以便容器进行管理
    /// </summary>
    public interface IDisposableBinding : IDisposable
    {
        /// <summary>
        /// 当绑定被销毁（Unbind/Dispose）时触发
        /// </summary>
        event Action OnDisposed;
    }
}

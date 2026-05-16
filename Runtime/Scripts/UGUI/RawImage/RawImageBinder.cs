using System;
using VoyageForge.ObservationToolkit.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// RawImage 绑定器，负责将模型值绑定到 RawImage.texture。
    /// </summary>
    public class RawImageBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, RawImage, Texture>
    {
        /// <summary>
        /// 创建 RawImage 绑定器。
        /// </summary>
        public RawImageBinder(RawImage target, Action<Texture> handler, Binding<S, SProperty> binding) : base(target,
            handler, binding)
        {
        }
    }

    /// <summary>
    /// RawImage 事件代理，统一封装 RawImage.texture 写入。
    /// </summary>
    public class RawImageBindingEventProxy : UIBindingEventProxy<RawImage, Texture>
    {
        /// <summary>
        /// 设置 RawImage.texture。
        /// </summary>
        public override void SetValue(Texture value)
        {
            Target.texture = value;
        }
    }
}

using System;
using VoyageForge.ObservationToolkit.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Image 绑定器，负责将模型值绑定到 Image.sprite。
    /// </summary>
    public class ImageBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Image, Sprite>
    {
        /// <summary>
        /// 创建 Image 绑定器。
        /// </summary>
        public ImageBinder(Image target, Action<Sprite> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    /// <summary>
    /// Image 事件代理，统一封装 Image.sprite 写入。
    /// </summary>
    public class ImageBindingEventProxy : UIBindingEventProxy<Image, Sprite>
    {
        /// <summary>
        /// 设置 Image.sprite。
        /// </summary>
        public override void SetValue(Sprite value)
        {
            Target.sprite = value;
        }
    }
}

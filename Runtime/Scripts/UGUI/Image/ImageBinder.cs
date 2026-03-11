using System;
using Voyage.ObservationToolkit.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Image 的 Sprite 绑定器 (单向)
    /// </summary>
    public class ImageBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, Image, Sprite>
    {
        public ImageBinder(Image target, Action<Sprite> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    public class ImageBindingEventProxy : UIBindingEventProxy<Image, Sprite>
    {
        public override void SetValue(Sprite value)
        {
            Target.sprite = value;
        }
    }
}

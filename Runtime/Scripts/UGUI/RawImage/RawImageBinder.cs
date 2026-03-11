using System;
using Voyage.ObservationToolkit.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// RawImage 的 Texture 绑定器 (单向)
    /// </summary>
    public class RawImageBinder<S, SProperty> : OneWayUGUIBinderBase<S, SProperty, RawImage, Texture>
    {
        public RawImageBinder(RawImage target, Action<Texture> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    public class RawImageBindingEventProxy : UIBindingEventProxy<RawImage, Texture>
    {
        public override void SetValue(Texture value)
        {
            Target.texture = value;
        }
    }
}

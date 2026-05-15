using System;
using Voyage.ObservationToolkit.Runtime;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Toggle 绑定器，Toggle 的 UI 值类型固定为 bool。
    /// </summary>
    public class ToggleBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Toggle, bool>
    {
        /// <summary>
        /// 创建 Toggle 绑定器。
        /// </summary>
        public ToggleBinder(Toggle target, Action<bool> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }
    }

    /// <summary>
    /// Toggle 事件代理，统一封装 Toggle.isOn 写入。
    /// </summary>
    public class ToggleBindingEventProxy : UIBindingEventProxy<Toggle, bool>
    {
        /// <summary>
        /// 设置 Toggle.isOn。
        /// </summary>
        public override void SetValue(bool value)
        {
            Target.isOn = value;
        }
    }
}

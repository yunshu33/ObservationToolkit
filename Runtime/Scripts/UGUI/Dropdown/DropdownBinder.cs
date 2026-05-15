using System;
using VoyageForge.ObservationToolkit.Runtime;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Dropdown 绑定器，Dropdown 的 UI 值类型固定为 int。
    /// </summary>
    public class DropdownBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Dropdown, int>
    {
        /// <summary>
        /// 创建 Dropdown 绑定器。
        /// </summary>
        public DropdownBinder(Dropdown target, Action<int> handler, Binding<S, SProperty> binding) : base(target,
            handler, binding)
        {
        }
    }

    /// <summary>
    /// Dropdown 事件代理，统一封装 Dropdown.value 写入。
    /// </summary>
    public class DropdownBindingEventProxy : UIBindingEventProxy<Dropdown, int>
    {
        /// <summary>
        /// 设置 Dropdown.value。
        /// </summary>
        public override void SetValue(int value)
        {
            Target.value = value;
        }
    }
}

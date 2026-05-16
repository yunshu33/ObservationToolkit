using System;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using UnityEngine.UI;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// Slider 绑定器，Slider 的 UI 值类型固定为 float。
    /// </summary>
    public class SliderBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, Slider, float>
    {
        /// <summary>
        /// 创建 Slider 绑定器。
        /// </summary>
        public SliderBinder(Slider target, Action<float> handler, Binding<S, SProperty> binding) : base(target, handler,
            binding)
        {
        }

        /// <summary>
        /// 将 Slider.float 值转换回模型属性值。
        /// 当模型为 int 时使用明确舍入规则，避免 float/int 双向绑定表现不稳定。
        /// </summary>
        protected override SProperty TargetConvertSource(float value)
        {
            if (_convert != null)
            {
                return _convert.TargetConvertSource(value);
            }

            if (typeof(SProperty) == typeof(int))
            {
                return (SProperty)(object)ConversionUtility.ToInt(value);
            }

            return base.TargetConvertSource(value);
        }
    }

    /// <summary>
    /// Slider 事件代理，统一封装 Slider.value 写入。
    /// </summary>
    public class SliderBindingEventProxy : UIBindingEventProxy<Slider, float>
    {
        /// <summary>
        /// 设置 Slider.value。
        /// </summary>
        public override void SetValue(float value)
        {
            Target.value = value;
        }
    }
}

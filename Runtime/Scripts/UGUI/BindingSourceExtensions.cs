using Voyage.ObservationToolkit.Runtime.Converter;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// BindingSource 到 UGUI 组件的链式绑定扩展。
    /// </summary>
    public static class BindingSourceExtensions
    {
        /// <summary>
        /// 绑定到 Text。
        /// </summary>
        public static TextUGUIBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Text target)
        {
            if (target.gameObject.TryGetComponent<TextBindingEventProxy>(out var proxy))
            {
                
            }
            else
            {
                proxy = target.gameObject.AddComponent<TextBindingEventProxy>();
            }

            return new TextUGUIBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Text，并预设接口转换器。
        /// </summary>
        public static TextUGUIBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            Text target,
            IConvert<SProperty, string> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }

        /// <summary>
        /// 绑定到 InputField。
        /// </summary>
        public static InputFieldUGUIBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            InputField target)
        {
            if (target.gameObject.TryGetComponent<InputFieldBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<InputFieldBindingEventProxy>();
            }

            return new InputFieldUGUIBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 InputField，并预设接口转换器。
        /// </summary>
        public static InputFieldUGUIBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            InputField target,
            IConvert<SProperty, string> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }
        
        /// <summary>
        /// 绑定到 Toggle。
        /// </summary>
        public static ToggleBinder<S,SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Toggle target)
        {
            if (target.gameObject.TryGetComponent<ToggleBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<ToggleBindingEventProxy>();
            }

            return new ToggleBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Toggle，并预设接口转换器。
        /// </summary>
        public static ToggleBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            Toggle target,
            IConvert<SProperty, bool> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }
        
        /// <summary>
        /// 绑定到 Slider。
        /// </summary>
        public static SliderBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Slider target)
        {
            if (target.gameObject.TryGetComponent<SliderBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<SliderBindingEventProxy>();
            }

            return new SliderBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Slider，并预设接口转换器。
        /// </summary>
        public static SliderBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            Slider target,
            IConvert<SProperty, float> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }

        /// <summary>
        /// 绑定到 Dropdown。
        /// </summary>
        public static DropdownBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Dropdown target)
        {
            if (target.gameObject.TryGetComponent<DropdownBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<DropdownBindingEventProxy>();
            }

            return new DropdownBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Dropdown，并预设接口转换器。
        /// </summary>
        public static DropdownBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            Dropdown target,
            IConvert<SProperty, int> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }

        /// <summary>
        /// 绑定到 Image。
        /// </summary>
        public static ImageBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            Image target)
        {
            if (target.gameObject.TryGetComponent<ImageBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<ImageBindingEventProxy>();
            }

            return new ImageBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Image，并预设接口转换器。
        /// </summary>
        public static ImageBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            Image target,
            IConvert<SProperty, UnityEngine.Sprite> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }

        /// <summary>
        /// 绑定到 RawImage。
        /// </summary>
        public static RawImageBinder<S, SProperty> To<S, SProperty>(this BindingSource<S, SProperty> binder,
            RawImage target)
        {
            if (target.gameObject.TryGetComponent<RawImageBindingEventProxy>(out var proxy))
            {
            }
            else
            {
                proxy = target.gameObject.AddComponent<RawImageBindingEventProxy>();
            }

            return new RawImageBinder<S, SProperty>(target, proxy.SetValue, binder.Binding);
        }

        /// <summary>
        /// 绑定到 RawImage，并预设接口转换器。
        /// </summary>
        public static RawImageBinder<S, SProperty> To<S, SProperty>(
            this BindingSource<S, SProperty> binder,
            RawImage target,
            IConvert<SProperty, UnityEngine.Texture> converter)
        {
            var result = binder.To(target);
            result.Converter = converter;
            return result;
        }

        /// <summary>
        /// 绑定到 Button 的 ICommand。
        /// </summary>
        public static ButtonCommandBinder<S> To<S>(this BindingSource<S, ICommand> binder, Button target)
        {
            return new ButtonCommandBinder<S>(target, binder.Binding);
        }

        /// <summary>
        /// 绑定到 Button 的 ICommand，并为命令提供固定参数。
        /// 参数会在 Button 点击时传给 ICommand.CanExecute 和 ICommand.Execute。
        /// </summary>
        public static ButtonCommandBinder<S> To<S>(
            this BindingSource<S, ICommand> binder,
            Button target,
            object parameter)
        {
            return new ButtonCommandBinder<S>(target, binder.Binding, parameter);
        }
    }
}

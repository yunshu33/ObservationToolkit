using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Sample.UGUI
{
    public class UGUITest : MonoBehaviour
    {
        [Header("ViewModel")]
        public TestViewModel model;

        [Header("UI Components")]
        public Text text;
        public Button button;
        public InputField inputField;
        public Toggle toggle;
        public Slider slider;
        public Dropdown dropdown;
        public Image image;
        public RawImage rawImage;

        private void Start()
        {
            BindAll();
        }

        private void BindAll()
        {
            // 1. Text 单向绑定 (float -> string)
            // 自动使用 InvariantCulture 转换
            if (text != null)
            {
                model.For(m => m.Value)
                    .To(text)
                    .OneWay()
                    .AddTo(this); 
            }

            // 2. InputField 双向绑定 (float <-> string)
            // 自动处理 CultureInfo 和 防重入
            if (inputField != null)
            {
                model.For(m => m.Value)
                    .To(inputField)
                    .TwoWay(input => input.onValueChanged)
                    .AddTo(this);
            }

            // 3. Toggle 双向绑定 (bool <-> bool)
            if (toggle != null)
            {
                model.For(m => m.IsOn)
                    .To(toggle)
                    .TwoWay(t => t.onValueChanged)
                    .AddTo(this);

                    
            }
            
            // 4. Slider 双向绑定 (float <-> float)
            // 自动处理防重入，防止死循环
            if (slider != null)
            {
                model.For(m => m.Value)
                    .To(slider)
                    .TwoWay(s => s.onValueChanged)
                    .AddTo(this);
            }

            // 5. Dropdown 双向绑定 (int <-> int)
            if (dropdown != null)
            {
                model.For(m => m.Index)
                    .To(dropdown)
                    .TwoWay(d => d.onValueChanged)
                    .AddTo(this);
            }
            
            // 6. Image 单向绑定 (Sprite -> Sprite)
            if (image != null)
            {
                model.For(m => m.Icon)
                    .To(image)
                    .OneWay()
                    .AddTo(this);
            }
            
            // 7. RawImage 单向绑定 (Texture -> Texture)
            if (rawImage != null)
            {
                model.For(m => m.Texture)
                    .To(rawImage)
                    .OneWay()
                    .AddTo(this);
            }

            // 8. Button Command 绑定
            // 自动随 Button 销毁而解绑
            if (button != null)
            {
                button.BindCommand(model, m => m.AddCommand);
            }
        }
        
        // 用于测试手动修改 Model，验证 UI 是否同步
        [ContextMenu("Modify Model Value")]
        public void ModifyModelValue()
        {
            model.Value += 10;
            model.IsOn = !model.IsOn;
            if (model.Index < 2) model.Index++; else model.Index = 0;
            
            Debug.Log($"Model Modified: Value={model.Value}, IsOn={model.IsOn}, Index={model.Index}");
        }
    }
}

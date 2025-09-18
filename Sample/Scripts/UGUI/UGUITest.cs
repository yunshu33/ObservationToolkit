using System;
using LJVoyage.ObservationToolkit.Runtime;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using LJVoyage.ObservationToolkit.Runtime.UGUI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Sample.UGUI
{
    public class UGUITest : MonoBehaviour
    {
        public TestModel2 model;

        public Text text;

        public Button button;

        public InputField inputField;

        public Toggle toggle;
        
        public Slider slider;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            model.Value++;

            model.IsOn = !model.IsOn;
        }

        [ContextMenu("AddListener")]
        private void AddListener()
        {
           
            
            model.For(m => m.Value).To(text).OneWay();

            model.For(m => m.Value).To(inputField).TwoWay(input => input.onValueChanged);

            model.For(m => m.IsOn).To(toggle).TwoWay(toggle => toggle.onValueChanged);
            
            model.For(m => m.Value).To(slider).TwoWay(slider => slider.onValueChanged);
        }

        public class FloatToStringConverter : IConvert<float, string>
        {
            public float ObjectConvertSource(object source)
            {
                return (float)(source);
            }

            public string SourceConvertTarget(float source)
            {
                return source.ToString("F2");
            }

            public float TargetConvertSource(string target)
            {
                throw new NotImplementedException();
            }
        }


        [ContextMenu("RemoveListener")]
        private void RemoveListener()
        {
            model.For(m => m.Value).To(text).Unbind();
            model.For(m => m.Value).To(inputField).Unbind(input => input.onValueChanged);
            model.For(m => m.IsOn).To(toggle).Unbind(toggle => toggle.onValueChanged);
            model.For(m => m.Value).To(slider).Unbind(slider => slider.onValueChanged);
        }
    }
}
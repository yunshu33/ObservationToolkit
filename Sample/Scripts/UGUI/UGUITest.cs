using System;
using LJVoyage.ObservationToolkit.Runtime;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using LJVoyage.ObservationToolkit.Runtime.UGUI;
using UnityEngine;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Sample.UGUI
{
    public class UGUITest : MonoBehaviour
    {
        public TestModel2 model;

        public Text text;

        public Button button;

        public InputField inputField;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            model.Value++;
        }
        
        [ContextMenu("AddListener")]
        private void AddListener()
        {
            model.For(m => m.Value).To(text).OneWay(new FloatToStringConverter());
            
            model.For(m=>m.Value).To(inputField).TwoWay(input=> input.onSubmit);
        }

        public class FloatToStringConverter : IConvert<float, string>
        {
            public float Convert(object source)
            {
                return (float)(source);
            }

            public string Convert(float source)
            {
                return source.ToString("F2");
            }
        }
        
        
        [ContextMenu("RemoveListener")]
        private void RemoveListener()
        {
            model.For(m => m.Value).To(text).Unbind();
        }
    }
}
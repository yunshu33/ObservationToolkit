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
            model.For(m => m.Value).To(text).OneWay();
            
            model.For(m=>m.Value).To(inputField).TwoWay(input=> input.onValueChanged);
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
            model.For(m=>m.Value).To(inputField).Unbind();
        }
    }
}
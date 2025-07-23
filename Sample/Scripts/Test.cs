using System;
using UnityEngine;
using UnityEngine.UI;
using LJVoyage.ObservationToolkit.Runtime;
using LJVoyage.ObservationToolkit.Runtime.UGUI;

namespace LJVoyage.ObservationToolkit.Sample
{
    public class Test : MonoBehaviour
    {
        public TestModel model;

        public Text text;

        public Button button;

        public InputField inputField;


        private void Awake()
        {
            // 转换代理 问题

            //OneWay 和Two way 要拆分为接口  部分 gui 不支持双向  比如 Text 

            //  way 可传入  转换代理 

            model.For(m => m.Value).To<int>(TestEvent).OneWay();

            model.For(m => m.Value).To(text).OneWay();

            model.For(m => m.Value).To(inputField).OneWay();

            //model.For(m=>m.Value).Bind();

            button.onClick.AddListener(Add);
        }


        private void TestEvent(int value)
        {
            text.text = value.ToString();
        }


        [ContextMenu("AddListener")]
        private void AddListener()
        {
        }

        [ContextMenu("RemoveListener")]
        private void RemoveListener()
        {
       
            model.For(m => m.Value).To(text).Unbind();
        }

        [ContextMenu("Add")]
        private void Add()
        {
            model.Value++;
        }

        [ContextMenu("Subtract")]
        private void Subtract()
        {
            model.Value--;
        }
    }

    
    

    [Serializable]
    public class TestModel : ITTT
    {
        public BindingHandler BindingHandler { get; set; }

        public int Value
        {
            get => _value;
            set { this.SetField(ref _value, value); }
        }

        [SerializeField] private int _value;

        public int count;
        [SerializeField] private bool _ttt;

        public bool TTT
        {
            get => _ttt;
            set { this.SetField(ref _ttt, value); }
        }
    }
    

    public interface ITTT : IBindingHolder
    {
        public bool TTT { get; set; }
    }
}
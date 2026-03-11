using System;
using UnityEngine;
using UnityEngine.UI;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;

using Voyage.ObservationToolkit.Runtime.ViewModel;

namespace Voyage.ObservationToolkit.Sample
{
    public class Test : MonoBehaviour
    {
        public TestModel model;

        public Text text;

        public Button button;

        public InputField inputField;


        private void Awake()
        {
            // 绑定并自动管理生命周期（随当前 GameObject 销毁而解绑）
            model.For(m => m.Value)
                .To(TestEvent)
                .OneWay()
                .AddTo(this); 

            button.onClick.AddListener(Add);
        }


        private void TestEvent2(TestModel model, int value)
        {
            Debug.Log(value);
        }


        private void TestEvent(int value)
        {
            text.text = value.ToString();
        }


        [ContextMenu("AddListener")]
        private void AddListener()
        {
            model.For(m => m.Value).To(TestEvent2).OneWay();
            // model.For(m => m.Value, new Convert1()).To(TestEvent2).OneWay();
        }

        [ContextMenu("RemoveListener")]
        private void RemoveListener()
        {
            // model.For(m => m.Value).To(TestEvent2).Unbind();
            model.For(m => m.Value).To(TestEvent).Unbind();
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

    [Serializable]
    public class TestData
    {
        public float value;
        public bool isOn;
        public int index;
        public Sprite icon;
        public Texture texture;
    }
    
    [Serializable]
    public class TestViewModel : ViewModel<TestData>
    {

        public float Value
        {
            get => Data.value;
            set
            {
                if (this.SetField(ref Data.value, value))
                {
                    _addCommand?.RaiseCanExecuteChanged();
                }
            }
        }


        [Observation]
        public bool IsOn
        {
           get;set;
        }

        public int Index
        {
            get => Data.index;
            set => this.SetField(ref Data.index, value);
        }

        public Sprite Icon
        {
            get => Data.icon;
            set => this.SetField(ref Data.icon, value);
        }

        public Texture Texture
        {
            get => Data.texture;
            set => this.SetField(ref Data.texture, value);
        }

        private ICommand _addCommand;
        public ICommand AddCommand => _addCommand ??= new RelayCommand(() => Value++, () => Value < 100);
    }


    public interface ITTT : IObservable
    {
        public bool TTT { get; set; }
    }
}
using System;
using System.Linq.Expressions;
using System.Reflection;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class InputFieldUGUIBinder<S, SProperty> : TwoWayUGUIBinderBase<S, SProperty, InputField, string>
    {
        public InputFieldUGUIBinder(InputField target, Action<string> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
            
        }

        
        
        public override void Invoke(S source, SProperty property)
        {
            if (_converter != null)
            {
                Handler?.Invoke(_converter.SourceConvertTarget(property));
            }
            else
            {
                Handler?.Invoke(property.ToString());
            }
        }

        public override void Unbind()
        {
            throw new NotImplementedException();
        }


        public override void Unbind(Expression<Func<InputField, UnityEvent<string>>> propertyExpression)
        {
            _binding.Unbind(this);
        }
        
        public override void OnUnbind()
        {
            Debug.Log($"UGUI 事件移除前:{_uiEvent.GetPersistentEventCount()}");
            _uiEvent.RemoveListener(_uiAction);
            Debug.Log($"UGUI 事件移除后:{_uiEvent.GetPersistentEventCount()}");
        }

        protected override SProperty TargetConvertSource(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return base.TargetConvertSource(value);
        }

        public override void OneWay(IConvert<SProperty, string> convert)
        {
            _converter = convert;
            OneWay();
        }


        public override void TwoWay(Expression<Func<InputField, UnityEvent<string>>> propertyExpression)
        {
            // 编译表达式并获取委托
            Func<InputField, UnityEvent<string>> propertyAccessor = propertyExpression.Compile();

            // 调用委托获取 UnityEvent<string>
            _uiEvent = propertyAccessor(_target);

            _uiAction = CreateSetter();

            _uiEvent.AddListener(_uiAction);

            OneWay();

            // var eventField = propertyExpression.Body as MemberExpression;
            // var eventFieldName = eventField.Member.Name;
            // var eventFieldType = eventField.Member.DeclaringType;
            // var eventFieldAdd = eventFieldType.GetMethod("AddListener");
            // var eventFieldRemove = eventFieldType.GetMethod("RemoveListener");
            // var eventFieldRaise = eventFieldType.GetMethod("RaiseEvent");
            // var eventFieldAddAction = eventFieldAdd.MakeGenericMethod(typeof(string));
            // var eventFieldRemoveAction = eventFieldRemove.MakeGenericMethod(typeof(string));
            // var eventFieldRaiseAction = eventFieldRaise.MakeGenericMethod(typeof(string));
        }

        public override void TwoWay(Expression<Func<InputField, UnityEvent<string>>> propertyExpression,
            IConvert<SProperty, string> convert)
        {
            _converter = convert;
            TwoWay(propertyExpression);
        }
    }


    public class InputFieldBindingEventProxy : UIBindingEventProxy<InputField, string>
    {
        public override void SetValue(string value)
        {
            Target.text = value;
        }
    }
}
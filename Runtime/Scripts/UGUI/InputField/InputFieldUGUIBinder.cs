
using System;
using System.Linq.Expressions;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public class InputFieldUGUIBinder<T, TProperty> : TwoWayUGUIBinderBase<T, TProperty, InputField, string>
    {
        public InputFieldUGUIBinder(BindingSource<T, TProperty> bindingSource, InputField target, Binding<T, TProperty> binding) : base(bindingSource, target, binding)
        {
        }

        protected override TProperty Convert(object value)
        {
            throw new System.NotImplementedException();
        }

        public override void Invoke(T source, object obj, TProperty property)
        {
            
        }

        public override void Unbind()
        {
            _binding.Unbind(this);
        }

        public override void OneWay(IConvert<TProperty, string> convert)
        {
            
        }

        public override void TwoWay(Expression<Func<InputField, UnityEvent<string>>> propertyExpression)
        {
            var eventField = propertyExpression.Body as MemberExpression;
            var eventFieldName = eventField.Member.Name;
            var eventFieldType = eventField.Member.DeclaringType;
            var eventFieldAdd = eventFieldType.GetMethod("AddListener");
            var eventFieldRemove = eventFieldType.GetMethod("RemoveListener");
            var eventFieldRaise = eventFieldType.GetMethod("RaiseEvent");
            var eventFieldAddAction = eventFieldAdd.MakeGenericMethod(typeof(string));
            var eventFieldRemoveAction = eventFieldRemove.MakeGenericMethod(typeof(string));
            var eventFieldRaiseAction = eventFieldRaise.MakeGenericMethod(typeof(string));
            
        }

        public override void TwoWay(IConvert<TProperty, string> convert)
        {
            throw new System.NotImplementedException();
        }
    }
}
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
        private UnityAction<string> _uiAction;

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
            _binding.Unbind(this);
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
            UnityEvent<string> unityEvent = propertyAccessor(_target);

            _uiAction = CreateSetter();

            unityEvent.AddListener(_uiAction);

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


        public Action<string> CreateBoundSetter(object source, string propertyName)
        {
            var targetType = source.GetType();
            var property = targetType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || !property.CanWrite)
                throw new ArgumentException($"属性 {propertyName} 不存在或不可写");

            // 参数：object value
            var valueParam = Expression.Parameter(typeof(object), "value");

            // (TProperty)value
            var valueCast = Expression.Convert(valueParam, property.PropertyType);

            // 固定 source
            var instance = Expression.Constant(source);

            // source.Property = (TProperty)value
            var body = Expression.Assign(Expression.Property(instance, property), valueCast);

            var lambda = Expression.Lambda<Action<string>>(body, valueParam);

            return lambda.Compile();
        }


        public override void TwoWay(IConvert<SProperty, string> convert)
        {
            throw new System.NotImplementedException();
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
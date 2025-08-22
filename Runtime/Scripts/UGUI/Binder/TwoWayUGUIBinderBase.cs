using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class TwoWayUGUIBinderBase<S, SProperty, U, UProperty> :
        OneWayUGUIBinderBase<S, SProperty, U, UProperty>,
        ITwoWayBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        protected UnityAction<UProperty> _uiAction;

        protected UnityEvent<UProperty> _uiEvent;

        protected TypeConverter _sourcePropertyConverter;

        public override string HashCode => _hash + _uiEventName;

        protected string _uiEventName;

        protected TwoWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }


        public void Unbind(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExp)
            {
                _binding.Unbind(_hash + "+" + memberExp.Member.Name);
            }
        }

        public override void Unbind()
        {
            _binding.Unbind(HashCode);
        }

        public override void OnUnbind()
        {
            if (_uiAction != null)
            {
                Debug.Log($"UGUI 事件移除前:{GetRuntimeListenerCount(_uiEvent)}");
                _uiEvent.RemoveListener(_uiAction);
                Debug.Log($"UGUI 事件移除后:{GetRuntimeListenerCount(_uiEvent)}");
            }
        }


        public static int GetRuntimeListenerCount(UnityEventBase unityEvent)
        {
            var field = typeof(UnityEventBase).GetField("m_Calls",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var invokeCallList = field.GetValue(unityEvent);

            var callsField = invokeCallList.GetType().GetField("m_RuntimeCalls",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var runtimeCalls = callsField.GetValue(invokeCallList) as System.Collections.ICollection;

            return runtimeCalls?.Count ?? 0;
        }


        protected UnityAction<UProperty> CreateSetter()
        {
            var source = _binding.Source;
            var propertyName = _binding.PropertyName;

            var targetType = source.GetType();
            var property = targetType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || !property.CanWrite)
                throw new ArgumentException($"属性 {propertyName} 不存在或不可写");

            // 参数：string value （注意这里要用 string，而不是 object）
            var valueParam = Expression.Parameter(typeof(UProperty), "value");

            // 固定 source
            var instance = Expression.Constant(source);

            // 调用 this.TargetConvertSource(value)，返回 SProperty
            var convertMethod = GetType().GetMethod("TargetConvertSource",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var convertedValue = Expression.Call(Expression.Constant(this), convertMethod, valueParam);

            // source.Property = TargetConvertSource(value)
            var body = Expression.Assign(Expression.Property(instance, property), convertedValue);

            var lambda = Expression.Lambda<UnityAction<UProperty>>(body, valueParam);

            return lambda.Compile();
        }


        /// <summary>
        /// 目标转换源
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual SProperty TargetConvertSource(UProperty value)
        {
            if (_convert != null)
            {
                return _convert.TargetConvertSource(value);
            }

            if (value == null)
                return default;

            if (_isTypeEqual)
            {
                return (SProperty)(object)value;
            }

            _sourcePropertyConverter ??= TypeDescriptor.GetConverter(typeof(SProperty));


            if (_sourcePropertyConverter.CanConvertFrom(typeof(SProperty)))
            {
                try
                {
                    return (SProperty)_sourcePropertyConverter.ConvertFrom(value);
                }
                catch
                {
                    // 忽略，尝试下一步
                }
            }

            try
            {
                return (SProperty)System.Convert.ChangeType(value, typeof(SProperty));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"无法将类型 {typeof(SProperty)} 转换为 {typeof(SProperty)}", ex);
            }
        }

        public void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExp)
            {
                _uiEventName = "+" + memberExp.Member.Name;
            }
            else
            {
                throw new Exception($"UGUI 事件绑定失败，事件名：{_uiEventName}");
            }

            // 编译表达式并获取委托
            Func<U, UnityEvent<UProperty>> propertyAccessor = propertyExpression.Compile();

            // 调用委托获取 UnityEvent<string>
            _uiEvent = propertyAccessor(_target);

            _uiAction = CreateSetter();

            _uiEvent.AddListener(_uiAction);

            OneWay();
        }


        public void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            IConvert<SProperty, UProperty> convert)
        {
            _convert = convert;
            TwoWay(propertyExpression);
        }
    }
}
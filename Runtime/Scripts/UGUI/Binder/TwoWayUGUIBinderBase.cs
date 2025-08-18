
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class TwoWayUGUIBinderBase<S, SProperty, U, UProperty> : OneWayUGUIBinderBase<S, SProperty, U, UProperty>,
        ITwoWayBinder<S, SProperty, U, UProperty> where U : UIBehaviour 
    {
        protected TwoWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(target, handler, binding)
        {
            
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

        
        protected TypeConverter _sourcePropertyConverter;
        
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
        
        public abstract void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);

        public abstract void TwoWay(IConvert<SProperty, UProperty> convert);
    }
}
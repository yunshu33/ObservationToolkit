using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class TwoWayUGUIBinderBase<S, SProperty, U, UProperty> :
        OneWayUGUIBinderBase<S, SProperty, U, UProperty>,
        ITwoWayBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        protected UnityAction<UProperty> _uiAction;

        protected UnityEvent<UProperty> _uiEvent;

        protected TypeConverter _sourcePropertyConverter;

        public override int HashCode => _hash ^ _uiEventHash;

        protected int _uiEventHash;
        
        /// <summary>
        /// 标记是否正在从 Model 更新 UI，用于防止死循环 (Model -> UI -> Event -> Model)
        /// </summary>
        protected bool _isUpdatingUI;

        protected TwoWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }
        
        public override void Invoke(S source, SProperty property)
        {
            try
            {
                _isUpdatingUI = true;
                base.Invoke(source, property);
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }


        public void Unbind(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExp)
            {
                int targetHash = _hash ^ memberExp.Member.Name.GetHashCode();
                if (targetHash == HashCode)
                {
                    // 如果匹配当前绑定，走完整的解绑流程
                    Unbind();
                }
                else
                {
                    // 否则只解除底层绑定（可能是部分解绑？）
                    _binding.Unbind(targetHash);
                }
            }
        }

        // 移除 override Unbind()，使用基类 OneWayUGUIBinderBase 的实现

        public override void OnUnbind()
        {
            // 确保先解绑 UI 事件
            if (_uiEvent != null && _uiAction != null)
            {
                // Debug.Log($"UGUI 事件移除前:{GetRuntimeListenerCount(_uiEvent)}");
                _uiEvent.RemoveListener(_uiAction);
                // Debug.Log($"UGUI 事件移除后:{GetRuntimeListenerCount(_uiEvent)}");
                _uiEvent = null;
                _uiAction = null;
            }
            
            base.OnUnbind();
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
            Expression convertedValue;
            if (_isTypeEqual && _convert == null)
            {
                convertedValue = Expression.Convert(valueParam, typeof(SProperty));
            }
            else
            {
                var convertMethod = GetType().GetMethod("TargetConvertSource",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                convertedValue = Expression.Call(Expression.Constant(this), convertMethod, valueParam);
            }

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

            if (_isTypeEqual)
            {
                return (SProperty)(object)value;
            }

            if (value == null)
            {
                 // 处理值类型空值
                 if (default(SProperty) != null)
                 {
                     return default;
                 }
                return default;
            }

            // 优先使用 Convert.ChangeType，因为它更符合直觉且稳定
            try
            {
                return (SProperty)System.Convert.ChangeType(value, typeof(SProperty), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                // 如果 ChangeType 失败，尝试 TypeConverter 作为备选
            }

            _sourcePropertyConverter ??= TypeDescriptor.GetConverter(typeof(SProperty));

            if (_sourcePropertyConverter.CanConvertFrom(typeof(UProperty)))
            {
                try
                {
                    return (SProperty)_sourcePropertyConverter.ConvertFrom(null, System.Globalization.CultureInfo.InvariantCulture, value);
                }
                catch
                {
                    // ignore
                }
            }

            throw new InvalidCastException(
                $"无法将类型 {typeof(UProperty)} 转换为 {typeof(SProperty)}. Value: {value}");
        }

        public IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExp)
            {
                _uiEventHash = memberExp.Member.Name.GetHashCode();
            }
            else
            {
                throw new Exception($"UGUI 事件绑定失败，无法获取事件名");
            }

            // 编译表达式并获取委托
            Func<U, UnityEvent<UProperty>> propertyAccessor = propertyExpression.Compile();

            // 调用委托获取 UnityEvent<string>
            _uiEvent = propertyAccessor(_target);

            var rawSetter = CreateSetter();
            _uiAction = (value) =>
            {
                if (_isUpdatingUI) return;
                rawSetter(value);
            };

            _uiEvent.AddListener(_uiAction);

            return OneWay();
        }


        public IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            IConvert<SProperty, UProperty> convert)
        {
            _convert = convert;
            return TwoWay(propertyExpression);
        }
    }
}
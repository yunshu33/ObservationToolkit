using System;
using System.Linq.Expressions;
using System.Reflection;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    /// <summary>
    /// UGUI 双向绑定基类，负责 UI 事件监听、UI 值回写模型和循环更新保护。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    /// <typeparam name="U">UGUI 组件类型。</typeparam>
    /// <typeparam name="UProperty">UGUI 事件值类型。</typeparam>
    public abstract class TwoWayUGUIBinderBase<S, SProperty, U, UProperty> :
        OneWayUGUIBinderBase<S, SProperty, U, UProperty>,
        ITwoWayBinder<S, SProperty, U, UProperty> where U : UIBehaviour
    {
        /// <summary>
        /// 注册到 UI 事件上的回写回调。
        /// </summary>
        protected UnityAction<UProperty> _uiAction;

        /// <summary>
        /// 当前绑定的 UI 事件，例如 Slider.onValueChanged。
        /// </summary>
        protected UnityEvent<UProperty> _uiEvent;

        /// <summary>
        /// UI 事件名对应的哈希，和目标组件哈希组合后作为双向绑定唯一键。
        /// </summary>
        protected int _uiEventHash;

        /// <summary>
        /// 当前绑定的最终哈希。双向绑定同一个组件上的不同事件时，需要区分事件。
        /// </summary>
        public override int HashCode => _hash ^ _uiEventHash;

        /// <summary>
        /// 标记当前是否正在由 Model 更新 UI，避免 UI 赋值再次触发事件回写造成循环。
        /// </summary>
        protected bool _isUpdatingUI;

        /// <summary>
        /// 创建 UGUI 双向绑定基类。
        /// </summary>
        protected TwoWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<S, SProperty> binding) : base(
            target, handler, binding)
        {
        }

        /// <summary>
        /// Model -> UI 更新入口。更新 UI 期间会开启循环保护。
        /// </summary>
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

        /// <summary>
        /// 按指定 UI 事件解除双向绑定。
        /// </summary>
        public void Unbind(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExp)
            {
                return;
            }

            int targetHash = _hash ^ memberExp.Member.Name.GetHashCode();
            if (targetHash == HashCode)
            {
                Unbind();
                return;
            }

            _binding.Unbind(targetHash);
        }

        /// <summary>
        /// 从 Binding 中移除时，确保 UI 事件监听也被移除。
        /// </summary>
        public override void OnUnbind()
        {
            if (_uiEvent != null && _uiAction != null)
            {
                _uiEvent.RemoveListener(_uiAction);
                _uiEvent = null;
                _uiAction = null;
            }

            base.OnUnbind();
        }

        /// <summary>
        /// 调试辅助：读取 UnityEvent 的运行时监听数量。
        /// 这个方法使用反射，避免在热路径调用。
        /// </summary>
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

        /// <summary>
        /// 创建 UI 值回写模型属性的委托。
        /// 这里在绑定建立时只编译一次，避免每次 UI 事件触发时使用反射 SetValue。
        /// </summary>
        protected UnityAction<UProperty> CreateSetter()
        {
            var source = _binding.Source;
            var propertyName = _binding.PropertyName;
            var targetType = source.GetType();
            var property = targetType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || !property.CanWrite)
            {
                throw new ArgumentException($"属性 {propertyName} 不存在或不可写。");
            }

            var valueParam = Expression.Parameter(typeof(UProperty), "value");
            var instance = Expression.Constant(source);

            Expression convertedValue;
            if (_isTypeEqual && _convert == null)
            {
                convertedValue = Expression.Convert(valueParam, typeof(SProperty));
            }
            else
            {
                var convertMethod = typeof(TwoWayUGUIBinderBase<S, SProperty, U, UProperty>).GetMethod(
                    nameof(ConvertValueForSetter),
                    BindingFlags.Instance | BindingFlags.NonPublic);
                convertedValue = Expression.Call(Expression.Constant(this), convertMethod, valueParam);
            }

            var body = Expression.Assign(Expression.Property(instance, property), convertedValue);
            var lambda = Expression.Lambda<UnityAction<UProperty>>(body, valueParam);
            return lambda.Compile();
        }

        /// <summary>
        /// 给表达式树调用的转换桥接方法。
        /// 单独留一个非虚私有方法，是为了稳定 MethodInfo 查找，再由这里分派到可覆写转换逻辑。
        /// </summary>
        private SProperty ConvertValueForSetter(UProperty value)
        {
            return TargetConvertSource(value);
        }

        /// <summary>
        /// 将 UI 值转换为模型属性值。
        /// 派生类可以覆盖此方法处理特殊组件行为，例如 Slider 的 float -> int。
        /// </summary>
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

            try
            {
                return ConversionUtility.Convert<SProperty>(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(
                    $"无法将类型 {typeof(UProperty)} 转换为 {typeof(SProperty)}。Value: {value}", ex);
            }
        }

        /// <summary>
        /// 建立双向绑定，使用默认类型转换。
        /// </summary>
        public IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExp)
            {
                throw new Exception("UGUI 事件绑定失败，表达式必须直接指向 UnityEvent 属性或字段。");
            }

            _uiEventHash = memberExp.Member.Name.GetHashCode();
            _uiEvent = GetUnityEvent(_target, memberExp.Member);

            var rawSetter = CreateSetter();
            _uiAction = value =>
            {
                if (_isUpdatingUI) return;
                rawSetter(value);
            };

            _uiEvent.AddListener(_uiAction);
            return OneWay();
        }

        /// <summary>
        /// 建立双向绑定，使用自定义转换器。
        /// </summary>
        public IDisposableBinding TwoWay(
            Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            IConvert<SProperty, UProperty> convert)
        {
            _convert = convert;
            return TwoWay(propertyExpression);
        }

        /// <summary>
        /// 建立双向绑定，使用一对转换函数。
        /// </summary>
        public IDisposableBinding TwoWay(
            Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,
            Func<SProperty, UProperty> sourceToTarget,
            Func<UProperty, SProperty> targetToSource)
        {
            return TwoWay(propertyExpression, new DelegateConvert<SProperty, UProperty>(sourceToTarget, targetToSource));
        }

        /// <summary>
        /// 根据表达式解析出的成员直接读取 UnityEvent，避免编译表达式带来的额外成本。
        /// </summary>
        private static UnityEvent<UProperty> GetUnityEvent(U target, MemberInfo member)
        {
            return member switch
            {
                PropertyInfo property => (UnityEvent<UProperty>)property.GetValue(target),
                FieldInfo field => (UnityEvent<UProperty>)field.GetValue(target),
                _ => throw new ArgumentException($"成员 {member.Name} 不是属性或字段。")
            };
        }
    }
}

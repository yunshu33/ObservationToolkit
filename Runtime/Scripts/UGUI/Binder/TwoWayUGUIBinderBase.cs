using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using VoyageForge.ObservationToolkit.Runtime;
using VoyageForge.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VoyageForge.ObservationToolkit.Runtime.UGUI
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
        /// Setter 委托缓存。
        /// key 由源对象真实类型、属性名和源属性类型组成，避免同类同属性反复编译表达式。
        /// </summary>
        private static readonly Dictionary<string, Action<object, SProperty>> SetterCache = new();

        /// <summary>
        /// Setter 缓存锁。
        /// </summary>
        private static readonly object SetterCacheLock = new();

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
        /// 绑定实例只负责转换 UI 值；真正的属性 setter 使用全局缓存复用。
        /// </summary>
        protected UnityAction<UProperty> CreateSetter()
        {
            var source = _binding.Source;
            var propertyName = _binding.PropertyName;
            var sourceType = source.GetType();
            var property = sourceType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || !property.CanWrite)
            {
                throw new ArgumentException($"属性 {propertyName} 不存在或不可写。");
            }

            var setter = GetOrCreateSetter(sourceType, property);
            return value => setter(source, TargetConvertSource(value));
        }

        /// <summary>
        /// 获取或创建源属性 setter。
        /// </summary>
        private static Action<object, SProperty> GetOrCreateSetter(Type sourceType, PropertyInfo property)
        {
            var key = $"{sourceType.AssemblyQualifiedName}|{property.Name}|{typeof(SProperty).AssemblyQualifiedName}";
            lock (SetterCacheLock)
            {
                if (SetterCache.TryGetValue(key, out var setter))
                {
                    return setter;
                }
            }

            var compiledSetter = BuildSetter(sourceType, property);
            lock (SetterCacheLock)
            {
                SetterCache[key] = compiledSetter;
            }

            return compiledSetter;
        }

        /// <summary>
        /// 编译 open setter：参数为 object 源对象和强类型属性值。
        /// 这样同一个 ViewModel 类型的同一属性可以复用同一个 setter 委托。
        /// </summary>
        private static Action<object, SProperty> BuildSetter(Type sourceType, PropertyInfo property)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var valueParam = Expression.Parameter(typeof(SProperty), "value");
            var typedSource = Expression.Convert(sourceParam, sourceType);

            Expression typedValue = property.PropertyType == typeof(SProperty)
                ? valueParam
                : Expression.Convert(valueParam, property.PropertyType);

            var body = Expression.Assign(Expression.Property(typedSource, property), typedValue);
            return Expression.Lambda<Action<object, SProperty>>(body, sourceParam, valueParam).Compile();
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

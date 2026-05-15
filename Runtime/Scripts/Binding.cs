using System;
using System.Collections.Generic;
using System.Reflection;
using VoyageForge.ObservationToolkit.Runtime.Converter;

namespace VoyageForge.ObservationToolkit.Runtime
{
    /// <summary>
    /// 某一个源属性对应的绑定集合。
    /// </summary>
    /// <typeparam name="S">源对象类型。</typeparam>
    /// <typeparam name="SProperty">源属性类型。</typeparam>
    public class Binding<S, SProperty> : Binding, IBinding<SProperty>
    {
        /// <summary>
        /// 绑定的源属性名。
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// 源属性名。
        /// </summary>
        public string PropertyName => _propertyName;

        /// <summary>
        /// 运行时动态值转换器，主要用于 object 值入口。
        /// </summary>
        private IConvert<SProperty, object> _converter;

        /// <summary>
        /// 运行时动态值转换器。
        /// </summary>
        public IConvert<SProperty, object> Converter
        {
            get => _converter;
            set => _converter = value;
        }

        /// <summary>
        /// 源对象弱引用，避免绑定系统意外延长 Model 生命周期。
        /// </summary>
        private readonly WeakReference<object> _source;

        /// <summary>
        /// 源属性反射信息。只在读取初始值等少数场景使用，避免反复查找。
        /// </summary>
        private PropertyInfo _sourcePropertyInfo;

        /// <summary>
        /// 源对象实例。
        /// </summary>
        public S Source
        {
            get
            {
                if (!_source.TryGetTarget(out var obj))
                {
                    throw new Exception("源对象已经被释放。");
                }

                return (S)obj;
            }
        }

        /// <summary>
        /// 绑定器字典，用于按哈希快速解绑。
        /// </summary>
        private readonly Dictionary<int, Binder<S, SProperty>> _binders;

        /// <summary>
        /// 绑定器快照。属性变化是热路径，遍历数组比遍历字典更稳定，也允许回调中解绑。
        /// </summary>
        private Binder<S, SProperty>[] _binderSnapshot = Array.Empty<Binder<S, SProperty>>();

        /// <summary>
        /// 创建属性绑定集合。
        /// </summary>
        public Binding(string propertyName, WeakReference<object> source)
        {
            _propertyName = propertyName;
            _source = source;
            _binders = new Dictionary<int, Binder<S, SProperty>>();
        }

        /// <summary>
        /// 注册绑定器。相同哈希再次绑定会替换旧绑定，避免重复绑定直接抛异常。
        /// </summary>
        public void Bind(Binder<S, SProperty> binder)
        {
            if (_binders.TryGetValue(binder.HashCode, out var oldBinder))
            {
                if (ReferenceEquals(oldBinder, binder))
                {
                    return;
                }

                oldBinder.OnUnbind();
            }

            _binders[binder.HashCode] = binder;
            RebuildSnapshot();
        }

        /// <summary>
        /// 按哈希解绑。
        /// </summary>
        public void Unbind(int hashcode)
        {
            if (!_binders.TryGetValue(hashcode, out Binder<S, SProperty> target))
            {
                throw new Exception($"未找到绑定 {hashcode}");
            }

            _binders.Remove(hashcode);
            RebuildSnapshot();
            target.OnUnbind();
        }

        /// <summary>
        /// 获取当前源属性值。
        /// </summary>
        public bool TryGetCurrentValue(out SProperty value)
        {
            value = default;

            if (!_source.TryGetTarget(out var obj))
            {
                return false;
            }

            _sourcePropertyInfo ??= obj.GetType().GetProperty(_propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (_sourcePropertyInfo == null)
            {
                return false;
            }

            value = ConversionUtility.Convert<SProperty>(_sourcePropertyInfo.GetValue(obj));
            return true;
        }

        /// <summary>
        /// 使用强类型值触发所有绑定器。
        /// </summary>
        public void Invoke(SProperty value)
        {
            if (!_source.TryGetTarget(out var obj))
            {
                throw new Exception("源对象已经被释放。");
            }

            var source = (S)obj;
            var snapshot = _binderSnapshot;

            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].Invoke(source, value);
            }
        }

        /// <summary>
        /// 使用泛型值入口触发绑定。
        /// </summary>
        public override void Invoke<V>(V value)
        {
            if (value is SProperty typedValue)
            {
                Invoke(typedValue);
                return;
            }

            Invoke((object)value);
        }

        /// <summary>
        /// 使用 object 值入口触发绑定。
        /// </summary>
        public override void Invoke(object value)
        {
            var property = Converter != null
                ? Converter.ObjectConvertSource(value)
                : ConversionUtility.Convert<SProperty>(value);

            Invoke(property);
        }

        /// <summary>
        /// 重建绑定器快照。只在绑定表发生变化时调用，避免属性通知热路径分配。
        /// </summary>
        private void RebuildSnapshot()
        {
            _binderSnapshot = new Binder<S, SProperty>[_binders.Count];
            _binders.Values.CopyTo(_binderSnapshot, 0);
        }
    }

    /// <summary>
    /// 非泛型绑定基类，用于 BindingHandler 统一保存不同属性类型的绑定。
    /// </summary>
    public abstract class Binding
    {
        /// <summary>
        /// 使用 object 值触发绑定。
        /// </summary>
        public abstract void Invoke(object value);

        /// <summary>
        /// 使用泛型值触发绑定。
        /// </summary>
        public abstract void Invoke<V>(V value);
    }

    /// <summary>
    /// 可直接以强类型值触发的绑定接口。
    /// </summary>
    public interface IBinding<in V>
    {
        /// <summary>
        /// 触发绑定。
        /// </summary>
        void Invoke(V value);
    }
}

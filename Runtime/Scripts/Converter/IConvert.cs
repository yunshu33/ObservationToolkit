using System;

namespace Voyage.ObservationToolkit.Runtime.Converter
{
    /// <summary>
    /// 双向值转换器。
    /// Source 表示模型属性类型，Target 表示目标 UI 或回调接收的类型。
    /// </summary>
    public interface IConvert<Source, Target>
    {
        /// <summary>
        /// 将 object 值转换为模型属性类型。
        /// 主要用于非泛型入口或运行时动态值回写。
        /// </summary>
        Source ObjectConvertSource(object source);

        /// <summary>
        /// 将模型属性值转换为目标值。
        /// OneWay 和 TwoWay 的 Model -> Target 阶段会调用此方法。
        /// </summary>
        Target SourceConvertTarget(Source source);

        /// <summary>
        /// 将目标值转换回模型属性值。
        /// TwoWay 的 Target -> Model 阶段会调用此方法。
        /// </summary>
        Source TargetConvertSource(Target target);
    }

    /// <summary>
    /// 转换器基类，提供默认的 object -> Source 转换。
    /// 自定义转换器通常只需要实现 SourceConvertTarget 和 TargetConvertSource。
    /// </summary>
    public abstract class ConvertBase<Source, Target> : IConvert<Source, Target>
    {
        /// <summary>
        /// 默认使用统一转换工具处理 object 值，减少自定义转换器的样板代码。
        /// </summary>
        public virtual Source ObjectConvertSource(object source)
        {
            return ConversionUtility.Convert<Source>(source);
        }

        /// <summary>
        /// 将模型属性值转换为目标值。
        /// </summary>
        public abstract Target SourceConvertTarget(Source source);

        /// <summary>
        /// 将目标值转换回模型属性值。
        /// </summary>
        public abstract Source TargetConvertSource(Target target);
    }

    /// <summary>
    /// 基于委托的转换器，适合在绑定处直接传入简单转换函数。
    /// </summary>
    public sealed class DelegateConvert<Source, Target> : ConvertBase<Source, Target>
    {
        /// <summary>
        /// 模型到目标的转换函数。
        /// </summary>
        private readonly Func<Source, Target> _sourceToTarget;

        /// <summary>
        /// 目标到模型的转换函数；OneWay 场景可以为空。
        /// </summary>
        private readonly Func<Target, Source> _targetToSource;

        /// <summary>
        /// 创建委托转换器。
        /// </summary>
        public DelegateConvert(Func<Source, Target> sourceToTarget, Func<Target, Source> targetToSource = null)
        {
            _sourceToTarget = sourceToTarget ?? throw new ArgumentNullException(nameof(sourceToTarget));
            _targetToSource = targetToSource;
        }

        /// <summary>
        /// 调用模型到目标的委托。
        /// </summary>
        public override Target SourceConvertTarget(Source source)
        {
            return _sourceToTarget(source);
        }

        /// <summary>
        /// 调用目标到模型的委托；如果未提供反向转换，则回退到统一转换工具。
        /// </summary>
        public override Source TargetConvertSource(Target target)
        {
            return _targetToSource != null
                ? _targetToSource(target)
                : ConversionUtility.Convert<Source>(target);
        }
    }

    /// <summary>
    /// 示例转换器：int 与 string 双向转换。
    /// </summary>
    public class Convert1 : ConvertBase<int, string>
    {
        /// <summary>
        /// 将 int 转换为 string。
        /// </summary>
        public override string SourceConvertTarget(int source)
        {
            return ConversionUtility.Convert<string>(source);
        }

        /// <summary>
        /// 将 string 转换回 int。
        /// </summary>
        public override int TargetConvertSource(string target)
        {
            return ConversionUtility.Convert<int>(target);
        }
    }
}

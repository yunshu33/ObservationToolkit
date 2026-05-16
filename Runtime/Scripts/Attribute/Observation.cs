using System;

namespace VoyageForge.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标记需要由 IL Weaver 注入观察通知的属性或类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ObservationAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记一个类型或属性使用 Model 语义进行静态织入。
    /// Model 织入只处理对象自身属性：属性 setter 写入自身 backing field 后触发同名属性通知。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ObservableModelAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记一个类型或属性使用 ViewModel 语义进行静态织入。
    /// ViewModel 织入会优先把属性代理到 Data/Model 的同名成员，再触发 ViewModel 属性通知。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ObservableViewModelAttribute : Attribute
    {
    }

    /// <summary>
    /// 标记需要被 IL Weaver 忽略的属性或类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class IgnoreObservationAttribute : Attribute
    {
    }
}

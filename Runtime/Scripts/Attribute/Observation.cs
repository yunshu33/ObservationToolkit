using System;

namespace Voyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标记需要由 IL Weaver 注入观察通知的属性或类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ObservationAttribute : Attribute
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

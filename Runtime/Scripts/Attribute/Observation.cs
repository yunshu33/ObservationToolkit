using System;

namespace LJVoyage.ObservationToolkit.Runtime
{
    
    /// <summary>
    /// 标记需要观察的属性或类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]
    public class ObservationAttribute : Attribute
    {
        
    }
    
    /// <summary>
    /// 标记需要忽略观察的属性或类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]
    public class NoObservationAttribute : Attribute
    {
        
    }
    
    /// <summary>
    /// 标记需要忽略观察的属性或类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Class)]
    public class IgnoreObservationAttribute : Attribute
    {
        
    }
}
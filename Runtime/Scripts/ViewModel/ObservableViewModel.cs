using System;

namespace VoyageForge.ObservationToolkit.Runtime.ViewModel
{
    /// <summary>
    /// ViewModel<TData> 的语义化别名基类。
    /// 如果项目希望在代码层面直接表达“这是面向 View 的可观察对象”，可以继承该类型；
    /// 如果已有继承体系，也可以只实现 IObservableViewModel<TData> 接口。
    /// </summary>
    /// <typeparam name="TData">ViewModel 包装或代理的数据模型类型。</typeparam>
    [Serializable]
    public abstract class ObservableViewModel<TData> : ViewModel<TData> where TData : new()
    {
    }
}

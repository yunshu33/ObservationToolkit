using System;
using Voyage.ObservationToolkit.Runtime;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime.ViewModel
{
    /// <summary>
    /// ViewModel 基类
    /// </summary>
    /// <typeparam name="TData">数据模型类型</typeparam>
    [Serializable]
    public abstract class ViewModel<TData> : IObservable<TData> where TData : new()
    {
        /// <summary>
        /// 绑定处理程序
        /// </summary>
        public BindingHandler BindingHandler { get; set; }

        [SerializeField] private TData _data = new TData();

        /// <summary>
        /// 包装的 Model 数据
        /// </summary>
        public virtual TData Data
        {
            get => _data;
            set => _data = value;
        }
    }
}

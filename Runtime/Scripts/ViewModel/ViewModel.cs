using System;
using UnityEngine;

namespace Voyage.ObservationToolkit.Runtime.ViewModel
{
    /// <summary>
    /// ViewModel 基类，用于包装可序列化数据模型并提供绑定通知能力。
    /// </summary>
    /// <typeparam name="TData">数据模型类型。</typeparam>
    [Serializable]
    public abstract class ViewModel<TData> : IObservable<TData> where TData : new()
    {
        /// <summary>
        /// 绑定处理器。
        /// </summary>
        public BindingHandler BindingHandler { get; set; }

        /// <summary>
        /// 被包装的数据模型实例。
        /// </summary>
        [SerializeField] private TData _data = new TData();

        /// <summary>
        /// 被包装的数据模型。
        /// </summary>
        public virtual TData Data
        {
            get => _data;
            set => _data = value;
        }
    }
}

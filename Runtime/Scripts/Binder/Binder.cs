
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">源</typeparam>
    /// <typeparam name="SProperty">源属性</typeparam>
    /// <typeparam name="TProperty">目标属性</typeparam>    
    public abstract class Binder<S, SProperty,TProperty> : IOneWayBinder<S, SProperty, TProperty>
    {
       
        public abstract void OneWay();
        
        public abstract void OneWay(IConvert<SProperty, TProperty> convert);

        public abstract void Unbind();
    }
    
}
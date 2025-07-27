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
    public abstract class Binder<S, SProperty, TProperty> : Binder<S, SProperty>, IOneWayBinder<S, SProperty, TProperty>
    {
        public abstract void OneWay(IConvert<SProperty, TProperty> convert);
    }


    public abstract class Binder<S, SProperty>
    {
        public abstract void OneWay();

        protected abstract SProperty Convert(object value);

        public abstract void Invoke(S source,object arg1,SProperty arg2);
        

        public abstract void Unbind();


        public abstract string HashCode { get; }
    }
}
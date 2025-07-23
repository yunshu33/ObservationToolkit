using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public interface IOneWayBinder<T, TProperty, U, in UProperty> where U : UIBehaviour
    {
        void OneWay(IConvert<TProperty, UProperty> convert);


        void OneWay();
    }
}
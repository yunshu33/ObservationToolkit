using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public interface ITwoWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        void TwoWay();

        void TwoWay(IConvert<TProperty, UProperty> convert, IConvert<UProperty, TProperty> convert2);
    }
}
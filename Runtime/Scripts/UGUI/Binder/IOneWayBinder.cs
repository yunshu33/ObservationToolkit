using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public interface IOneWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        IDisposableBinding OneWay(IConvert<TProperty, UProperty> convert);

        IDisposableBinding OneWay();
    }
}
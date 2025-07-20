using MvvmToolkit.Converter;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MvvmToolkit.UGUI
{
    public interface ITwoWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        void TwoWay();
        
        void TwoWay(IConvert<TProperty, UProperty> convert, IConvert<UProperty, TProperty> convert2);
        
        
    }
}
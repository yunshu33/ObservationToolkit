using System;
using MvvmToolkit.Converter;
using UnityEngine.EventSystems;

namespace MvvmToolkit.UGUI
{
    public interface IOneWayBinder<T, TProperty, U, in UProperty> where U : UIBehaviour
    {
        void OneWay(IConvert<TProperty, UProperty> convert);


        void OneWay();
    }
}
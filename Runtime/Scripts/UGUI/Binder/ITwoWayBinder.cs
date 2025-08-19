using System;
using System.Linq.Expressions;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public interface ITwoWayBinder<T, TProperty, U,  UProperty> where U : UIBehaviour
    {


        void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);
        
        void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,IConvert<TProperty, UProperty> convert);
    }
}
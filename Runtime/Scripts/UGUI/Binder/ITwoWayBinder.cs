using System;
using System.Linq.Expressions;
using Voyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public interface ITwoWayBinder<T, TProperty, U,  UProperty> where U : UIBehaviour
    {

        IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);
        
        IDisposableBinding TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression,IConvert<TProperty, UProperty> convert);
        
        
        void Unbind(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);
    }
}
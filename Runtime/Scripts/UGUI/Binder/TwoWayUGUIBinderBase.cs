
using System;
using System.Linq.Expressions;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class TwoWayUGUIBinderBase<T, TProperty, U, UProperty> : OneWayUGUIBinderBase<T, TProperty, U, UProperty>,
        ITwoWayBinder<T, TProperty, U, UProperty> where U : UIBehaviour
    {
        protected TwoWayUGUIBinderBase(U target, Action<UProperty> handler, Binding<T, TProperty> binding) : base(target, handler, binding)
        {
            
        }



        public abstract void TwoWay(Expression<Func<U, UnityEvent<UProperty>>> propertyExpression);

        public abstract void TwoWay(IConvert<TProperty, UProperty> convert);
    }
}
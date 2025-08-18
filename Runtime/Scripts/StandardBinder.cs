using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LJVoyage.ObservationToolkit.Runtime.Converter;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 标准的绑定器  
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="SProperty"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public class StandardBinder<S, SProperty, TProperty> : Binder<S, SProperty, TProperty> 
    {
       

        public StandardBinder(Action<TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        public StandardBinder(Action<S, TProperty> handler, Binding<S, SProperty> binding) : base(handler, binding)
        {
        }

        

       
    }
}
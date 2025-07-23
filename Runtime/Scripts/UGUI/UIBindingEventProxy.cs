using System;
using UnityEngine.EventSystems;

namespace LJVoyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class UIBindingEventProxy<T> : UIBehaviour where T : UIBehaviour
    {
        protected T _target;

        public Action onDestroy;
        
        protected override void Awake()
        {
            base.Awake();
            _target = GetComponent<T>();
        }

        public abstract void SetValue(object value);
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            onDestroy?.Invoke();
        }
    }
}
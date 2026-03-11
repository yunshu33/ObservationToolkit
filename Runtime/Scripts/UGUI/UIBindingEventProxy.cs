using System;
using UnityEngine.EventSystems;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public abstract class UIBindingEventProxy<T,TProperty> : UIBehaviour where T : UIBehaviour
    {
        private T _target;


        public T Target
        {
            get
            {
                if (_target == null)
                {
                    _target = GetComponent<T>();
                    if (_target == null)
                    {
                        UnityEngine.Debug.LogError($"{gameObject.name} 上找不到组件 {typeof(T).Name}");
                    }
                }

                return _target;
            }
        }
        
        public Action onDestroy;
        
        protected override void Awake()
        {
            base.Awake();
            _target = GetComponent<T>();
        }

        public abstract void SetValue(TProperty value);
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            onDestroy?.Invoke();
        }
    }
}
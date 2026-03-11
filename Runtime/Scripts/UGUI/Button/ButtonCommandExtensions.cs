using System;
using System.Linq.Expressions;
using Voyage.ObservationToolkit.Runtime;
using Voyage.ObservationToolkit.Runtime.Command;
using UnityEngine;
using UnityEngine.UI;

namespace Voyage.ObservationToolkit.Runtime.UGUI
{
    public static class ButtonCommandExtensions
    {
        public static void BindCommand(this Button button, ICommand command, object parameter = null)
        {
            if (button == null || command == null) return;

            // 获取或添加 Proxy
            if (!button.gameObject.TryGetComponent<CommandProxy>(out var proxy))
            {
                proxy = button.gameObject.AddComponent<CommandProxy>();
            }

            // 清理旧绑定
            proxy.Clear();

            // 创建新绑定
            var binder = new CommandBinder<Button>(button, button.onClick, command, parameter);

            // 注册销毁回调
            proxy.SetBinder(binder);
        }

        public static void BindCommand<S>(this Button button, S source, Expression<Func<S, ICommand>> commandExpression, object parameter = null) where S : class, IObservable
        {
            source.For(commandExpression).To(button).OneWay().AddTo(button);
        }

        /// <summary>
        /// 一个简单的 Proxy 用于管理 CommandBinder 生命周期
        /// </summary>
        private class CommandProxy : MonoBehaviour
        {
            private IDisposable _binder;

            public void SetBinder(IDisposable binder)
            {
                _binder = binder;
            }

            public void Clear()
            {
                _binder?.Dispose();
                _binder = null;
            }

            private void OnDestroy()
            {
                Clear();
            }
        }
    }
}

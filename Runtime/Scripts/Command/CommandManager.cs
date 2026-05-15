using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace VoyageForge.ObservationToolkit.Runtime.Command
{
    /// <summary>
    /// 命令状态管理器。
    /// 该类型负责记录某个 <see cref="ICommand"/> 依赖哪些 <see cref="IObservable"/> 属性，
    /// 并在属性变化时统一触发命令的 <see cref="ICommand.RaiseCanExecuteChanged"/>。
    /// </summary>
    public static class CommandManager
    {
        /// <summary>
        /// 源对象到命令观察注册表的弱关联表。
        /// 使用 <see cref="ConditionalWeakTable{TKey,TValue}"/> 可以避免命令管理器延长 ViewModel 生命周期。
        /// </summary>
        private static readonly ConditionalWeakTable<IObservable, CommandObservationRegistry> Registries = new();

        /// <summary>
        /// 为命令登记可执行状态依赖的源属性，并返回命令本身，方便在命令创建表达式中链式调用。
        /// </summary>
        /// <typeparam name="TCommand">命令实例的实际类型。</typeparam>
        /// <typeparam name="TSource">可观察源对象类型。</typeparam>
        /// <typeparam name="TProperty">被观察属性的类型。</typeparam>
        /// <param name="command">需要在源属性变化时刷新可执行状态的命令。</param>
        /// <param name="source">提供属性变化通知的可观察源对象。</param>
        /// <param name="propertyExpression">直接指向源属性的表达式，例如 <c>m =&gt; m.Score</c>。</param>
        /// <returns>原始命令实例，便于继续赋值给 <see cref="ICommand"/> 属性。</returns>
        public static TCommand Observes<TCommand, TSource, TProperty>(
            this TCommand command,
            TSource source,
            Expression<Func<TSource, TProperty>> propertyExpression)
            where TCommand : ICommand
            where TSource : class, IObservable
        {
            ObserveCanExecute(command, source, propertyExpression);
            return command;
        }

        /// <summary>
        /// 为命令登记可执行状态依赖的源属性，并返回可释放的观察句柄。
        /// 如果调用方需要在源对象销毁前主动取消观察，可以保存返回值并调用 <see cref="IDisposable.Dispose"/>。
        /// </summary>
        /// <typeparam name="TSource">可观察源对象类型。</typeparam>
        /// <typeparam name="TProperty">被观察属性的类型。</typeparam>
        /// <param name="command">需要在源属性变化时刷新可执行状态的命令。</param>
        /// <param name="source">提供属性变化通知的可观察源对象。</param>
        /// <param name="propertyExpression">直接指向源属性的表达式，例如 <c>m =&gt; m.Score</c>。</param>
        /// <returns>用于取消本次观察关系的释放句柄。</returns>
        public static IDisposable ObserveCanExecute<TSource, TProperty>(
            this ICommand command,
            TSource source,
            Expression<Func<TSource, TProperty>> propertyExpression)
            where TSource : class, IObservable
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));

            var propertyName = GetPropertyName(propertyExpression);
            var registry = Registries.GetValue(source, _ => new CommandObservationRegistry());
            registry.Add(propertyName, command);

            return new CommandObservation(registry, propertyName, command);
        }

        /// <summary>
        /// 根据源对象和属性名刷新所有相关命令的可执行状态。
        /// 该方法由 <see cref="IObservable.OnPropertyChanged{V}"/> 调用，普通业务代码通常不需要手动调用。
        /// </summary>
        /// <param name="source">发生属性变化的可观察源对象。</param>
        /// <param name="propertyName">发生变化的属性名。</param>
        public static void RaiseCanExecuteChanged(IObservable source, string propertyName)
        {
            if (source == null || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            if (Registries.TryGetValue(source, out var registry))
            {
                registry.Raise(propertyName);
            }
        }

        /// <summary>
        /// 从属性表达式中解析属性名。
        /// Command 依赖关系必须绑定到直接属性访问，避免方法调用或复杂表达式造成刷新语义不明确。
        /// </summary>
        /// <typeparam name="TSource">表达式源对象类型。</typeparam>
        /// <typeparam name="TProperty">表达式属性类型。</typeparam>
        /// <param name="propertyExpression">直接指向属性的表达式。</param>
        /// <returns>表达式访问的属性名称。</returns>
        private static string GetPropertyName<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is not MemberExpression memberExpression ||
                memberExpression.Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("命令状态依赖必须为属性表达式。", nameof(propertyExpression));
            }

            return memberExpression.Member.Name;
        }

        /// <summary>
        /// 单个源对象上的命令观察注册表。
        /// 每个属性名对应一组弱引用命令，属性变化时会逐个刷新仍然存活的命令。
        /// </summary>
        private sealed class CommandObservationRegistry
        {
            /// <summary>
            /// 保护注册表读写的同步对象。
            /// 绑定通常在 Unity 主线程发生，但这里保持基本线程安全，避免工具或测试环境并发访问时破坏集合。
            /// </summary>
            private readonly object _syncRoot = new();

            /// <summary>
            /// 属性名到命令弱引用列表的映射。
            /// 命令使用弱引用保存，避免 ViewModel 长时间存活时意外延长命令对象生命周期。
            /// </summary>
            private readonly Dictionary<string, List<WeakReference<ICommand>>> _commandsByProperty = new();

            /// <summary>
            /// 添加一个属性到命令的观察关系。
            /// 同一个命令重复观察同一个属性时会被去重，避免一次属性变化触发多次刷新。
            /// </summary>
            /// <param name="propertyName">被观察的属性名。</param>
            /// <param name="command">需要刷新可执行状态的命令。</param>
            public void Add(string propertyName, ICommand command)
            {
                lock (_syncRoot)
                {
                    if (!_commandsByProperty.TryGetValue(propertyName, out var commands))
                    {
                        commands = new List<WeakReference<ICommand>>();
                        _commandsByProperty[propertyName] = commands;
                    }

                    for (var i = commands.Count - 1; i >= 0; i--)
                    {
                        if (!commands[i].TryGetTarget(out var existingCommand))
                        {
                            commands.RemoveAt(i);
                            continue;
                        }

                        if (ReferenceEquals(existingCommand, command))
                        {
                            return;
                        }
                    }

                    commands.Add(new WeakReference<ICommand>(command));
                }
            }

            /// <summary>
            /// 移除一个属性到命令的观察关系。
            /// 当属性列表被清空时，会移除对应字典项，保持注册表紧凑。
            /// </summary>
            /// <param name="propertyName">被观察的属性名。</param>
            /// <param name="command">需要取消观察的命令。</param>
            public void Remove(string propertyName, ICommand command)
            {
                lock (_syncRoot)
                {
                    if (!_commandsByProperty.TryGetValue(propertyName, out var commands))
                    {
                        return;
                    }

                    for (var i = commands.Count - 1; i >= 0; i--)
                    {
                        if (!commands[i].TryGetTarget(out var existingCommand) ||
                            ReferenceEquals(existingCommand, command))
                        {
                            commands.RemoveAt(i);
                        }
                    }

                    if (commands.Count == 0)
                    {
                        _commandsByProperty.Remove(propertyName);
                    }
                }
            }

            /// <summary>
            /// 刷新指定属性关联的所有命令。
            /// 该方法会先在锁内复制仍然存活的命令快照，再在锁外触发事件，避免命令回调中修改注册表导致死锁。
            /// </summary>
            /// <param name="propertyName">发生变化的属性名。</param>
            public void Raise(string propertyName)
            {
                ICommand[] commandsSnapshot;

                lock (_syncRoot)
                {
                    if (!_commandsByProperty.TryGetValue(propertyName, out var commands))
                    {
                        return;
                    }

                    var liveCommands = new List<ICommand>(commands.Count);
                    for (var i = commands.Count - 1; i >= 0; i--)
                    {
                        if (commands[i].TryGetTarget(out var command))
                        {
                            liveCommands.Add(command);
                        }
                        else
                        {
                            commands.RemoveAt(i);
                        }
                    }

                    if (commands.Count == 0)
                    {
                        _commandsByProperty.Remove(propertyName);
                    }

                    commandsSnapshot = liveCommands.ToArray();
                }

                for (var i = 0; i < commandsSnapshot.Length; i++)
                {
                    commandsSnapshot[i].RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 单次命令观察关系的释放句柄。
        /// 该句柄只负责取消注册关系，不拥有命令或源对象的生命周期。
        /// </summary>
        private sealed class CommandObservation : IDisposable
        {
            /// <summary>
            /// 保存观察关系的注册表。
            /// </summary>
            private readonly CommandObservationRegistry _registry;

            /// <summary>
            /// 当前观察关系对应的属性名。
            /// </summary>
            private readonly string _propertyName;

            /// <summary>
            /// 当前观察关系对应的命令。
            /// </summary>
            private readonly ICommand _command;

            /// <summary>
            /// 标记该句柄是否已经释放。
            /// 重复释放会被忽略，保证调用方可以安全地在多个生命周期入口中调用。
            /// </summary>
            private bool _isDisposed;

            /// <summary>
            /// 创建命令观察释放句柄。
            /// </summary>
            /// <param name="registry">保存观察关系的注册表。</param>
            /// <param name="propertyName">当前观察关系对应的属性名。</param>
            /// <param name="command">当前观察关系对应的命令。</param>
            public CommandObservation(CommandObservationRegistry registry, string propertyName, ICommand command)
            {
                _registry = registry;
                _propertyName = propertyName;
                _command = command;
            }

            /// <summary>
            /// 取消当前命令观察关系。
            /// </summary>
            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _registry.Remove(_propertyName, _command);
                _isDisposed = true;
            }
        }
    }
}

using System;
using System.Linq.Expressions;

namespace LJVoyage.ObservationToolkit.Runtime
{
    /// <summary>
    /// 双向绑定的包装类
    /// </summary>
    public abstract class TwoWayBindingWrapper<Source,  Target>
        where Target : class
    {
        protected Action<Source> _action;

        private readonly IBindingHolder _bindingHandler;

        protected readonly Target _target;

        private readonly string _propertyName;

        protected TwoWayBindingWrapper(Target target, IBindingHolder bindingHandler, string propertyName)
        {
            _bindingHandler = bindingHandler;
            _target = target;
            _propertyName = propertyName;
        }

        /// <summary>
        /// 创建一个 Action 用于给 _binderHandler 中 _propertyName 对应的属性赋值
        /// </summary>
        /// <returns>返回一个 Action 委托</returns>
        public virtual void CreateAssignmentAction()
        {
            // 获取 _binderHandler 类型
            var binderHandlerType = _bindingHandler.GetType();
            // 获取 _propertyName 对应的属性信息
            var propertyInfo = binderHandlerType.GetProperty(_propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"未找到属性 {_propertyName} 在类型 {binderHandlerType.Name} 中。");
            }

            // 创建参数表达式
            var valueParam = Expression.Parameter(typeof(Source), "value");
            // 创建 _binderHandler 的参数表达式
            var binderHandlerParam = Expression.Parameter(typeof(IBindingHolder), "binderHandler");
            
            // 将 binderHandlerParam 转换为 binderHandlerType 类型
            var convertedBinderHandlerParam = Expression.Convert(binderHandlerParam, binderHandlerType);
            // 创建属性访问表达式，使用转换后的参数
            var propertyAccess = Expression.Property(convertedBinderHandlerParam, propertyInfo);
            
            // 处理类型转换
            Expression convertedValue;
            if (typeof(Source) == typeof(string) && propertyInfo.PropertyType == typeof(int))
            {
                // 如果是 string 转 int，调用 int.Parse 方法
                var parseMethod = typeof(int).GetMethod("Parse", new[] { typeof(string) });
                convertedValue = Expression.Call(parseMethod, valueParam);
                
                
            }
            else if (typeof(Source) == typeof(string) && propertyInfo.PropertyType == typeof(float))
            {
                // 如果是 string 转 float，调用 float.Parse 方法
                var parseMethod = typeof(float).GetMethod("Parse", new[] { typeof(string) });
                convertedValue = Expression.Call(parseMethod, valueParam);
            }
            else
            {
                // 其他情况使用普通的 Convert 方法
                convertedValue = Expression.Convert(valueParam, propertyInfo.PropertyType);
            }
            
            

            // 创建赋值表达式
            var assignExpression = Expression.Assign(propertyAccess, convertedValue);
            // 创建 Lambda 表达式
            var lambda = Expression.Lambda<Action<IBindingHolder, Source>>(assignExpression, binderHandlerParam, valueParam);

            // 编译 Lambda 表达式得到委托
            var compiledLambda = lambda.Compile();

            // 返回一个 Action<S> 委托，在执行时调用编译后的 Lambda 表达式
            _action = value => compiledLambda(_bindingHandler, value);
        }


        /// <summary>
        /// 追加 Action
        /// </summary>
        protected abstract void AppendAction();
    }
}
using System;
using System.Linq;
using Voyage.ObservationToolkit.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Voyage.ObservationToolkit.Editor
{
    /// <summary>
    /// 处理 ViewModel<T> 类型的属性代理
    /// 将 ViewModel 的属性读写重定向到 Data 的字段
    /// </summary>
    public class ViewModelWeaver
    {
        private readonly TypeDefinition _type;
        private readonly MethodReference _setFieldRef;

        public ViewModelWeaver(TypeDefinition type, MethodReference setFieldRef)
        {
            _type = type;
            _setFieldRef = setFieldRef;
        }

        public void Process()
        {
            // 检查是否继承自 ViewModel<T>
            var baseType = _type.BaseType;
            if (baseType == null || !baseType.IsGenericInstance || baseType.Name != "ViewModel`1")
                return;

            var genericBase = (GenericInstanceType)baseType;
            var tDataRef = genericBase.GenericArguments[0];
            var tDataDef = tDataRef.Resolve();

            if (tDataDef == null) return;

            // 获取 get_Data() 方法引用
            var getDataMethodRef = GetGetDataMethodRef(baseType, genericBase);
            if (getDataMethodRef == null) return;

            bool isClassObservable = _type.CustomAttributes.Any(x => x.AttributeType.Name == nameof(ObservationAttribute));

            foreach (var prop in _type.Properties)
            {
                // 使用统一的过滤逻辑
                if (WeaverHelper.ShouldIgnoreObservation(prop, isClassObservable))
                    continue;

                // 查找 TData 中对应的字段（忽略大小写）
                var dataField = tDataDef.Fields.FirstOrDefault(f => f.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                if (dataField == null) continue;

                // 导入字段定义
                var dataFieldRef = _type.Module.ImportReference(dataField);

                // 重写 Getter
                if (prop.GetMethod != null)
                {
                    RewriteProxyGetter(prop.GetMethod, getDataMethodRef, dataFieldRef);
                }

                // 重写 Setter
                if (prop.SetMethod != null)
                {
                    RewriteProxySetter(prop.SetMethod, getDataMethodRef, dataFieldRef, _setFieldRef, prop.Name);
                }
            }
        }

        private MethodReference GetGetDataMethodRef(TypeReference baseType, GenericInstanceType genericBase)
        {
            // 查找 Data 属性的 getter 方法
            // 在 ViewModel<T> 中，Data 属性是 virtual 的。
            // 我们需要调用 get_Data() 来获取数据对象。
            var dataProp = _type.Properties.FirstOrDefault(p => p.Name == "Data");
            
            // 如果在子类找不到，去基类找
            if (dataProp == null)
            {
                var resolvedBase = baseType.Resolve();
                if (resolvedBase != null)
                    dataProp = resolvedBase.Properties.FirstOrDefault(p => p.Name == "Data");
            }
        
            if (dataProp == null) return null;
        
            // 如果 dataProp 是在基类定义的，我们需要正确处理泛型上下文
            if (dataProp.DeclaringType.FullName == baseType.Resolve().FullName)
            {
                // 直接使用 baseType (GenericInstanceType) 作为声明类型
                var genericDeclaringType = baseType;

                // 创建方法引用指向 ViewModel<T>.get_Data
                var originalMethod = dataProp.GetMethod;
                var methodRef = new MethodReference(originalMethod.Name, originalMethod.ReturnType, genericDeclaringType)
                {
                    HasThis = originalMethod.HasThis,
                    ExplicitThis = originalMethod.ExplicitThis,
                    CallingConvention = originalMethod.CallingConvention
                };
            
                // 这里的 ReturnType 是 !0 (GenericParameter)，它属于 Runtime.dll 的 Definition。
                // 赋值给 methodRef 后，Cecil 在 Write 时会正确处理它作为签名的一部分。
                methodRef.ReturnType = originalMethod.ReturnType; 

                return methodRef; 
            }
            
            // 如果 Data 是在子类中定义的（或者 override 的）
            return _type.Module.ImportReference(dataProp.GetMethod);
        }

        private void RewriteProxyGetter(MethodDefinition method, MethodReference getDataRef, FieldReference dataFieldRef)
        {
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.InitLocals = false; // 清除局部变量初始化标记

            var il = method.Body.GetILProcessor();

            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Callvirt, getDataRef); // get_Data()
            il.Emit(OpCodes.Ldfld, dataFieldRef); // .field
            il.Emit(OpCodes.Ret);
        }

        private void RewriteProxySetter(MethodDefinition method, MethodReference getDataRef, FieldReference dataFieldRef, MethodReference setFieldRef, string propName)
        {
            method.Body.Instructions.Clear();
            method.Body.Variables.Clear();
            method.Body.InitLocals = false;

            var il = method.Body.GetILProcessor();

            var genericSetField = new GenericInstanceMethod(setFieldRef);
            
            // 泛型参数处理：直接使用 FieldType
            genericSetField.GenericArguments.Add(dataFieldRef.FieldType);

            // SetField(this, ref Data.field, value, name)
            
            il.Emit(OpCodes.Ldarg_0); // arg0: this (IObservable)
            
            // arg1: ref Data.field
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Callvirt, getDataRef); // get_Data()
            il.Emit(OpCodes.Ldflda, dataFieldRef); // ref .field
            
            il.Emit(OpCodes.Ldarg_1); // arg2: value
            il.Emit(OpCodes.Ldstr, propName); // arg3: propertyName
            
            il.Emit(OpCodes.Call, genericSetField); 
            il.Emit(OpCodes.Pop); // pop bool result
            il.Emit(OpCodes.Ret);
        }
    }
}

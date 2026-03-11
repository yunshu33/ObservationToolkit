using System.Linq;
using Voyage.ObservationToolkit.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Voyage.ObservationToolkit.Editor
{
    /// <summary>
    /// 标准属性通知编织器
    /// 处理普通类的属性通知注入
    /// </summary>
    public class PropertyWeaver
    {
        private readonly TypeDefinition _type;
        private readonly MethodReference _setFieldRef;

        public PropertyWeaver(TypeDefinition type, MethodReference setFieldRef)
        {
            _type = type;
            _setFieldRef = setFieldRef;
        }

        public void Process()
        {
            // 判断类级别是否标记了 [Observation]
            bool isClassObservable = _type.CustomAttributes.Any(x => x.AttributeType.Name == nameof(ObservationAttribute));

            foreach (var prop in _type.Properties)
            {
                // 检查是否应该编织该属性
                if (WeaverHelper.ShouldIgnoreObservation(prop, isClassObservable))
                    continue;

                // 尝试获取 BackingField
                var backingField = prop.BackingField();
                if (backingField == null) continue;

                var setMethod = prop.SetMethod;
                if (setMethod == null) continue;

                setMethod.Body.SimplifyMacros();

                // 执行 IL 替换
                InjectNotification(setMethod, backingField, prop.Name);

                setMethod.Body.Optimize();
            }
        }

        private void InjectNotification(MethodDefinition setMethod, FieldReference backingField, string propName)
        {
            var il = setMethod.Body.GetILProcessor();
            var instructions = setMethod.Body.Instructions.ToList();
            
            // 找到所有 stfld 指令对应 backingField
            var stfldIndices = instructions
                .Select((instr, idx) => new { instr, idx })
                .Where(x => x.instr.OpCode == OpCodes.Stfld && x.instr.Operand == backingField)
                .Select(x => x.idx)
                .ToList();

            if (stfldIndices.Count == 0) return;

            // 将每个 stfld 替换为 stloc tmpVar + SetField 调用
            foreach (var idx in stfldIndices)
            {
                // 添加临时变量
                VariableDefinition tmpVar = new VariableDefinition(backingField.FieldType);
                setMethod.Body.Variables.Add(tmpVar);
                setMethod.Body.InitLocals = true;

                var storeTmp = il.Create(OpCodes.Stloc, tmpVar);
                il.Replace(instructions[idx], storeTmp);

                var genericSetField = new GenericInstanceMethod(_setFieldRef);
                genericSetField.GenericArguments.Add(backingField.FieldType);

                // 注入 SetField 调用
                // SetField(this, ref _field, value, propName)
                
                il.InsertAfter(storeTmp, il.Create(OpCodes.Ldarg_0)); // this
                il.InsertAfter(storeTmp.Next, il.Create(OpCodes.Ldflda, backingField)); // ref _field
                il.InsertAfter(storeTmp.Next.Next, il.Create(OpCodes.Ldloc, tmpVar)); // value (from tmpVar)
                il.InsertAfter(storeTmp.Next.Next.Next, il.Create(OpCodes.Ldstr, propName)); // propertyName
                il.InsertAfter(storeTmp.Next.Next.Next.Next, il.Create(OpCodes.Call, genericSetField));
                il.InsertAfter(storeTmp.Next.Next.Next.Next.Next, il.Create(OpCodes.Pop)); // 丢弃 SetField 返回的 bool
            }
        }
    }
}

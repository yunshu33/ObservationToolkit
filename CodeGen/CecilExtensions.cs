using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Voyage.ObservationToolkit.Editor
{
    /// <summary>
    /// Mono.Cecil 扩展方法
    /// </summary>
    public static class CecilExtensions
    {
        /// <summary>
        /// 获取属性对应的 Backing Field
        /// </summary>
        public static FieldReference BackingField(this PropertyDefinition prop)
        {
            var candidate = TryGetBackingFieldFromIL(prop);
            if (candidate != null) return candidate;

            var typeDef = prop.DeclaringType;
            var names = new[]
            {
                $"<{prop.Name}>k__BackingField",
                $"_{prop.Name}",
                $"m_{prop.Name.ToLower()}",
                $"{prop.Name.ToLower()}"
            };

            foreach (var f in typeDef.Fields)
                if (names.Contains(f.Name))
                    return f;

            return null;
        }

        private static FieldReference TryGetBackingFieldFromIL(PropertyDefinition prop)
        {
            var setMethod = prop.SetMethod;
            if (setMethod != null && setMethod.HasBody)
            {
                foreach (var instr in setMethod.Body.Instructions)
                    if (instr.OpCode == OpCodes.Stfld || instr.OpCode == OpCodes.Ldfld ||
                        instr.OpCode == OpCodes.Ldflda)
                        return (FieldReference)instr.Operand;
            }

            var getMethod = prop.GetMethod;
            if (getMethod != null && getMethod.HasBody)
            {
                foreach (var instr in getMethod.Body.Instructions)
                    if (instr.OpCode == OpCodes.Ldfld || instr.OpCode == OpCodes.Ldflda)
                        return (FieldReference)instr.Operand;
            }

            return null;
        }

        /// <summary>
        /// 获取所有类型（包括嵌套类型）
        /// </summary>
        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition module)
        {
            var types = new List<TypeDefinition>(module.Types);
            for (int i = 0; i < types.Count; i++)
            {
                if (types[i].HasNestedTypes)
                    types.AddRange(types[i].NestedTypes);
            }

            return types;
        }
    }
}

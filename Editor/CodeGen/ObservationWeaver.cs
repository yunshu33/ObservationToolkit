using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LJVoyage.ObservationToolkit.Editor.CodeGen;
using LJVoyage.ObservationToolkit.Runtime;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Editor
{
    public class ObservationWeaver : ILPostProcessor
    {
        private const string BindingAssemblyName = "LJVoyage.ObservationToolkit.Runtime";


        string runtimeDllPath = Path.Combine(
            Environment.CurrentDirectory,
            "Library/ScriptAssemblies/LJVoyage.ObservationToolkit.Runtime.dll");

        Assembly _runtimeAssembly;

        public override ILPostProcessor GetInstance()
        {
            if (_runtimeAssembly == null)
            {
                string runtimeDllPath = Path.Combine(
                    Environment.CurrentDirectory,
                    "Library/ScriptAssemblies/LJVoyage.ObservationToolkit.Runtime.dll");

                _runtimeAssembly = Assembly.LoadFrom(runtimeDllPath);
            }

            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            List<string> assemblies = new List<string> { "Assembly-CSharp" };

            // 非Editor 环境下  无法使用 AssetDatabase

            // var guid = AssetDatabase.FindAssets("t:AssemblyGroup");
            //
            // if (guid != null && guid.Length > 0)
            // {
            //     var path = AssetDatabase.GUIDToAssetPath(guid[0]);
            //     var asset = AssetDatabase.LoadAssetAtPath<AssemblyGroup>(path);
            //     if (asset != null)
            //     {
            //         assemblies.AddRange(asset.Assemblies.Select(x => x.name));
            //     }
            // }
            //
            // foreach (var assembly in assemblies)
            // {
            //    Console.WriteLine(assembly);
            // }


            return assemblies.Any(x => x == compiledAssembly.Name);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var module = ModuleDefinition.ReadModule(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData));

            var setFieldRef = FindSetFieldInOtherAssembly(module);

            if (setFieldRef == null)
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly);

            foreach (var type in module.GetAllTypes())
            {
                if (!type.HasProperties) continue;

                if (type.CustomAttributes.All(x => x.AttributeType.Name != nameof(ObservationAttribute)))
                    continue;

                foreach (var prop in type.Properties)
                {
                    if (ShouldIgnoreObservation(prop))
                        continue;

                    var backingField = prop.BackingField();

                    if (backingField == null) continue;

                    var setMethod = prop.SetMethod;

                    setMethod.Body.SimplifyMacros();

                    var il = setMethod.Body.GetILProcessor();

                    // 找到所有 stfld 指令对应 backingField
                    var instructions = setMethod.Body.Instructions.ToList();
                    var stfldIndices = instructions
                        .Select((instr, idx) => new { instr, idx })
                        .Where(x => x.instr.OpCode == OpCodes.Stfld && x.instr.Operand == backingField)
                        .Select(x => x.idx)
                        .ToList();
                    if (stfldIndices.Count == 0) continue;

                    // 将每个 stfld 替换为 stloc tmpVar
                    foreach (var idx in stfldIndices)
                    {
                        // 添加临时变量
                        VariableDefinition tmpVar = new VariableDefinition(backingField.FieldType);
                        setMethod.Body.Variables.Add(tmpVar);
                        setMethod.Body.InitLocals = true;

                        var storeTmp = il.Create(OpCodes.Stloc, tmpVar);
                        il.Replace(instructions[idx], storeTmp);

                        var genericSetField = new GenericInstanceMethod(setFieldRef);
                        genericSetField.GenericArguments.Add(backingField.FieldType);


                        il.InsertAfter(storeTmp, il.Create(OpCodes.Ldarg_0)); // this -> binding
                        il.InsertAfter(storeTmp.Next, il.Create(OpCodes.Ldflda, backingField));
                        // ref _field
                        il.InsertAfter(storeTmp.Next.Next, il.Create(OpCodes.Ldloc, tmpVar));
                        // tmpVar
                        il.InsertAfter(storeTmp.Next.Next.Next, il.Create(OpCodes.Ldstr, prop.Name));
                        // 属性名
                        il.InsertAfter(storeTmp.Next.Next.Next.Next, il.Create(OpCodes.Call, genericSetField));
                        il.InsertAfter(storeTmp.Next.Next.Next.Next.Next, il.Create(OpCodes.Pop)); // 丢弃 bool
                    }

                    // 方法尾调用 SetField<T>
                    // var lastInstr = setMethod.Body.Instructions.Last(i => i.OpCode == OpCodes.Ret);


                    setMethod.Body.Optimize();
                }
            }

            using (var ms = new MemoryStream())
            {
                module.Write(ms);
                return new ILPostProcessResult(new InMemoryAssembly(ms.ToArray(),
                    compiledAssembly.InMemoryAssembly.PdbData));
            }
        }

        private static bool ShouldIgnoreObservation(PropertyDefinition prop)
        {
            if (prop.PropertyType.FullName == typeof(BindingHandler).FullName)
                return true;

            // 先检查子类自身属性
            if (prop.CustomAttributes.Any(a => a.AttributeType.Name == nameof(IgnoreObservationAttribute)))
                return true;

            if (prop.CustomAttributes.Any(a => a.AttributeType.Name == nameof(ObservationAttribute)))
                return false;

            // 如果子类没标记，再检查父类链
            var type = prop.DeclaringType.BaseType?.Resolve();
            var propName = prop.Name;

            while (type != null)
            {
                var baseProp = type.Properties.FirstOrDefault(p => p.Name == propName);
                if (baseProp != null)
                {
                    if (baseProp.CustomAttributes.Any(a => a.AttributeType.Name == nameof(IgnoreObservationAttribute)))
                        return true;
                }

                type = type.BaseType?.Resolve();
            }
            // 3. 接口  暂时不支持
            // foreach (var iface in prop.DeclaringType.Interfaces)
            // {
            //     
            // }

            return false;
        }

        private static MethodReference FindSetFieldInOtherAssembly(ModuleDefinition module)
        {
            var runtimeDllPath = Path.Combine(
                Environment.CurrentDirectory,
                "Library/ScriptAssemblies/LJVoyage.ObservationToolkit.Runtime.dll");

            if (!File.Exists(runtimeDllPath)) return null;

            var runtimeModule = ModuleDefinition.ReadModule(runtimeDllPath);
            var bindingExtensions = runtimeModule.Types.FirstOrDefault(t => t.Name == "BindingExtensions");
            var methodDef = bindingExtensions?.Methods.FirstOrDefault(m =>
                m.IsStatic && m.HasGenericParameters && m.Name == "SetField" &&
                m.Parameters.Count >= 2 && m.Parameters[0].ParameterType.Name == "IObservable");

            return methodDef == null ? null : module.ImportReference(methodDef);
        }
    }


    public static class CecilExtensions
    {
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
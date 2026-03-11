using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Voyage.ObservationToolkit.Runtime.ViewModel;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine;

namespace Voyage.ObservationToolkit.Editor
{
    /// <summary>
    /// 观察工具编织器
    /// 负责在编译后处理程序集，注入属性通知代码
    /// </summary>
    public class ObservationWeaver : ILPostProcessor
    {
        [Serializable]
        private class WeaverConfig
        {
            public bool enableWeaver = true;
            public bool weaveAssemblyCSharp = true;
            public List<string> extraAssemblies = new List<string>();
        }

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            List<string> assemblies = new()
            {
                "Assembly-CSharp",
                "Voyage.ObservationToolkit.Sample"
            };

            // Load from config
            var configPath = Path.Combine(Environment.CurrentDirectory, "ProjectSettings", "ObservationWeaverSettings.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonUtility.FromJson<WeaverConfig>(json);
                    if (config != null)
                    {
                        // 1. Check Global Switch
                        if (!config.enableWeaver)
                        {
                            return false;
                        }

                        // 2. Build target list
                        assemblies.Clear();
                        
                        if (config.weaveAssemblyCSharp)
                        {
                            assemblies.Add("Assembly-CSharp");
                        }

                        if (config.extraAssemblies != null)
                        {
                            assemblies.AddRange(config.extraAssemblies);
                        }
                    }
                }
                catch
                {
                    // Fallback to default if load fails
                }
            }

            return assemblies.Any(x => x == compiledAssembly.Name);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var readerParameters = CreateReaderParameters(compiledAssembly);
            var module = ModuleDefinition.ReadModule(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData), readerParameters);

            var setFieldRef = FindSetFieldInOtherAssembly(module);

            if (setFieldRef == null)
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly);

            foreach (var type in module.GetAllTypes())
            {
                if (!type.HasProperties) continue;

                // 1. 处理 ViewModel<T> 自动代理
                new ViewModelWeaver(type, setFieldRef).Process();

                // 2. 处理标准属性通知
                new PropertyWeaver(type, setFieldRef).Process();
            }

            using (var ms = new MemoryStream())
            {
                module.Write(ms);
                return new ILPostProcessResult(new InMemoryAssembly(ms.ToArray(),
                    compiledAssembly.InMemoryAssembly.PdbData));
            }
        }

        private ReaderParameters CreateReaderParameters(ICompiledAssembly compiledAssembly)
        {
            var resolver = new DefaultAssemblyResolver();
            foreach (var reference in compiledAssembly.References)
            {
                resolver.AddSearchDirectory(Path.GetDirectoryName(reference));
            }

            // 关键：将当前项目根目录下的 Library/ScriptAssemblies 也加入搜索路径
            var scriptAssemblies = Path.Combine(Environment.CurrentDirectory, "Library", "ScriptAssemblies");
            if (Directory.Exists(scriptAssemblies))
            {
                resolver.AddSearchDirectory(scriptAssemblies);
            }

            var readerParameters = new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadSymbols = true,
                SymbolReaderProvider = new PortablePdbReaderProvider(),
            };

            if (compiledAssembly.InMemoryAssembly.PdbData != null && compiledAssembly.InMemoryAssembly.PdbData.Length > 0)
            {
                readerParameters.SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData);
            }
            else
            {
                readerParameters.ReadSymbols = false;
                readerParameters.SymbolReaderProvider = null;
            }

            return readerParameters;
        }

        /// <summary>
        /// 查找 Runtime 中的 SetField 方法
        /// </summary>
        private static MethodReference FindSetFieldInOtherAssembly(ModuleDefinition module)
        {
            var runtimeDllPath = Path.Combine(
                Environment.CurrentDirectory,
                "Library/ScriptAssemblies/Voyage.ObservationToolkit.Runtime.dll");

            if (!File.Exists(runtimeDllPath)) return null;

            var runtimeModule = ModuleDefinition.ReadModule(runtimeDllPath);
            var bindingExtensions = runtimeModule.Types.FirstOrDefault(t => t.Name == "BindingExtensions");
            var methodDef = bindingExtensions?.Methods.FirstOrDefault(m =>
                m.IsStatic && m.HasGenericParameters && m.Name == "SetField" &&
                m.Parameters.Count >= 2 && m.Parameters[0].ParameterType.Name == "IObservable");

            return methodDef == null ? null : module.ImportReference(methodDef);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VoyageForge.ObservationToolkit.Runtime.ViewModel;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace VoyageForge.ObservationToolkit.Editor
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
            public bool enableLogging = true;
            public bool weaveAssemblyCSharp = true;
            public List<string> extraAssemblies = new List<string>();
        }

        private static bool _enableLogging = true;
        private static string LogFilePath => Path.Combine(
            Environment.CurrentDirectory,
            "Library",
            "ObservationToolkit",
            "ObservationWeaver.log");

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            _enableLogging = true;
            var configPath = Path.Combine(Environment.CurrentDirectory, "ProjectSettings", "ObservationWeaverSettings.json");
            List<string> assemblies = new()
            {
                "Assembly-CSharp",
                "VoyageForge.ObservationToolkit.Sample"
            };

            // Load from config
            if (File.Exists(configPath))
            {
                try
                {
                    var config = LoadConfig(configPath);
                    if (config != null)
                    {
                        _enableLogging = config.enableLogging;

                        // 1. Check Global Switch
                        if (!config.enableWeaver)
                        {
                            LogToFile($"Skipped {compiledAssembly.Name}: weaver is disabled in {configPath}.");
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
                            assemblies.AddRange(NormalizeAssemblyNames(config.extraAssemblies));
                        }
                    }
                }
                catch
                {
                    // Fallback to default if load fails
                }
            }

            var willProcess = assemblies.Any(x => x == compiledAssembly.Name);
            if (willProcess)
            {
                LogToFile($"Will process {compiledAssembly.Name}.");
            }

            return willProcess;
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            _enableLogging = LoadLoggingEnabled();
            var diagnostics = new List<DiagnosticMessage>();
            Log(diagnostics, $"Processing {compiledAssembly.Name}.");

            var readerParameters = CreateReaderParameters(compiledAssembly);
            var module = ModuleDefinition.ReadModule(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData), readerParameters);

            var setFieldRef = FindSetFieldInOtherAssembly(module);

            if (setFieldRef == null)
            {
                Log(diagnostics, $"Skipped {compiledAssembly.Name}: could not resolve BindingExtensions.SetField from Runtime assembly.");
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, diagnostics);
            }

            var propertyWeaveCount = 0;
            var viewModelRewriteCount = 0;

            foreach (var type in module.GetAllTypes())
            {
                if (!type.HasProperties) continue;

                // 1. 处理 ViewModel<T> 自动代理
                viewModelRewriteCount += new ViewModelWeaver(type, setFieldRef).Process();

                // 2. 处理标准属性通知
                propertyWeaveCount += new PropertyWeaver(type, setFieldRef).Process();
            }

            Log(diagnostics, $"Processed {compiledAssembly.Name}: property notifications={propertyWeaveCount}, view model accessors={viewModelRewriteCount}.");

            using (var ms = new MemoryStream())
            {
                if (module.HasSymbols)
                {
                    using var pdb = new MemoryStream();
                    module.Write(ms, new WriterParameters
                    {
                        WriteSymbols = true,
                        SymbolWriterProvider = new PortablePdbWriterProvider(),
                        SymbolStream = pdb
                    });

                    return new ILPostProcessResult(new InMemoryAssembly(ms.ToArray(), pdb.ToArray()), diagnostics);
                }

                module.Write(ms);
                return new ILPostProcessResult(new InMemoryAssembly(ms.ToArray(), null), diagnostics);
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
                "Library/ScriptAssemblies/VoyageForge.ObservationToolkit.Runtime.dll");

            if (!File.Exists(runtimeDllPath)) return null;

            var runtimeModule = ModuleDefinition.ReadModule(runtimeDllPath);
            var bindingExtensions = runtimeModule.Types.FirstOrDefault(t => t.Name == "BindingExtensions");
            var methodDef = bindingExtensions?.Methods.FirstOrDefault(m =>
                m.IsStatic && m.HasGenericParameters && m.Name == "SetField" &&
                m.Parameters.Count >= 2 && m.Parameters[0].ParameterType.Name == "IObservable");

            return methodDef == null ? null : module.ImportReference(methodDef);
        }

        private static void Log(List<DiagnosticMessage> diagnostics, string message)
        {
            if (!_enableLogging) return;
            diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Warning,
                MessageData = $"[Observation Weaver] {message}"
            });
            LogToFile(message);
        }

        private static void LogToFile(string message)
        {
            if (!_enableLogging) return;

            try
            {
                var directory = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(
                    LogFilePath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [Observation Weaver] {message}{Environment.NewLine}");
            }
            catch
            {
                // ILPP logging must never fail compilation.
            }
        }

        private static bool LoadLoggingEnabled()
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "ProjectSettings", "ObservationWeaverSettings.json");
            if (!File.Exists(configPath)) return true;

            try
            {
                return LoadConfig(configPath)?.enableLogging ?? true;
            }
            catch
            {
                return true;
            }
        }

        private static WeaverConfig LoadConfig(string configPath)
        {
            var json = File.ReadAllText(configPath);
            var config = new WeaverConfig
            {
                enableWeaver = ReadBool(json, "enableWeaver", true),
                enableLogging = ReadBool(json, "enableLogging", true),
                weaveAssemblyCSharp = ReadBool(json, "weaveAssemblyCSharp", true),
                extraAssemblies = NormalizeAssemblyNames(ReadStringArray(json, "extraAssemblies"))
            };

            return config;
        }

        private static bool ReadBool(string json, string name, bool defaultValue)
        {
            var match = Regex.Match(json, $"\"{Regex.Escape(name)}\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
            return match.Success ? bool.Parse(match.Groups[1].Value) : defaultValue;
        }

        private static List<string> ReadStringArray(string json, string name)
        {
            var match = Regex.Match(json, $"\"{Regex.Escape(name)}\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
            if (!match.Success)
            {
                return new List<string>();
            }

            return Regex.Matches(match.Groups[1].Value, "\"((?:\\\\.|[^\"])*)\"")
                .Cast<Match>()
                .Select(x => Regex.Unescape(x.Groups[1].Value))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        private static List<string> NormalizeAssemblyNames(IEnumerable<string> assemblyNames)
        {
            return assemblyNames
                .Select(NormalizeAssemblyName)
                .Distinct()
                .ToList();
        }

        private static string NormalizeAssemblyName(string assemblyName)
        {
            return assemblyName == "Voyage.ObservationToolkit.Sample"
                ? "VoyageForge.ObservationToolkit.Sample"
                : assemblyName;
        }
    }
}

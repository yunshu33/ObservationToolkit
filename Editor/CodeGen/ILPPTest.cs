using System.IO;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor;
using UnityEngine;

namespace LJVoyage.ObservationToolkit.Editor.CodeGen
{
    public class ILPPTest
    {
        [MenuItem("Tools/Run ILPostProcessor")]
        public static void Run()
        {
            var weaver = new ObservationWeaver();

            // 读取程序集
            var assemblyPath = "Library/ScriptAssemblies/Assembly-CSharp.dll";
            var peData = File.ReadAllBytes(assemblyPath);

            var inMemoryAssembly = new InMemoryAssembly(peData, null);

            var compiledAssembly = new DummyCompiledAssembly()
            {
                Name = "Assembly-CSharp",
                InMemoryAssembly = inMemoryAssembly
            };

            var result = weaver.Process(compiledAssembly);

            if (result != null)
            {
                Debug.Log("ILPostProcessor ran successfully.");
                File.WriteAllBytes("Assets/Assembly-CSharp_Post.dll", result.InMemoryAssembly.PeData);
            }
            else
            {
                Debug.Log("ILPostProcessor did nothing or failed.");
            }
        }
    }
}
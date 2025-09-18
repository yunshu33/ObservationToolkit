using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace LJVoyage.ObservationToolkit.Editor.CodeGen
{
    class DummyCompiledAssembly : ICompiledAssembly
    {
        private string[] _defines;
        public string Name { get; set; }
        public InMemoryAssembly InMemoryAssembly { get; set; }

        // 其它属性可以返回空
        public string[] DefineConstraints => new string[0];
        public string[] References => new string[0];

        public string[] Defines
        {
            get => _defines;
            set => _defines = value;
        }

        public string[] IncludePlatforms => new string[0];
        public string OutputDirectory => "";
    }
}
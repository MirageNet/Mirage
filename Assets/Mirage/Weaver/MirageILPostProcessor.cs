using System.Linq;
using Mirage.CodeGen;
using Unity.CompilationPipeline.Common.ILPostProcessing;


namespace Mirage.Weaver
{
    public class MirageILPostProcessor : ILPostProcessor
    {
        public const string RuntimeAssemblyName = "Mirage";

        public sealed override ILPostProcessor GetInstance() => this;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            return ILPPHelper.CreateAndProcess(compiledAssembly, RuntimeAssemblyName, Create);
        }

        private static Weaver Create(ICompiledAssembly compiledAssembly)
        {
            var enableTrace = compiledAssembly.Defines.Contains("WEAVER_DEBUG_LOGS");
            var logger = new WeaverLogger(enableTrace);
            var weaver = new Weaver(logger);
            return weaver;
        }


        public override bool WillProcess(ICompiledAssembly compiledAssembly) => ILPPHelper.WillProcess(compiledAssembly, RuntimeAssemblyName);
    }
}

using System;
using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Mirage.CodeGen
{
    // note: Cant create abstract ILPostProcessor base class because unity will try to process it even if it is abstract
    public static class ILPPHelper
    {
        /// <summary>
        /// Process when assembly that references <paramref name="runtimeAssemblyName"/>
        /// </summary>
        /// <param name="compiledAssembly"></param>
        /// <returns></returns>
        public static bool WillProcess(ICompiledAssembly compiledAssembly, string runtimeAssemblyName) =>
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == runtimeAssemblyName);

        /// <summary>
        /// Checks WillProcess, and then creates a Weaver instance and runs it on assembly
        /// </summary>
        /// <param name="compiledAssembly"></param>
        /// <param name="runtimeAssemblyName"></param>
        /// <param name="createWeaver"></param>
        /// <returns></returns>
        public static ILPostProcessResult CreateAndProcess(ICompiledAssembly compiledAssembly, string runtimeAssemblyName, Func<ICompiledAssembly, WeaverBase> createWeaver)
        {
            var willProcess = WillProcess(compiledAssembly, runtimeAssemblyName);
            var logText = willProcess ? "Processing" : "Skipping";
            Console.WriteLine($"[MirageILPostProcessor] {logText} {compiledAssembly.Name}");

            if (!willProcess)
                return null;

            var weaver = createWeaver.Invoke(compiledAssembly);

            var result = weaver.Process(compiledAssembly);
            Console.WriteLine($"[MirageILPostProcessor] {result.Type} {compiledAssembly.Name}");

            return result.ILPostProcessResult;
        }
    }
}

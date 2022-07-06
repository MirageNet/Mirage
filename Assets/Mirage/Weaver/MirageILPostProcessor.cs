using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Mirage.Weaver
{
    public class MirageILPostProcessor : ILPostProcessor
    {
        public const string RuntimeAssemblyName = "Mirage";

        public override ILPostProcessor GetInstance() => this;

        static void Log(string msg)
        {
            Console.WriteLine($"[MirageILPostProcessor] {msg}");
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            bool willProcess = WillProcess(compiledAssembly);
            string logText = willProcess ? "Processing" : "Skipping";
            Log($"{logText} {compiledAssembly.Name}");
            if (!willProcess)
                return null;

            var logger = new WeaverLogger();
            var weaver = new Weaver(logger);

            AssemblyDefinition assemblyDefinition = weaver.Weave(compiledAssembly);

            // write
            var pe = new MemoryStream();
            var pdb = new MemoryStream();

            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            assemblyDefinition?.Write(pe, writerParameters);

            logText = assemblyDefinition != null ? "Success" : "Failed";
            Log($"{logText} {compiledAssembly.Name}");
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), logger.Diagnostics);
        }

        /// <summary>
        /// Process when assembly that references Mirage
        /// </summary>
        /// <param name="compiledAssembly"></param>
        /// <returns></returns>
        public override bool WillProcess(ICompiledAssembly compiledAssembly) =>
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == RuntimeAssemblyName);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;


namespace Mirage.Weaver
{
    public class MirageILPostProcessor : ILPostProcessor
    {
        public const string RuntimeAssemblyName = "Mirage";

        public override ILPostProcessor GetInstance() => this;

        private static void Log(string msg)
        {
            Console.WriteLine($"[MirageILPostProcessor] {msg}");
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var willProcess = WillProcess(compiledAssembly);
            var logText = willProcess ? "Processing" : "Skipping";
            Log($"{logText} {compiledAssembly.Name}");
            if (!willProcess)
                return null;

            var enableTrace = compiledAssembly.Defines.Contains("WEAVER_DEBUG_LOGS");
            var logger = new WeaverLogger(enableTrace);
            var weaver = new Weaver(logger);

            var assemblyDefinition = weaver.Weave(compiledAssembly);

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
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), ProcessDiagnostics(logger, compiledAssembly));
        }

        private List<DiagnosticMessage> ProcessDiagnostics(WeaverLogger logger, ICompiledAssembly compiledAssembly)
        {
            var diag = logger.Diagnostics;
            var errorCount = diag.Where(x => x.DiagnosticType == DiagnosticType.Error).Count();
            if (errorCount > 0)
            {
                var defineMsg = ArrayMessage("Defines", compiledAssembly.Defines);
                var refMsg = ArrayMessage("References", compiledAssembly.References);
                var msg = $"Weaver Failed with {errorCount} errors on {compiledAssembly.Name}. See Editor log for full details.\n{defineMsg}\n{refMsg}";


                // if fail
                // insert debug info for weaver as first message,
                diag.Insert(0, new DiagnosticMessage
                {
                    DiagnosticType = DiagnosticType.Error,
                    MessageData = msg
                });
            }

            return diag;

            string ArrayMessage(string prefix, string[] array)
            {
                return array.Length == 0
                    ? $"{prefix}:[]"
                    : $"{prefix}:[\n  {string.Join("\n  ", array)}\n]";
            }
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

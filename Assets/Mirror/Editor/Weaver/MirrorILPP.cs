using UnityEngine;
using System.Collections;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirror.Weaver
{
    public class MirrorILPP : ILPostProcessor
    {
        public const string RuntimeAssemblyName = "Mirror";

        public override ILPostProcessor GetInstance() => this;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            AssemblyDefinition assemblyDefinition = Weaver.WeaveAssembly(compiledAssembly);

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

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), Weaver.Diagnostics);
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly) =>
            compiledAssembly.Name == RuntimeAssemblyName || 
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == RuntimeAssemblyName);

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Mirage.CodeGen
{
    // note WeaverBase and BaseILPostProcessor are separate classes so that WeaverBase can be used by tests without needing the BaseILPostProcessor class
    public abstract class BaseILPostProcessor : ILPostProcessor
    {
        protected abstract string RuntimeAssemblyName { get; }
        protected abstract WeaverBase Create();

        public override ILPostProcessor GetInstance() => this;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var weaver = Create();
            return weaver.Process(compiledAssembly);
        }

        /// <summary>
        /// Process when assembly that references <see cref="RuntimeAssemblyName"/>
        /// </summary>
        /// <param name="compiledAssembly"></param>
        /// <returns></returns>
        public override bool WillProcess(ICompiledAssembly compiledAssembly) =>
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == RuntimeAssemblyName);
    }

    /// <summary>
    /// Use as a base class for custom IL weaver. This class will read and write the AssemblyDefinition and return the results.
    /// </summary>
    public abstract class WeaverBase
    {
        public virtual string Name => GetType().FullName;

        public readonly IWeaverLogger logger;

        /// <summary>
        /// Results from <see cref="Process(ICompiledAssembly)"/>
        /// </summary>
        public AssemblyDefinition AssemblyDefinition { get; private set; }

        public WeaverBase(IWeaverLogger logger = null)
        {
            this.logger = logger ?? new WeaverLogger(false);
        }

        public enum Result
        {
            Success,
            NoChanges,
            Failed,
        }

        protected abstract Result Process(AssemblyDefinition assembly);

        public ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            try
            {
                AssemblyDefinition = ReadAssembly(compiledAssembly);

                var result = Process(AssemblyDefinition);

                if (result == Result.Success)
                    return Success();
                else if (result == Result.Failed)
                    return Failed();
                else // no changes, no results
                    return null;
            }
            catch (Exception e)
            {
                // this means that Weaver had problems
                // we should never get here unless there is a bug in the implementation
                // for problems with user code, use logger instead of throwing so that multiple errors can be reported to the user.

                return Error(e);
            }
        }

        private ILPostProcessResult Success()
        {
            // write assembly to file on success
            var pe = new MemoryStream();
            var pdb = new MemoryStream();

            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            AssemblyDefinition.Write(pe, writerParameters);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), logger.GetDiagnostics());
        }

        private ILPostProcessResult Failed()
        {
            return new ILPostProcessResult(null, logger.GetDiagnostics());
        }

        private ILPostProcessResult Error(Exception e)
        {
            var message = new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                MessageData = $"Weaver {Name} failed on {AssemblyDefinition?.Name} because of Exception: {e}",
            };
            return new ILPostProcessResult(null, new List<DiagnosticMessage> { message });
        }

        private static AssemblyDefinition ReadAssembly(ICompiledAssembly compiledAssembly)
        {
            var assemblyResolver = new PostProcessorAssemblyResolver(compiledAssembly);
            var readerParameters = new ReaderParameters
            {
                SymbolStream = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData),
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = assemblyResolver,
                ReflectionImporterProvider = new PostProcessorReflectionImporterProvider(),
                ReadingMode = ReadingMode.Immediate
            };

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(new MemoryStream(compiledAssembly.InMemoryAssembly.PeData), readerParameters);

            // apparently, it will happen that when we ask to resolve a type that lives inside the MyRuntime.asmdef, and we
            // are also postprocessing MyRuntime.asmdef, type resolving will fail, because we do not actually try to resolve
            // inside the assembly we are processing. Let's make sure we do that, so that we can use postprocessor features inside
            // MyRuntime.asmdef itself as well.
            assemblyResolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }
    }
}

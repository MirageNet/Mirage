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
    public enum ResultType
    {
        Success,
        NoChanges,
        Failed,
    }

    public struct Result
    {
        public ResultType Type;
        public AssemblyDefinition AssemblyDefinition;
        public ILPostProcessResult ILPostProcessResult;
    }

    /// <summary>
    /// Use as a base class for custom IL weaver. This class will read and write the AssemblyDefinition and return the results.
    /// </summary>
    public abstract class WeaverBase
    {
        public virtual string Name => GetType().FullName;

        public readonly IWeaverLogger logger;
        public readonly WeaverDiagnosticsTimer timer;

        private AssemblyDefinition _assemblyDefinition;
        /// <summary>Names of assemblies the weaved assembly is allowed to reference, see <see cref="CheckAssemblyReferences"/></summary>
        private HashSet<string> _allowedReferences;

        public WeaverBase(IWeaverLogger logger = null)
        {
            this.logger = logger ?? new WeaverLogger(false);
            timer = new WeaverDiagnosticsTimer(GetType().Name)
            {
                writeToFile = true,
            };
        }

        protected abstract ResultType Process(AssemblyDefinition assembly, ICompiledAssembly compiledAssembly);

        public Result Process(ICompiledAssembly compiledAssembly)
        {
            try
            {
                timer.Start(compiledAssembly.Name);

                using (timer.Sample("AssemblyDefinitionFor"))
                {
                    _assemblyDefinition = ReadAssembly(compiledAssembly);
                }
                _allowedReferences = GetAllowedReferences(compiledAssembly, _assemblyDefinition);

                var result = Process(_assemblyDefinition, compiledAssembly);
                var ilResult = CreateResults(result);
                if (ilResult?.Diagnostics != null)
                    InsertDebugIfErrors(ilResult.Diagnostics, compiledAssembly);

                return new Result
                {
                    Type = result,
                    AssemblyDefinition = _assemblyDefinition,
                    ILPostProcessResult = ilResult
                };
            }
            catch (Exception e)
            {
                // this means that Weaver had problems
                // we should never get here unless there is a bug in the implementation
                // for problems with user code, use logger instead of throwing so that multiple errors can be reported to the user.

                return new Result
                {
                    Type = ResultType.Failed,
                    ILPostProcessResult = Error(e)
                };
            }
            finally
            {
                // end in finally incase it return early
                timer?.End();
            }
        }

        private ILPostProcessResult CreateResults(ResultType result)
        {
            if (result == ResultType.Success)
                return Success();
            else if (result == ResultType.Failed)
                return Failed();
            else // no changes, no results
                return null;
        }

        private ILPostProcessResult Success()
        {
            if (!CheckAssemblyReferences())
                return Failed();

            // write assembly to file on success
            var pe = new MemoryStream();
            var pdb = new MemoryStream();

            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            _assemblyDefinition.Write(pe, writerParameters);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), logger.GetDiagnostics());
        }

        private ILPostProcessResult Failed()
        {
            return new ILPostProcessResult(null, logger.GetDiagnostics());
        }

        /// <summary>
        /// Weaver can add references that the compiler didn't need (eg UnityEditor for InitializeOnLoadMethod),
        /// but only to assemblies this one was compiled against, or that it already referenced
        /// </summary>
        private static HashSet<string> GetAllowedReferences(ICompiledAssembly compiledAssembly, AssemblyDefinition assemblyDefinition)
        {
            var allowed = new HashSet<string>();
            foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences)
                allowed.Add(reference.Name);

            foreach (var path in compiledAssembly.References)
                allowed.Add(Path.GetFileNameWithoutExtension(path));

            return allowed;
        }

        /// <summary>
        /// Errors if weaving added a reference outside of <see cref="_allowedReferences"/>.
        /// <para>
        /// A bad import (eg the weaver's own System.Private.CoreLib, or this assembly referencing itself) still writes fine and
        /// works until the editor restarts, then unity refuses to load the assembly. Failing here points at the weaver instead
        /// </para>
        /// </summary>
        private bool CheckAssemblyReferences()
        {
            var valid = true;
            foreach (var reference in _assemblyDefinition.MainModule.AssemblyReferences)
            {
                if (_allowedReferences.Contains(reference.Name))
                    continue;

                logger.Error($"Weaver added reference to '{reference.FullName}' which {_assemblyDefinition.Name.Name} was not compiled against. Unity will not load the assembly after an editor restart, this is a bug in the weaver");
                valid = false;
            }
            return valid;
        }

        private void InsertDebugIfErrors(List<DiagnosticMessage> diag, ICompiledAssembly compiledAssembly)
        {
            var errorCount = diag.Where(x => x.DiagnosticType == DiagnosticType.Error).Count();
            if (errorCount == 0)
                return;

            var defineMsg = ArrayMessage("Defines", compiledAssembly.Defines);
            var refMsg = ArrayMessage("References", compiledAssembly.References);
            var msg = $"Weaver Failed with {errorCount} errors on {compiledAssembly.Name}. See Editor log for full details.\n{defineMsg}\n{refMsg}";

            // insert debug info for weaver as first message,
            diag.Insert(0, new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                MessageData = msg
            });
        }

        private static string ArrayMessage(string prefix, string[] array)
        {
            return array.Length == 0
                ? $"{prefix}:[]"
                : $"{prefix}:[\n  {string.Join("\n  ", array)}\n]";
        }

        private ILPostProcessResult Error(Exception e)
        {
            var message = new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                MessageData = $"Weaver {Name} failed on {_assemblyDefinition?.Name} because of Exception: {e}",
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

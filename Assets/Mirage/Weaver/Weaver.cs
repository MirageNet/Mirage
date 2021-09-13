using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mirage.Weaver
{
    internal class FoundType
    {
        public readonly TypeDefinition Definition;
        public readonly bool IsNetworkBehaviour;
        public readonly bool IsMonoBehaviour;

        public FoundType(TypeDefinition definition, bool isNested)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            // dont need to check if NB/MB if type is nested
            if (definition.IsClass && !isNested)
            {
                // is td a NetworkBehaviour or MonoBehaviour
                TypeReference parent = definition.BaseType;
                while (parent != null)
                {
                    if (parent.Is<NetworkBehaviour>())
                    {
                        IsNetworkBehaviour = true;
                        IsMonoBehaviour = true;
                        break;
                    }
                    else if (parent.Is<UnityEngine.MonoBehaviour>())
                    {
                        IsMonoBehaviour = true;
                        break;
                    }

                    if (parent.CanBeResolved())
                        parent = parent.Resolve().BaseType;
                }
            }
        }
    }
    
    /// <summary>
    /// Weaves an Assembly
    /// <para>
    /// Debug Defines:<br />
    /// - <c>WEAVER_DEBUG_LOGS</c><br />
    /// - <c>WEAVER_DEBUG_TIMER</c><br />
    /// </para>
    /// </summary>
    public class Weaver
    {
        private readonly IWeaverLogger logger;
        private Readers readers;
        private Writers writers;
        private PropertySiteProcessor propertySiteProcessor;
        private WeaverDiagnosticsTimer timer;
        private ModuleImportCache moduleCache;

        private AssemblyDefinition CurrentAssembly { get; set; }

        [Conditional("WEAVER_DEBUG_LOGS")]
        public static void DebugLog(TypeDefinition td, string message)
        {
            Console.WriteLine($"Weaver[{td.Name}]{message}");
        }

        public Weaver(IWeaverLogger logger)
        {
            this.logger = logger;
        }

        public static AssemblyDefinition AssemblyDefinitionFor(ICompiledAssembly compiledAssembly)
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

            //apparently, it will happen that when we ask to resolve a type that lives inside MLAPI.Runtime, and we
            //are also postprocessing MLAPI.Runtime, type resolving will fail, because we do not actually try to resolve
            //inside the assembly we are processing. Let's make sure we do that, so that we can use postprocessor features inside
            //MLAPI.Runtime itself as well.
            assemblyResolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition);

            return assemblyDefinition;
        }


        public AssemblyDefinition Weave(ICompiledAssembly compiledAssembly)
        {
            try
            {
                timer = new WeaverDiagnosticsTimer() { writeToFile = true };
                timer.Start(compiledAssembly.Name);

                using (timer.Sample("AssemblyDefinitionFor"))
                {
                    CurrentAssembly = AssemblyDefinitionFor(compiledAssembly);
                }

                ModuleDefinition module = CurrentAssembly.MainModule;
                moduleCache = new ModuleImportCache(module);
                readers = new Readers(moduleCache, logger);
                writers = new Writers(moduleCache, logger);
                propertySiteProcessor = new PropertySiteProcessor();
                var rwProcessor = new ReaderWriterProcessor(moduleCache, readers, writers, logger);

                bool modified = false;
                using (timer.Sample("ProcessExtensionsAndMessages"))
                {
                    modified |= rwProcessor.ProcessExtensionsAndMessages();
                }
                using (timer.Sample("CheckAllInstructionsForGenericCalls"))
                {
                    modified |= rwProcessor.CheckAllInstructionsForGenericCalls();
                }


                List<FoundType> foundTypes = FindAllTypes(module);
                using (timer.Sample("AttributeProcessor"))
                {
                    var attributeProcessor = new AttributeProcessor(moduleCache, logger);
                    modified |= attributeProcessor.ProcessTypes(foundTypes);
                }

                using (timer.Sample("WeaveNetworkBehavior"))
                {
                    foreach (FoundType type in foundTypes)
                    {
                        if (type.IsNetworkBehaviour)
                        {
                            modified |= WeaveNetworkBehaviour(type.Definition);
                        }
                    }
                }

                using (timer.Sample("propertySiteProcessor"))
                {
                    if (modified)
                        propertySiteProcessor.Process(module);
                }

                if (!modified)
                    return CurrentAssembly;

                using (timer.Sample("InitializeReaderAndWriters"))
                {
                    rwProcessor.InitializeReaderAndWriters();
                }

                return CurrentAssembly;
            }
            catch (Exception e)
            {
                logger.Error("Exception :" + e);
                return null;
            }
            finally
            {
                // end in finally incase it return early
                long endTime = timer?.End() ?? 0;

                moduleCache?.Close(endTime);
            }
        }

        List<FoundType> FindAllTypes(ModuleDefinition module)
        {
            using (timer.Sample("GetAllTypes"))
            {
                var allTypes = new List<FoundType>();
                AddTypes(module.Types, false, allTypes);
                return allTypes;
            }

            void AddTypes(Collection<TypeDefinition> types, bool nested, List<FoundType> allTypes)
            {
                foreach (TypeDefinition td in types)
                {
                    allTypes.Add(new FoundType(td, nested));

                    AddTypes(td.NestedTypes, true, allTypes);
                }
            }
        }

        bool WeaveNetworkBehaviour(TypeDefinition td)
        {
            // process this and base classes from parent to child order

            var behaviourClasses = new List<TypeDefinition>();

            TypeDefinition parent = td;
            while (parent != null)
            {
                if (parent.Is<NetworkBehaviour>())
                {
                    break;
                }

                try
                {
                    behaviourClasses.Insert(0, parent);
                    parent = parent.BaseType.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    // this can happen for plugins.
                    break;
                }
            }

            bool modified = false;
            foreach (TypeDefinition behaviour in behaviourClasses)
            {
                modified |= new NetworkBehaviourProcessor(moduleCache, behaviour, readers, writers, propertySiteProcessor, logger).Process();
            }
            return modified;
        }
    }

    class WeaverDiagnosticsTimer
    {
        public bool writeToFile;
        StreamWriter writer;
        Stopwatch stopwatch;
        private string name;

        public long ElapsedMilliseconds => stopwatch?.ElapsedMilliseconds ?? 0;

        ~WeaverDiagnosticsTimer()
        {
            writer?.Dispose();
            writer = null;
        }

        [Conditional("WEAVER_DEBUG_TIMER")]
        public void Start(string name)
        {
            this.name = name;

            if (writeToFile)
            {
                string path = $"./Build/WeaverLogs/Timer_{name}.log";
                try
                {
                    writer = new StreamWriter(path)
                    {
                        AutoFlush = true,
                    };
                }
                catch (Exception e)
                {
                    writer?.Dispose();
                    writeToFile = false;
                    WriteLine($"Failed to open {path}: {e}");
                }
            }

            stopwatch = Stopwatch.StartNew();

            WriteLine($"Weave Started - {name}");
#if WEAVER_DEBUG_LOGS
            WriteLine($"Debug logs enabled");
#else
            WriteLine($"Debug logs disabled");
#endif 
        }

        [Conditional("WEAVER_DEBUG_TIMER")]
        void WriteLine(string msg)
        {
            string fullMsg = $"[WeaverDiagnostics] {msg}";
            Console.WriteLine(fullMsg);
            if (writeToFile)
            {
                writer.WriteLine(fullMsg);
            }
        }

        public long End()
        {
            WriteLine($"Weave Finished: {ElapsedMilliseconds}ms - {name}");
            stopwatch?.Stop();
            writer?.Close();
            return ElapsedMilliseconds;
        }

        public SampleScope Sample(string label)
        {
            return new SampleScope(this, label);
        }

        public struct SampleScope : IDisposable
        {
            readonly WeaverDiagnosticsTimer timer;
            readonly long start;
            readonly string label;

            public SampleScope(WeaverDiagnosticsTimer timer, string label)
            {
                this.timer = timer;
                start = timer.ElapsedMilliseconds;
                this.label = label;
            }

            public void Dispose()
            {
                timer.WriteLine($"{label}: {timer.ElapsedMilliseconds - start}ms - {timer.name}");
            }
        }
    }
}

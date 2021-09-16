using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEngine;
using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;

namespace Mirage.Weaver
{
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
                readers = new Readers(module, logger);
                writers = new Writers(module, logger);
                propertySiteProcessor = new PropertySiteProcessor();
                var rwProcessor = new ReaderWriterProcessor(module, readers, writers);

                bool modified = false;
                using (timer.Sample("ReaderWriterProcessor"))
                {
                    modified = rwProcessor.Process();
                }

                IReadOnlyList<FoundType> foundTypes = FindAllClasses(module);

                using (timer.Sample("AttributeProcessor"))
                {
                    var attributeProcessor = new AttributeProcessor(module, logger);
                    modified |= attributeProcessor.ProcessTypes(foundTypes);
                }

                using (timer.Sample("WeaveNetworkBehavior"))
                {
                    foreach (FoundType foundType in foundTypes)
                    {
                        if (foundType.IsNetworkBehaviour)
                            modified |= WeaveNetworkBehavior(foundType);
                    }
                }


                if (modified)
                {
                    using (timer.Sample("propertySiteProcessor"))
                    {
                        propertySiteProcessor.Process(module);
                    }

                    using (timer.Sample("InitializeReaderAndWriters"))
                    {
                        rwProcessor.InitializeReaderAndWriters();
                    }
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
                timer?.End();
            }
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

        IReadOnlyList<FoundType> FindAllClasses(ModuleDefinition module)
        {
            using (timer.Sample("FindAllClasses"))
            {
                var foundTypes = new List<FoundType>();
                foreach (TypeDefinition type in module.Types)
                {
                    ProcessType(type, foundTypes);

                    foreach (TypeDefinition nested in type.NestedTypes)
                    {
                        ProcessType(nested, foundTypes);
                    }
                }

                return foundTypes;
            }
        }

        private void ProcessType(TypeDefinition type, List<FoundType> foundTypes)
        {
            if (!type.IsClass) return;

            TypeReference parent = type.BaseType;
            bool isNetworkBehaviour = false;
            bool isMonoBehaviour = false;
            while (parent != null)
            {
                if (parent.Is<NetworkBehaviour>())
                {
                    isNetworkBehaviour = true;
                    isMonoBehaviour = true;
                    break;
                }
                if (parent.Is<MonoBehaviour>())
                {
                    isMonoBehaviour = true;
                    break;
                }

                parent = parent.TryResolveParent();
            }

            foundTypes.Add(new FoundType(type, isNetworkBehaviour, isMonoBehaviour));
        }

        bool WeaveNetworkBehavior(FoundType foundType)
        {
            List<TypeDefinition> behaviourClasses = FindAllBaseTypes(foundType);

            bool modified = false;
            // process this and base classes from parent to child order
            for (int i = behaviourClasses.Count - 1; i >= 0; i--)
            {
                TypeDefinition behaviour = behaviourClasses[i];
                if (NetworkBehaviourProcessor.WasProcessed(behaviour)) { continue; }

                modified |= new NetworkBehaviourProcessor(behaviour, readers, writers, propertySiteProcessor, logger).Process();
            }
            return modified;
        }

        /// <summary>
        /// Returns all base types that are between the type and NetworkBehaviour
        /// </summary>
        /// <param name="foundType"></param>
        /// <returns></returns>
        static List<TypeDefinition> FindAllBaseTypes(FoundType foundType)
        {
            var behaviourClasses = new List<TypeDefinition>();

            TypeDefinition type = foundType.TypeDefinition;
            while (type != null)
            {
                if (type.Is<NetworkBehaviour>())
                {
                    break;
                }

                behaviourClasses.Add(type);
                type = type.BaseType.TryResolve();
            }

            return behaviourClasses;
        }
    }

    public class FoundType
    {
        public readonly TypeDefinition TypeDefinition;

        /// <summary>
        /// Is Derived From NetworkBehaviour
        /// </summary>
        public readonly bool IsNetworkBehaviour;

        public readonly bool IsMonoBehaviour;

        public FoundType(TypeDefinition typeDefinition, bool isNetworkBehaviour, bool isMonoBehaviour)
        {
            TypeDefinition = typeDefinition;
            IsNetworkBehaviour = isNetworkBehaviour;
            IsMonoBehaviour = isMonoBehaviour;
        }

        public override string ToString()
        {
            return TypeDefinition.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using Mirage.CodeGen;
using Mono.Cecil;
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
    public class Weaver : WeaverBase
    {
        private Readers readers;
        private Writers writers;
        private PropertySiteProcessor propertySiteProcessor;

        [Conditional("WEAVER_DEBUG_LOGS")]
        public static void DebugLog(TypeDefinition td, string message)
        {
            Console.WriteLine($"Weaver[{td.Name}] {message}");
        }

        private static void Log(string msg)
        {
            Console.WriteLine($"[Weaver] {msg}");
        }

        public Weaver(IWeaverLogger logger) : base(logger) { }

        protected override ResultType Process(AssemblyDefinition assembly, ICompiledAssembly compiledAssembly)
        {
            Log($"Starting weaver on {compiledAssembly.Name}");
            try
            {
                var module = assembly.MainModule;
                readers = new Readers(module, logger);
                writers = new Writers(module, logger);
                propertySiteProcessor = new PropertySiteProcessor();
                var rwProcessor = new ReaderWriterProcessor(module, readers, writers, logger);

                var modified = false;
                using (timer.Sample("ReaderWriterProcessor"))
                {
                    modified = rwProcessor.Process();
                }

                var foundTypes = FindAllClasses(module);

                using (timer.Sample("AttributeProcessor"))
                {
                    var attributeProcessor = new AttributeProcessor(module, logger);
                    modified |= attributeProcessor.ProcessTypes(foundTypes);
                }

                using (timer.Sample("WeaveNetworkBehavior"))
                {
                    foreach (var foundType in foundTypes)
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

                return ResultType.Success;
            }
            catch (Exception e)
            {
                logger.Error("Exception :" + e);
                // write line too because the error about doesn't show stacktrace
                Console.WriteLine("[WeaverException] :" + e);
                return ResultType.Failed;
            }
            finally
            {
                Log($"Finished weaver on {compiledAssembly.Name}");
            }
        }

        private IReadOnlyList<FoundType> FindAllClasses(ModuleDefinition module)
        {
            using (timer.Sample("FindAllClasses"))
            {
                var foundTypes = new List<FoundType>();
                foreach (var type in module.Types)
                {
                    ProcessType(type, foundTypes);

                    foreach (var nested in type.NestedTypes)
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

            var parent = type.BaseType;
            var isNetworkBehaviour = false;
            var isMonoBehaviour = false;
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

        private bool WeaveNetworkBehavior(FoundType foundType)
        {
            var behaviourClasses = FindAllBaseTypes(foundType);

            var modified = false;
            // process this and base classes from parent to child order
            for (var i = behaviourClasses.Count - 1; i >= 0; i--)
            {
                var behaviour = behaviourClasses[i];
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
        private static List<TypeDefinition> FindAllBaseTypes(FoundType foundType)
        {
            var behaviourClasses = new List<TypeDefinition>();

            var type = foundType.TypeDefinition;
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

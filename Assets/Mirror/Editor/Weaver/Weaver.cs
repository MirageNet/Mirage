using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor.Compilation;

namespace Mirror.Weaver
{
    // This data is flushed each time - if we are run multiple times in the same process/domain
    class WeaverLists
    {
        // setter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldDefinition, MethodDefinition> replacementSetterProperties = new Dictionary<FieldDefinition, MethodDefinition>();
        // getter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldDefinition, MethodDefinition> replacementGetterProperties = new Dictionary<FieldDefinition, MethodDefinition>();

        // amount of SyncVars per class. dict<className, amount>
        public Dictionary<string, int> numSyncVars = new Dictionary<string, int>();

        public int GetSyncVarStart(string className)
        {
            return numSyncVars.ContainsKey(className)
                   ? numSyncVars[className]
                   : 0;
        }

        public void SetNumSyncVars(string className, int num)
        {
            numSyncVars[className] = num;
        }
    }

    internal static class Weaver
    {
        public static WeaverLists WeaveLists { get; private set; }
        public static AssemblyDefinition CurrentAssembly { get; private set; }

        public readonly static List<DiagnosticMessage> Diagnostics = new List<DiagnosticMessage>();

        public static void DLog(TypeDefinition td, string fmt, params object[] args)
        {
            Console.WriteLine("[" + td.Name + "] " + string.Format(fmt, args));
        }

        // display weaver error
        // and mark process as failed
        public static void Error(string message)
        {
            AddError(null, message);
        }

        public static void Error(string message, MethodDefinition methodDefinition)
        {
            AddError(methodDefinition.DebugInformation.SequencePoints.FirstOrDefault(), message);
        }

        // display weaver error
        // and mark process as failed
        public static void Error(string message, MemberReference mr)
        {
            AddError(null, $"{message} (at {mr})");
        }

        public static void AddError(SequencePoint sequencePoint, string message)
        {
            Diagnostics.Add(new DiagnosticMessage
            {
                DiagnosticType = DiagnosticType.Error,
                File = sequencePoint?.Document.Url.Replace($"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}", ""),
                Line = sequencePoint?.StartLine ?? 0,
                Column = sequencePoint?.StartColumn ?? 0,
                MessageData = message
            });
        }

        public static void Warning(string message, MemberReference mr)
        {
            Log.Warning($"{message} (at {mr})");
        }

        static void CheckMonoBehaviour(TypeDefinition td)
        {
            if (td.IsDerivedFrom<UnityEngine.MonoBehaviour>())
            {
                MonoBehaviourProcessor.Process(td);
            }
        }

        static bool WeaveNetworkBehavior(TypeDefinition td)
        {
            if (!td.IsClass)
                return false;

            if (!td.IsDerivedFrom<NetworkBehaviour>())
            {
                CheckMonoBehaviour(td);
                return false;
            }

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
                modified |= new NetworkBehaviourProcessor(behaviour).Process();
            }
            return modified;
        }

        static bool WeaveModule(ModuleDefinition moduleDefinition)
        {
            try
            {
                bool modified = false;

                var watch = System.Diagnostics.Stopwatch.StartNew();

                watch.Start();
                var types = new List<TypeDefinition>(moduleDefinition.Types);

                foreach (TypeDefinition td in types)
                {
                    if (td.IsClass && td.BaseType.CanBeResolved())
                    {
                        modified |= WeaveNetworkBehavior(td);
                        modified |= ServerClientAttributeProcessor.Process(td);
                    }
                }
                watch.Stop();
                Console.WriteLine("Weave behaviours and messages took" + watch.ElapsedMilliseconds + " milliseconds");

                if (modified)
                    PropertySiteProcessor.Process(moduleDefinition);

                return modified;
            }
            catch (Exception ex)
            {
                Error(ex.ToString());
                throw;
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

        static AssemblyDefinition Weave(ICompiledAssembly unityAssembly)
        {
            CurrentAssembly = AssemblyDefinitionFor(unityAssembly);
            
            ModuleDefinition module = CurrentAssembly.MainModule;

            var rwstopwatch = System.Diagnostics.Stopwatch.StartNew();

            var processor = new ReaderWriterProcessor();

            bool modified = processor.Process(module);
            rwstopwatch.Stop();
            Console.WriteLine($"Find all reader and writers took {rwstopwatch.ElapsedMilliseconds} milliseconds");

            Console.WriteLine($"Script Module: {module.Name}");

            modified |= WeaveModule(module);

            if (!modified)
                return null;

            return CurrentAssembly;
        }

        private static void AddPaths(DefaultAssemblyResolver asmResolver, Assembly assembly)
        {
            foreach (string path in assembly.allReferences)
            {
                asmResolver.AddSearchDirectory(Path.GetDirectoryName(path));
            }
        }

        public static AssemblyDefinition WeaveAssembly(ICompiledAssembly assembly)
        {
            WeaveLists = new WeaverLists();
            Diagnostics.Clear();

            try
            {
                return Weave(assembly);
            }
            catch (Exception e)
            {
                Error("Exception :" + e);
                return null;
            }
        }
    }
}

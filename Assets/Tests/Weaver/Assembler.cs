using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor.Compilation;
using UnityEngine;

namespace Mirage.Tests.Weaver
{
    public class CompiledAssembly : ICompiledAssembly
    {
        private readonly string assemblyPath;
        private InMemoryAssembly inMemoryAssembly;

        public CompiledAssembly(string assemblyPath)
        {
            this.assemblyPath = assemblyPath;
        }

        public InMemoryAssembly InMemoryAssembly
        {
            get
            {

                if (inMemoryAssembly == null)
                {
                    byte[] peData = File.ReadAllBytes(assemblyPath);

                    string pdbFileName = Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb";

                    byte[] pdbData = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(assemblyPath), pdbFileName));

                    inMemoryAssembly = new InMemoryAssembly(peData, pdbData);
                }
                return inMemoryAssembly;
            }
        }

        public string Name => Path.GetFileNameWithoutExtension(assemblyPath);

        public string[] References { get; set; }

        public string[] Defines { get; set; }
    }

    public class WeaverMessages
    {
        public readonly CompilerMessageType type;
        public readonly string message;

        public WeaverMessages(CompilerMessageType type, string message)
        {
            this.type = type;
            this.message = message;
        }
    }

    public class Assembler
    {
        public string OutputFile { get; set; }
        public string ProjectPathFile => Path.Combine(WeaverTestLocator.OutputDirectory, OutputFile);
        public List<CompilerMessage> CompilerMessages { get; private set; }
        public bool CompilerErrors { get; private set; }

        readonly HashSet<string> sourceFiles = new HashSet<string>();
        private List<WeaverMessages> messages;
        private AssemblyBuilder assemblyBuilder;
        private AssemblyDefinition assembly;

        public Assembler()
        {
            CompilerMessages = new List<CompilerMessage>();
        }

        public void AddSourceFile(string sourceFile)
        {
            sourceFiles.Add(Path.Combine(WeaverTestLocator.OutputDirectory, sourceFile));
        }
        public void AddSourceFiles(string[] sourceFiles)
        {
            foreach (string src in sourceFiles)
            {
                this.sourceFiles.Add(Path.Combine(WeaverTestLocator.OutputDirectory, src));
            }
        }

        // Delete output dll / pdb / mdb
        public void DeleteOutput()
        {
            // "x.dll" shortest possible dll name
            if (OutputFile.Length < 5)
            {
                return;
            }

            try
            {
                File.Delete(ProjectPathFile);
            }
            catch { /* Do Nothing */ }

            try
            {
                File.Delete(Path.ChangeExtension(ProjectPathFile, ".pdb"));
            }
            catch { /* Do Nothing */ }

            try
            {
                File.Delete(Path.ChangeExtension(ProjectPathFile, ".dll.mdb"));
            }
            catch { /* Do Nothing */ }
        }

        public AssemblyDefinition Build(List<WeaverMessages> messages)
        {
            // the way things are build changes in 2020
#if UNITY_2020_3_OR_NEWER
            // in 2020 assemblyBuilder automaitcally calls ILPP
            return Build2020(messages);
#else
            // in 2019 assemblyBuilder does not call ILPP
            // this means we have to check for compile errors and then run weaver manually
            return Build2019(logger);
#endif
        }

#if UNITY_2020_3_OR_NEWER
        /// <summary>
        /// Builds and Weaves an Assembly with references to unity engine and other asmdefs.
        /// <para>
        ///     NOTE: Does not write the weaved assemble to disk
        /// </para>
        /// </summary>
        public AssemblyDefinition Build2020(List<WeaverMessages> messages)
        {
            this.messages = messages;

            // This will compile scripts with the same references as files in the asset folder.
            // This means that the dll will get references to all asmdef just as if it was the default "Assembly-CSharp.dll"
            assemblyBuilder = new AssemblyBuilder(ProjectPathFile, sourceFiles.ToArray())
            {
                referencesOptions = ReferencesOptions.UseEngineModules
            };

            assemblyBuilder.buildFinished += BuildFinished2020;

            Console.WriteLine($"[WeaverTests] Build {ProjectPathFile}");
            // Start build of assembly
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}", assemblyBuilder.assemblyPath);
                return assembly;
            }

            while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
            {
                System.Threading.Thread.Sleep(10);
            }

            return assembly;
        }

        void BuildFinished2020(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            // check only compiler errors are from weaver
            // if they are not inside logger, the are c# errors in test

            Console.WriteLine($"[WeaverTests] buildFinished {assemblyPath}");
            foreach (CompilerMessage item in compilerMessages)
            {
                DebugLog(item);

                // the ILPP runs by itself so we dont have access to its logger, so instead we copy the compiler message into the test logger
                const string ERROR_TAG = "): error ";
                const string WARNING_TAG = "): warning ";

                string trim;
                switch (item.type)
                {
                    case CompilerMessageType.Error:
                        trim = ERROR_TAG;
                        break;
                    case CompilerMessageType.Warning:
                        trim = WARNING_TAG;
                        break;
                    default:
                        trim = "";
                        break;
                }

                string realMessage = item.message.Substring(item.message.IndexOf(trim) + trim.Length);
                messages.Add(new WeaverMessages(item.type, realMessage));
            }

            void DebugLog(CompilerMessage item)
            {
                switch (item.type)
                {
                    case CompilerMessageType.Error:
                        Console.WriteLine($"[WeaverTests:Error] {item.message}");
                        break;
                    case CompilerMessageType.Warning:
                        Console.WriteLine($"[WeaverTests:Warn] {item.message}");
                        break;
                    default:
                        break;
                }
            }
        }
#else
            /// <summary>
            /// Builds and Weaves an Assembly with references to unity engine and other asmdefs.
            /// <para>
            ///     NOTE: Does not write the weaved assemble to disk
            /// </para>
            /// </summary>
            public AssemblyDefinition Build2019(IWeaverLogger logger)
            {
                this.logger = logger;
                assembly = null;

                // This will compile scripts with the same references as files in the asset folder.
                // This means that the dll will get references to all asmdef just as if it was the default "Assembly-CSharp.dll"
                assemblyBuilder = new AssemblyBuilder(ProjectPathFile, sourceFiles.ToArray())
                {
                    referencesOptions = ReferencesOptions.UseEngineModules
                };

                assemblyBuilder.buildFinished += BuildFinished2019;

                Console.WriteLine($"[WeaverTests] Build {ProjectPathFile}");
                // Start build of assembly
                if (!assemblyBuilder.Build())
                {
                    Debug.LogErrorFormat("Failed to start build of assembly {0}", assemblyBuilder.assemblyPath);
                    return assembly;
                }

                while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
                {
                    System.Threading.Thread.Sleep(10);
                }

                return assembly;
            }

            void BuildFinished2019(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                Console.WriteLine($"[WeaverTests] buildFinished {assemblyPath}");
                foreach (CompilerMessage item in compilerMessages)
                {
                    switch (item.type)
                    {
                        case CompilerMessageType.Error:
                            Console.WriteLine($"[WeaverTests:Error] {assemblyPath}");
                            break;
                        case CompilerMessageType.Warning:
                            Console.WriteLine($"[WeaverTests:Warn] {assemblyPath}");
                            break;
                        default:
                            break;
                    }
                }
#if !UNITY_2020_2_OR_NEWER
                    CompilerMessages.AddRange(compilerMessages);
                    foreach (CompilerMessage cm in compilerMessages)
                    {
                        if (cm.type == CompilerMessageType.Error)
                        {
                            Debug.LogErrorFormat("{0}:{1} -- {2}", cm.file, cm.line, cm.message);
                            CompilerErrors = true;
                        }
                    }
#endif

            // assembly builder does not call ILPostProcessor (WTF Unity?),  so we must invoke it ourselves.
            var compiledAssembly = new CompiledAssembly(assemblyPath)
            {
                Defines = assemblyBuilder.defaultDefines,
                References = assemblyBuilder.defaultReferences
            };

            var weaver = new Mirage.Weaver.Weaver(logger);

            assembly = weaver.Weave(compiledAssembly);
        }
#endif

    }
}

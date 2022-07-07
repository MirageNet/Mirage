using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mirage.Weaver;
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

        public CompiledAssembly(string assemblyPath, AssemblyBuilder assemblyBuilder)
        {
            this.assemblyPath = assemblyPath;
            Defines = assemblyBuilder.defaultDefines;
            References = assemblyBuilder.defaultReferences;
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

    public class Assembler
    {
        public string OutputFile { get; }
        public string ProjectPathFile { get; }

        public List<CompilerMessage> CompilerMessages { get; } = new List<CompilerMessage>();
        public bool CompilerErrors { get; private set; }

        readonly HashSet<string> sourceFiles = new HashSet<string>();

        AssemblyDefinition builtAssembly = null;
        IWeaverLogger logger;
        AssemblyBuilder builder;

        public Assembler(string outputFile, string[] sourceFiles)
        {
            OutputFile = outputFile;
            string dir = WeaverTestLocator.GetOutputDirectory();
            ProjectPathFile = Path.Combine(dir, OutputFile);

            foreach (string src in sourceFiles)
            {
                this.sourceFiles.Add(Path.Combine(WeaverTestLocator.SourceDirectory, src));
            }
        }

        static void Log(string msg)
        {
            Console.WriteLine($"[WeaverTest] {msg}");
        }
        static void LogWarning(string msg)
        {
            Console.WriteLine($"[WeaverTest] Warning: {msg}");

            // do debug log too if we are not running in batch mode
            if (!Application.isBatchMode)
                Debug.LogWarning(msg);
        }

        /// <summary>
        /// Builds and Weaves an Assembly with references to unity engine and other asmdefs.
        /// <para>
        ///     NOTE: Does not write the weaved assemble to disk
        /// </para>
        /// </summary>
        public async Task<AssemblyDefinition> BuildAsync(IWeaverLogger logger)
        {
            Log($"Assembler.Build for {OutputFile}");

            this.logger = logger;
            // This will compile scripts with the same references as files in the asset folder.
            // This means that the dll will get references to all asmdef just as if it was the default "Assembly-CSharp.dll"
            builder = new AssemblyBuilder(ProjectPathFile, sourceFiles.ToArray())
            {
                referencesOptions = ReferencesOptions.UseEngineModules,
            };

            builder.buildFinished += buildFinished;

            bool started = builder.Build();
            // Start build of assembly
            if (!started)
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}", builder.assemblyPath);
                return builtAssembly;
            }

            while (builder.status != AssemblyBuilderStatus.Finished)
            {
                await Task.Yield();
            }

            return builtAssembly;
        }

        void buildFinished(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            Log($"buildFinished for {OutputFile}");

            // in unity2020, ILPP runs automatically. but in unity2021 it does not
#if !UNITY_2020_2_OR_NEWER || UNITY_2021_3_OR_NEWER
            CompilerMessages.AddRange(compilerMessages);
            foreach (CompilerMessage m in compilerMessages)
            {
                if (m.type == CompilerMessageType.Error)
                {
                    // first error
                    if (!CompilerErrors)
                        LogWarning($"Batch failed!!!");

                    CompilerErrors = true;
                    LogWarning($"{m.file}:{m.line} -- {m.message}");
                }
            }

            // we can't run weaver if there are compile errors
            if (CompilerErrors)
                return;
#endif

            // call weaver on result
            var compiledAssembly = new CompiledAssembly(assemblyPath, builder);

            Log($"Starting weaver on {OutputFile}");
            var weaver = new Mirage.Weaver.Weaver(logger);
            builtAssembly = weaver.Weave(compiledAssembly);
            Log($"Finished weaver on {OutputFile}");

            // NOTE: we need to write to check for ArgumentException from writing
            TryWriteAssembly(builtAssembly);
        }

        private void TryWriteAssembly(AssemblyDefinition assembly)
        {
            // fine to be given null here, means that weaver didn't finish 
            if (assembly == null)
                return;

            try
            {
                string file = $"./temp/WeaverTests/{assembly.Name}.dll";
                string dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                assembly.Write(file);
            }
            catch (Exception e)
            {
                Log($"Exception on {OutputFile}. {e}");
                throw;
            }
        }

        /// <summary>
        /// Delete output dll / pdb / mdb
        /// </summary>
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

    }
}

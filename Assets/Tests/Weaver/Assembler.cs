using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Mirage.Weaver
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

    public class Assembler : ScriptableObject
    {
        string _outputDirectory;
        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_outputDirectory))
                {
                    ScriptableObject assemblerObj = CreateInstance<Assembler>();

                    var monoScript = MonoScript.FromScriptableObject(assemblerObj);
                    string myPath = AssetDatabase.GetAssetPath(monoScript);
                    _outputDirectory = Path.GetDirectoryName(myPath);
                }
                return _outputDirectory;
            }
        }
        public string OutputFile { get; set; }
        public List<CompilerMessage> CompilerMessages { get; private set; }
        public bool CompilerErrors { get; private set; }

        readonly HashSet<string> sourceFiles = new HashSet<string>();

        public Assembler()
        {
            CompilerMessages = new List<CompilerMessage>();
        }

        // Add a range of source files to compile
        public void AddSourceFiles(string[] sourceFiles)
        {
            foreach (string src in sourceFiles)
            {
                this.sourceFiles.Add(Path.Combine(OutputDirectory, src));
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

            string projPathFile = Path.Combine(OutputDirectory, OutputFile);

            try
            {
                File.Delete(projPathFile);
            }
            catch { /* Do Nothing */ }

            try
            {
                File.Delete(Path.ChangeExtension(projPathFile, ".pdb"));
            }
            catch { /* Do Nothing */ }

            try
            {
                File.Delete(Path.ChangeExtension(projPathFile, ".dll.mdb"));
            }
            catch { /* Do Nothing */ }
        }

        // clear all settings except for referenced assemblies (which are cleared with ClearReferences)
        public void Clear(bool deleteOutputOnClear)
        {
            if (deleteOutputOnClear)
            {
                DeleteOutput();
            }

            CompilerErrors = false;
            OutputFile = "";
            sourceFiles.Clear();
            CompilerMessages.Clear();
        }

        public AssemblyDefinition Build(IWeaverLogger logger)
        {
            AssemblyDefinition assembly = null;

            var assemblyBuilder = new AssemblyBuilder(Path.Combine(OutputDirectory, OutputFile), sourceFiles.ToArray())
            {
                referencesOptions = ReferencesOptions.UseEngineModules
            };

            assemblyBuilder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
            {
                CompilerMessages.AddRange(compilerMessages);
                foreach (CompilerMessage cm in compilerMessages)
                {
                    if (cm.type == CompilerMessageType.Error)
                    {
                        Debug.LogErrorFormat("{0}:{1} -- {2}", cm.file, cm.line, cm.message);
                        CompilerErrors = true;
                    }
                }

                // assembly builder does not call ILPostProcessor (WTF Unity?),  so we must invoke it ourselves.
                var compiledAssembly = new CompiledAssembly(assemblyPath)
                {
                    Defines = assemblyBuilder.defaultDefines,
                    References = assemblyBuilder.defaultReferences
                };

                var weaver = new Weaver(logger);

                assembly = weaver.Weave(compiledAssembly);
            };

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
    }
}

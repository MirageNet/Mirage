using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor.Compilation;
using UnityEngine;
#if UNITY_2020_3_OR_NEWER
using System.Text.RegularExpressions;
#else
using Mirage.Weaver;
#endif

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
        public bool CompilerErrors { get; private set; }

        readonly HashSet<string> sourceFiles = new HashSet<string>();
        readonly List<WeaverMessages> messages = new List<WeaverMessages>();
        AssemblyBuilder assemblyBuilder;

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

        public List<WeaverMessages> Build()
        {
            // the way things are build changes in 2020
#if UNITY_2020_3_OR_NEWER
            // in 2020 assemblyBuilder automaitcally calls ILPP
            Build2020();
#else
            // in 2019 assemblyBuilder does not call ILPP
            // this means we have to check for compile errors and then run weaver manually
            Build2019();
#endif
            return messages;
        }

#if UNITY_2020_3_OR_NEWER
        /// <summary>
        /// Builds and Weaves an Assembly with references to unity engine and other asmdefs.
        /// <para>
        ///     NOTE: Does not write the weaved assemble to disk
        /// </para>
        /// </summary>
        void Build2020()
        {
            // This will compile scripts with the same references as files in the asset folder.
            // This means that the dll will get references to all asmdef just as if it was the default "Assembly-CSharp.dll"
            assemblyBuilder = new AssemblyBuilder(ProjectPathFile, sourceFiles.ToArray())
            {
                referencesOptions = ReferencesOptions.UseEngineModules,

            };

            assemblyBuilder.buildFinished += BuildFinished2020;

            Console.WriteLine($"[WeaverTests] Build {ProjectPathFile}");
            // Start build of assembly
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("Failed to start build of assembly {0}", assemblyBuilder.assemblyPath);
                return;
            }

            // wait for finish before returning
            while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
            {
                System.Threading.Thread.Sleep(10);
            }
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

                // we want to remove the tag from the start of it so we just get the message
                string realMessage = RemoveMessageTag(item);

                if (IsCsharpWarning(item, realMessage))
                    continue;

                messages.Add(new WeaverMessages(item.type, realMessage));
            }


        }

        private static string RemoveMessageTag(CompilerMessage item)
        {
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
            return realMessage;
        }

        private static bool IsCsharpWarning(CompilerMessage item, string realMessage)
        {
            // is warning, and starts with `CS1234:` the ignore it because it is c# warning
            return item.type == CompilerMessageType.Warning && Regex.IsMatch(realMessage, "^(CS[0-9]{4}:)");
        }

#else

        /// <summary>
        /// Builds and Weaves an Assembly with references to unity engine and other asmdefs.
        /// <para>
        ///     NOTE: Does not write the weaved assemble to disk
        /// </para>
        /// </summary>
        void Build2019()
        {
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
                return;
            }

            // wait for finish before returning
            while (assemblyBuilder.status != AssemblyBuilderStatus.Finished)
            {
                System.Threading.Thread.Sleep(10);
            }
        }

        void BuildFinished2019(string assemblyPath, CompilerMessage[] compilerMessages)
        {
            foreach (CompilerMessage cm in compilerMessages)
            {
                DebugLog(cm);
                if (cm.type == CompilerMessageType.Error)
                {
                    CompilerErrors = true;
                }
            }

            // assembly builder does not call ILPostProcessor (WTF Unity?),  so we must invoke it ourselves.
            var compiledAssembly = new CompiledAssembly(assemblyPath)
            {
                Defines = assemblyBuilder.defaultDefines,
                References = assemblyBuilder.defaultReferences
            };

            var logger = new WeaverLogger();
            var weaver = new Mirage.Weaver.Weaver(logger);

            _ = weaver.Weave(compiledAssembly);

            foreach (Unity.CompilationPipeline.Common.Diagnostics.DiagnosticMessage item in logger.Diagnostics)
            {
                messages.Add(new WeaverMessages(
                    item.DiagnosticType == Unity.CompilationPipeline.Common.Diagnostics.DiagnosticType.Error ? CompilerMessageType.Error : CompilerMessageType.Warning,
                    item.MessageData
                ));
            }
        }
#endif

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
}

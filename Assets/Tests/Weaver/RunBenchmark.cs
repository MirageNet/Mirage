using Mirage.Tests.Weaver;
using Mirage.Weaver;
using Mono.Cecil;
using NUnit.Framework;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Profiling;

namespace Mirage.BenchmarkWeaver
{
    public class RunBenchmark
    {
        [Test]
        [TestCase("G:/UnityProjects/Mirage/Library/ScriptAssemblies/Mirage.Examples.dll")]
        [TestCase("G:/UnityProjects/Mirage/Library/ScriptAssemblies/Mirage.Tests.Runtime.dll")]
        public void Run(string dll)
        {
            bool old_enableBinaryLog = Profiler.enableBinaryLog;
            string old_logFile = Profiler.logFile;
            //Profiler.logFile = $"./Build/profiler_{Path.GetFileNameWithoutExtension(dll)}.raw";
            //Profiler.enableBinaryLog = true;
            //Profiler.enabled = true;

            var assemblyBuilder = new AssemblyBuilder("./", new string[] { "Assets/Tests/Runtime/Collections/SyncDictionaryTest.cs" })
            {
                referencesOptions = ReferencesOptions.UseEngineModules
            };

            var compiledAssembly = new CompiledAssembly(dll, assemblyBuilder);
            var logger = new WeaverLogger();
            var weaver = new Weaver.Weaver(logger);
            AssemblyDefinition result = weaver.Weave(compiledAssembly);
            for (int i = 0; i < logger.Diagnostics.Count; i++)
            {
                if (logger.Diagnostics[i].DiagnosticType == Unity.CompilationPipeline.Common.Diagnostics.DiagnosticType.Error)
                    Debug.LogError(logger.Diagnostics[i].MessageData);
                else
                    Debug.LogWarning(logger.Diagnostics[i].MessageData);
            }
            Assert.That(result, Is.Not.Null);

            Profiler.enabled = false;
            Profiler.enableBinaryLog = old_enableBinaryLog;
            Profiler.logFile = old_logFile;
        }
    }
}

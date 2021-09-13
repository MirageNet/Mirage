using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;
using UnityEditor;
using UnityEngine.Profiling;

namespace Mirage.Weaver
{
    public static class ProfileWeaver
    {
        public static bool WeaverOnMainThread;

        [MenuItem("Weaver/Profile")]
        public static void run()
        {
            WeaverOnMainThread = true;
            AssetDatabase.ImportAsset("Assets/Tests/Runtime/NetworkDiagnosticsTests.cs");
            //AssetDatabase.ImportAsset("Assets/Tests/Generated/ZigZagTests/ZigZagBehaviour_MyEnum_4.cs");

            EditorApplication.update += MainThreadUpdate;
        }
        static void MainThreadUpdate()
        {
            bool stopProfiler = false;
            while (true)
            {
                WeaveRequest request = null;
                lock (locker)
                {
                    // if queue has item, take it
                    // else wait till next update
                    if (queue.Count > 0)
                    {
                        request = queue.Dequeue();
                    }
                    else
                    {
                        return;
                    }
                }

                if (request != null)
                {
                    // process request
                    Profiler.logFile = $"./Build/profiler_{request.compiledAssembly.Name}_{DateTime.Now.ToFileTime()}.raw";
                    Profiler.enableBinaryLog = true;
                    Profiler.enabled = true;

                    AssemblyDefinition result = request.weaver.Weave(request.compiledAssembly);

                    Profiler.enabled = false;
                    Profiler.enableBinaryLog = false;
                    Profiler.logFile = "";

                    lock (locker)
                    {
                        request.result = result;
                        // if queue is now empty stop profiling
                        // if it has items the while(true) loop will repeat
                        if (queue.Count == 0)
                        {
                            stopProfiler = true;
                            break;
                        }
                    }
                }
            }

            // wait for all to process before stopping profiler
            if (stopProfiler)
            {
                WeaverOnMainThread = false;
                EditorApplication.update -= MainThreadUpdate;
            }
        }

        class WeaveRequest
        {
            public Weaver weaver;
            public ICompiledAssembly compiledAssembly;
            public AssemblyDefinition result;
        }
        static Queue<WeaveRequest> queue = new Queue<WeaveRequest>();
        static object locker = new object();


        internal static AssemblyDefinition WeaverMainThread(Weaver weaver, ICompiledAssembly compiledAssembly)
        {
            var request = new WeaveRequest()
            {
                weaver = weaver,
                compiledAssembly = compiledAssembly,
            };
            lock (locker)
            {
                queue.Enqueue(request);
            }

            while (true)
            {
                Thread.Sleep(1);

                lock (locker)
                {
                    if (request.result != null)
                    {
                        return request.result;
                    }
                }
            }
        }
    }

    public class MirageILPostProcessor : ILPostProcessor
    {
        public const string RuntimeAssemblyName = "Mirage";

        public override ILPostProcessor GetInstance() => this;

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            if (!WillProcess(compiledAssembly))
                return null;

            var logger = new WeaverLogger();
            var weaver = new Weaver(logger);

            AssemblyDefinition assemblyDefinition;
            if (ProfileWeaver.WeaverOnMainThread)
            {
                assemblyDefinition = ProfileWeaver.WeaverMainThread(weaver, compiledAssembly);
            }
            else
            {
                assemblyDefinition = weaver.Weave(compiledAssembly);
            }

            // write
            var pe = new MemoryStream();
            var pdb = new MemoryStream();

            var writerParameters = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };

            assemblyDefinition?.Write(pe, writerParameters);

            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), logger.Diagnostics);
        }

        /// <summary>
        /// Process when assembly that references Mirage
        /// </summary>
        /// <param name="compiledAssembly"></param>
        /// <returns></returns>
        public override bool WillProcess(ICompiledAssembly compiledAssembly) =>
            compiledAssembly.References.Any(filePath => Path.GetFileNameWithoutExtension(filePath) == RuntimeAssemblyName);
    }
}

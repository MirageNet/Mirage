using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Mirage.Weaver
{
    // original code under MIT Copyright (c) 2021 Unity Technologies
    // https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/472d51b34520e8fb6f0aa43fd56d162c3029e0b0/com.unity.netcode.gameobjects/Editor/CodeGen/PostProcessorAssemblyResolver.cs
    internal class PostProcessorAssemblyResolver : IAssemblyResolver
    {
        internal int CacheHits;
        internal int CacheMisses;
        internal int CacheNew;
        internal int CacheSize => _assemblyCache.Count;

        private readonly string[] _assemblyReferences;
        private readonly string[] _assemblyReferencesFileName;
        private readonly ICompiledAssembly _compiledAssembly;
        private AssemblyDefinition _selfAssembly;
        private static readonly Dictionary<string, Cache> _assemblyCache = new Dictionary<string, Cache>();
        private static object _lock = new object();

        private struct Cache
        {
            public AssemblyDefinition Assembly;
            public DateTime WriteTime;
        }

        public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            Console.WriteLine($"[Weaver.PostProcessorAssemblyResolver] Created, static Cache:{_assemblyCache.Count}");
            _compiledAssembly = compiledAssembly;
            _assemblyReferences = compiledAssembly.References;

            //Console.WriteLine($"[Weaver References]\n - {string.Join("\n- ", _assemblyReferences)}");
            // cache paths here so we dont need to call it each time we resolve
            _assemblyReferencesFileName = _assemblyReferences.Select(r => Path.GetFileName(r)).ToArray();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters(ReadingMode.Deferred));

        public static void Log(string msg)
        {
            Console.WriteLine(msg);

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    File.AppendAllText("./WeaverResolver.log", msg);
                    return;
                }
                catch (System.IO.IOException)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.Name == _compiledAssembly.Name)
                return _selfAssembly;

            if (!TryFindFile(name, out var inName))
                return null;

            string beePath, libName, fileName;
            if (FixBeePath(inName, out beePath))
            {
                libName = Path.Combine("Library/ScriptAssemblies", Path.GetFileName(inName));
            }
            else
            {
                beePath = inName;
                libName = inName;
            }

            fileName = inName;


            lock (_lock)
            {
                var lastWriteTime = File.GetLastWriteTime(fileName);
                bool miss;
                if (_assemblyCache.TryGetValue(fileName, out var result))
                {
                    if (result.WriteTime == lastWriteTime)
                    {
                        CacheHits++;
                        return result.Assembly;
                    }
                    CacheMisses++;
                    miss = true;
                }
                else
                {
                    CacheNew++;
                    miss = false;

                }
                var inWriteTime = File.GetLastWriteTime(inName);
                var postWriteTime = File.GetLastWriteTime(beePath);
                var libTime = File.GetLastWriteTime(libName);

                if (beePath != inName)
                {
                    var missText = miss ? "MISS" : "NEW";
                    Log($"[WeaverDebug3] {missText} {_selfAssembly?.Name?.ToString()}\nFile:{inWriteTime:hh:mm:ss.fff} {inName} \nPost:{postWriteTime:hh:mm:ss.fff} {beePath} \nLib :{libTime:hh:mm:ss.fff} {libName}\n\n\n");
                }

                parameters.AssemblyResolver = this;

                var ms = MemoryStreamFor(fileName);

                var pdb = fileName + ".pdb";
                if (File.Exists(pdb))
                    parameters.SymbolStream = MemoryStreamFor(pdb);

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
                _assemblyCache[fileName] = new Cache { Assembly = assemblyDefinition, WriteTime = lastWriteTime };
                return assemblyDefinition;
            }
        }

        private bool FixBeePath(string fullpath, out string postFile)
        {
            postFile = null;
            // if path is in bee folder we need to replace it with the post-processed one, or it wont we can't find referecnes/etc

            if (!fullpath.StartsWith("Library/Bee/artifacts/"))
                return false;
            // Library/Bee/artifacts/1900b0aEDbg.dag/Mirage.Components.dll
            // Library/Bee/artifacts/1900b0aEDbg.dag/post-processed/Mirage.Components.dll

            // we need to get the directory, then append "post-processed" folder, then filename
            var dir = Path.GetDirectoryName(fullpath);
            var fileName = Path.GetFileName(fullpath);

            postFile = Path.Combine(dir, "post-processed", fileName);

            //var lastWriteTime = File.GetLastWriteTime(fullpath);
            //var postWriteTime = File.GetLastWriteTime(postFile);
            //var libTime = File.GetLastWriteTime(Path.Combine("Library\\ScriptAssemblies", fileName));
            //Console.WriteLine($"[WeaverDebug3] {fileName}\nFile:{lastWriteTime:mm:ss.fff}\nPost:{postWriteTime:mm:ss.fff}\nLib:{libTime:mm:ss.fff}");

            // check it exists just incase
            if (File.Exists(postFile))
                return true;
            else
                return false;
        }

        private bool TryFindFile(AssemblyNameReference name, out string filePath)
        {
            // This method is called a lot, avoid linq

            // first pass, check if we can find dll or exe file
            var dllName = name.Name + ".dll";
            var exeName = name.Name + ".exe";
            for (var i = 0; i < _assemblyReferencesFileName.Length; i++)
            {
                // if filename matches, return full path
                var fileName = _assemblyReferencesFileName[i];
                if (fileName == dllName || fileName == exeName)
                {
                    filePath = _assemblyReferences[i];
                    return true;
                }
            }

            // second pass (only run if first fails), 

            //Unfortunately the current ICompiledAssembly API only provides direct references.
            //It is very much possible that a postprocessor ends up investigating a type in a directly
            //referenced assembly, that contains a field that is not in a directly referenced assembly.
            //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            //in the ILPostProcessing API. As a workaround, we rely on the fact here that the indirect references
            //are always located next to direct references, so we search in all directories of direct references we
            //got passed, and if we find the file in there, we resolve to it.
            var allParentDirectories = _assemblyReferences.Select(Path.GetDirectoryName).Distinct();
            foreach (var parentDir in allParentDirectories)
            {
                var candidate = Path.Combine(parentDir, name.Name + ".dll");
                if (File.Exists(candidate))
                {
                    filePath = candidate;
                    return true;
                }
            }

            Console.WriteLine($"[WeaverResolve] Failed to resolve:{name.Name}");
            filePath = null;
            return false;
        }

        private static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () =>
            {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    var readLength = fs.Read(byteArray, 0, (int)fs.Length);
                    if (readLength != fs.Length)
                        throw new InvalidOperationException("File read length is not full length of file.");
                }

                return new MemoryStream(byteArray);
            });
        }

        private static MemoryStream Retry(int retryCount, TimeSpan waitTime, Func<MemoryStream> func)
        {
            try
            {
                return func();
            }
            catch (IOException)
            {
                if (retryCount == 0)
                    throw;
                Console.WriteLine($"Caught IO Exception, trying {retryCount} more times");
                Thread.Sleep(waitTime);
                return Retry(retryCount - 1, waitTime, func);
            }
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            _selfAssembly = assemblyDefinition;
        }
    }
}

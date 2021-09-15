using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;

namespace Mirage.Weaver
{
    class PostProcessorAssemblyResolver : IAssemblyResolver
    {
        private static readonly Dictionary<int, AssemblyDefinitionWithLock> _assemblyCache = new Dictionary<int, AssemblyDefinitionWithLock>();

        private string[] _assemblyReferencesFileName;
        private string[] _assemblyReferences;
        private string _selfName;
        private AssemblyDefinition _selfAssembly;

        public void AddAssemblyDefinitionBeingOperatedOn(string name, string[] references, AssemblyDefinition assemblyDefinition)
        {
            _selfAssembly = assemblyDefinition;
            _selfName = name;
            _assemblyReferences = references;
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


        class AssemblyDefinitionWithLock
        {
            public readonly object locker = new object();
            public AssemblyDefinition Definition;
        }
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.Name == _selfName)
                return _selfAssembly;

            if (!GetFileNameAndKey(name, out string fileName, out int cacheKey))
                return null;

            AssemblyDefinitionWithLock result;
            // lock cache to get/create new Item
            if (!_assemblyCache.TryGetValue(cacheKey, out result))
            {
                lock (_assemblyCache)
                {
                    // try get it again incase another thread added it while we were waiting for lock
                    if (!_assemblyCache.TryGetValue(cacheKey, out result))
                    {
                        Console.WriteLine($"[PostProcessorAssemblyResolver] AddKey {fileName}");

                        result = new AssemblyDefinitionWithLock();
                        _assemblyCache[cacheKey] = result;
                    }
                }
            }


            if (result.Definition != null)
            {
                Console.WriteLine($"[PostProcessorAssemblyResolver] QuickCache {fileName}");
                return result.Definition;
            }


            // once we have the item, lock it to check if it is loaded or not
            lock (result.locker)
            {
                // try it again incase another thread loaded it
                if (result.Definition == null)
                {
                    Console.WriteLine($"[PostProcessorAssemblyResolver] LoadFile {fileName}");
                    result.Definition = ReadNewAssembly(parameters, fileName);
                }
                else
                {
                    Console.WriteLine($"[PostProcessorAssemblyResolver] SlowCache {fileName}");
                }

                return result.Definition;

            }
        }

        private bool GetFileNameAndKey(AssemblyNameReference name, out string fileName, out int cacheKey)
        {
            fileName = FindFile(name);
            cacheKey = 0;
            if (fileName == null)
                return false;

            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
            cacheKey = GetCombineHash(fileName, lastWriteTime);
            return true;
        }

        private static int GetCombineHash(string fileName, DateTime lastWriteTime)
        {
            unchecked
            {

                int hash = 17;
                hash = hash * 31 + fileName.GetHashCode();
                hash = hash * 31 + lastWriteTime.GetHashCode();
                return hash;
            }
        }

        private string FindFile(AssemblyNameReference name)
        {
            // This method is called a lot, avoid linq

            // first pass, check if we can find dll or exe file
            string dllName = name.Name + ".dll";
            string exeName = name.Name + ".exe";
            for (int i = 0; i < _assemblyReferencesFileName.Length; i++)
            {
                // if filename matches, return full path
                string fileName = _assemblyReferencesFileName[i];
                if (fileName == dllName || fileName == exeName)
                    return _assemblyReferences[i];
            }

            // second pass (only run if first fails), 

            //Unfortunately the current ICompiledAssembly API only provides direct references.
            //It is very much possible that a postprocessor ends up investigating a type in a directly
            //referenced assembly, that contains a field that is not in a directly referenced assembly.
            //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            //in the ILPostProcessing API. As a workaround, we rely on the fact here that the indirect references
            //are always located next to direct references, so we search in all directories of direct references we
            //got passed, and if we find the file in there, we resolve to it.
            IEnumerable<string> allParentDirectories = _assemblyReferences.Select(Path.GetDirectoryName).Distinct();
            foreach (string parentDir in allParentDirectories)
            {
                string candidate = Path.Combine(parentDir, name.Name + ".dll");
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }


        private AssemblyDefinition ReadNewAssembly(ReaderParameters parameters, string fileName)
        {
            var resolver = new PostProcessorAssemblyResolver();
            parameters.AssemblyResolver = resolver;

            MemoryStream ms = MemoryStreamFor(fileName);

            string pdb = fileName + ".pdb";
            if (File.Exists(pdb))
                parameters.SymbolStream = MemoryStreamFor(pdb);

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
            resolver.AddAssemblyDefinitionBeingOperatedOn(assemblyDefinition.Name.Name, Array.Empty<string>(), assemblyDefinition);
            return assemblyDefinition;
        }

        static MemoryStream MemoryStreamFor(string fileName)
        {
            return Retry(10, TimeSpan.FromSeconds(1), () =>
            {
                byte[] byteArray;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byteArray = new byte[fs.Length];
                    int readLength = fs.Read(byteArray, 0, (int)fs.Length);
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
    }
}

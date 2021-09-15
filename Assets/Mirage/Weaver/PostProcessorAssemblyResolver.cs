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
        class AssemblyDefinitionWithLock
        {
            public readonly object locker = new object();
            public AssemblyDefinition Definition;
        }
        class ReferenceWithName
        {
            public readonly string References;
            public readonly string FileName;

            public ReferenceWithName(string references)
            {
                References = references;
                FileName = Path.GetFileName(References);
            }

            public override bool Equals(object obj)
            {
                if (obj is ReferenceWithName other)
                {
                    return References == other.References;
                }
                return false;
            }
            public override int GetHashCode()
            {
                return References.GetHashCode();
            }
        }
        private static readonly Dictionary<int, AssemblyDefinitionWithLock> _assemblyCache = new Dictionary<int, AssemblyDefinitionWithLock>();
        private static readonly Dictionary<string, ReferenceWithName> _assemblyReferences = new Dictionary<string, ReferenceWithName>();

        private string _selfName;
        private AssemblyDefinition _selfAssembly;

        public void AddAssemblyDefinitionBeingOperatedOn(string name, string[] references, AssemblyDefinition assemblyDefinition)
        {
            _selfAssembly = assemblyDefinition;
            _selfName = name;

            lock (_assemblyReferences)
            {
                foreach (string newRef in references)
                {
                    var withName = new ReferenceWithName(newRef);
                    if (_assemblyReferences.TryGetValue(withName.FileName, out ReferenceWithName Existing))
                    {
                        if (Existing.References != withName.References)
                        {
                            Console.WriteLine($"[PostProcessorAssemblyResolver] Already exists!!! Existing:{Existing.References} New:{withName.References}");
                        }
                    }
                    else
                    {
                        _assemblyReferences.Add(withName.FileName, withName);
                    }
                }
            }
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
            ReferenceWithName withName;
            if (_assemblyReferences.TryGetValue(dllName, out withName))
            {
                return withName.References;
            }
            if (_assemblyReferences.TryGetValue(exeName, out withName))
            {
                return withName.References;
            }

            //for (int i = 0; i < _assemblyReferences.Length; i++)
            //{
            //    // if filename matches, return full path
            //    string fileName = _assemblyReferences[i];
            //    if (fileName == dllName || fileName == exeName)
            //        return _assemblyReferences[i];
            //}

            // second pass (only run if first fails), 

            //Unfortunately the current ICompiledAssembly API only provides direct references.
            //It is very much possible that a postprocessor ends up investigating a type in a directly
            //referenced assembly, that contains a field that is not in a directly referenced assembly.
            //if we don't do anything special for that situation, it will fail to resolve.  We should fix this
            //in the ILPostProcessing API. As a workaround, we rely on the fact here that the indirect references
            //are always located next to direct references, so we search in all directories of direct references we
            //got passed, and if we find the file in there, we resolve to it.
            IEnumerable<string> allParentDirectories = _assemblyReferences.Select(x => x.Value.References).Select(Path.GetDirectoryName).Distinct();
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

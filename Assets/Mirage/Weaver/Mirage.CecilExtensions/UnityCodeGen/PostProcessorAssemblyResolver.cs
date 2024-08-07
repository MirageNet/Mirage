using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Mirage.CodeGen
{
    // original code under MIT Copyright (c) 2021 Unity Technologies
    // https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/472d51b34520e8fb6f0aa43fd56d162c3029e0b0/com.unity.netcode.gameobjects/Editor/CodeGen/PostProcessorAssemblyResolver.cs
    internal sealed class PostProcessorAssemblyResolver : IAssemblyResolver
    {
        private readonly string[] _assemblyReferences;
        private readonly string[] _assemblyReferencesFileName;
        private readonly string[] _allParentDirectories;
        private readonly Dictionary<string, AssemblyDefinition> _assemblyCache = new Dictionary<string, AssemblyDefinition>();
        private readonly ICompiledAssembly _compiledAssembly;
        private AssemblyDefinition _selfAssembly;

        public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            _compiledAssembly = compiledAssembly;
            _assemblyReferences = compiledAssembly.References;
            // cache paths here so we dont need to call it each time we resolve
            _assemblyReferencesFileName = _assemblyReferences.Select(r => Path.GetFileName(r)).ToArray();

            // add path to a system type, because reference path might not be correct
            var systemTypePath = typeof(int).Assembly.Location;
            _allParentDirectories = _assemblyReferences
                .Append(systemTypePath)
                .Select(Path.GetDirectoryName)
                .Distinct()
                .ToArray();
        }

        public void Dispose()
        {
            foreach (var asm in _assemblyCache.Values)
                asm.Dispose();
            _assemblyCache.Clear();
        }

        public void AddAssemblyDefinitionBeingOperatedOn(AssemblyDefinition assemblyDefinition)
        {
            _selfAssembly = assemblyDefinition;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, null);

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name.Name == _compiledAssembly.Name)
                return _selfAssembly;

            var fileName = FindFile(name);
            if (fileName == null)
                return null;

            var lastWriteTime = File.GetLastWriteTime(fileName);

            var cacheKey = fileName + lastWriteTime;

            if (_assemblyCache.TryGetValue(cacheKey, out var result))
                return result;

            if (parameters == null)
                parameters = new ReaderParameters(ReadingMode.Deferred);

            parameters.AssemblyResolver = this;

            var ms = MemoryStreamFor(fileName);

            var pdb = fileName + ".pdb";
            if (File.Exists(pdb))
                parameters.SymbolStream = MemoryStreamFor(pdb);

            var assemblyDefinition = AssemblyDefinition.ReadAssembly(ms, parameters);
            _assemblyCache.Add(cacheKey, assemblyDefinition);
            return assemblyDefinition;
        }

        private string FindFile(AssemblyNameReference name)
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
            foreach (var parentDir in _allParentDirectories)
            {
                var candidate = Path.Combine(parentDir, name.Name + ".dll");
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private static MemoryStream MemoryStreamFor(string fileName)
        {
            var retryCount = 10;
            const int waitMs = 1000;
            while (retryCount > 0)
            {
                try
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
                }
                catch (IOException)
                {
                    retryCount--;
                    if (retryCount == 0)
                        throw;

                    Console.WriteLine($"Caught IO Exception for {fileName}, trying {retryCount} more times");
                    Thread.Sleep(waitMs);
                }
            }

            throw new InvalidOperationException("Should never get here");
        }
    }
}

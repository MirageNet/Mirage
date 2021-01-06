using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace Mirror.Weaver
{
    internal class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
        {
            return new PostProcessorReflectionImporter(module);
        }
    }
}
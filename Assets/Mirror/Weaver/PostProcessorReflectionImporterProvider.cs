using Mono.Cecil;

namespace Mirage.Weaver
{
    internal class PostProcessorReflectionImporterProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition module)
        {
            return new PostProcessorReflectionImporter(module);
        }
    }
}
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public static class NetworkWriterHelper
    {
        public static void CallRelease(ModuleImportCache moduleCache, ILProcessor worker, VariableDefinition writer)
        {
            MethodReference releaseMethod = GetReleaseMethod(moduleCache, writer);

            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Call, releaseMethod));
        }
        public static MethodReference GetReleaseMethod(ModuleImportCache moduleCache, VariableDefinition writer)
        {
            return GetReleaseMethod(moduleCache, writer.VariableType.Resolve());
        }
        public static MethodReference GetReleaseMethod(ModuleImportCache moduleCache, TypeDefinition writer)
        {
            MethodDefinition releaseMethod = writer.GetMethod(nameof(PooledNetworkWriter.Release));
            return moduleCache.ImportReference(releaseMethod);
        }
    }
}

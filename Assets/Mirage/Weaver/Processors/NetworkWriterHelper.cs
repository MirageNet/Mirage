using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public static class NetworkWriterHelper
    {
        public static void CallRelease(ModuleDefinition module, ILProcessor worker, VariableDefinition writer)
        {
            var releaseMethod = GetReleaseMethod(module, writer);

            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Call, releaseMethod));
        }
        public static MethodReference GetReleaseMethod(ModuleDefinition module, VariableDefinition writer)
        {
            return GetReleaseMethod(module, writer.VariableType.Resolve());
        }
        public static MethodReference GetReleaseMethod(ModuleDefinition module, TypeDefinition writer)
        {
            var releaseMethod = writer.GetMethod(nameof(PooledNetworkWriter.Release));
            return module.ImportReference(releaseMethod);
        }
    }
}

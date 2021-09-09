using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal abstract class ValueSerializer
    {
        /// <summary>
        /// Is the type that this Serializer for an int based type? (byte, int, ulong, etc)
        /// </summary>
        public abstract bool IsIntType { get; }

        public abstract void AppendWrite(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, FoundSyncVar syncVar);
        public abstract void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar);
    }
}

using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    /// <summary>
    /// Appends IL codes to write/read a field/param using bitpacking or extension methods
    /// <para>Use <see cref="ValueSerializerFinder"/> to find a ValueSerializer from a attributes</para>
    /// </summary>
    internal abstract class ValueSerializer
    {
        /// <summary>
        /// Is the type that this Serializer for an int based type? (byte, int, ulong, etc)
        /// </summary>
        public abstract bool IsIntType { get; }

        public abstract void AppendWrite(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition);
        public abstract void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar);

        protected static FieldReference ImportField(ModuleDefinition module, FieldDefinition fieldDefinition)
        {
            return module.ImportReference(fieldDefinition.MakeHostGenericIfNeeded());
        }
    }
}

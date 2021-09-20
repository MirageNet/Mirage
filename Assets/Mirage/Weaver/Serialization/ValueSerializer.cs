using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    /// <summary>
    /// Appends IL codes to write/read a field/param using bitpacking or extension methods
    /// <para>Use <see cref="ValueSerializerFinder"/> to find a ValueSerializer from a attributes</para>
    /// </summary>
    public abstract class ValueSerializer
    {
        /// <summary>
        /// Is the type that this Serializer for an int based type? (byte, int, ulong, etc)
        /// </summary>
        public abstract bool IsIntType { get; }

        public abstract void AppendWriteField(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition);
        public abstract void AppendWriteParameter(ModuleDefinition module, ILProcessor worker, VariableDefinition writer, ParameterDefinition valueParameter);

        public abstract void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, TypeReference fieldType);

        protected static FieldReference ImportField(ModuleDefinition module, FieldDefinition fieldDefinition)
        {
            return module.ImportReference(fieldDefinition.MakeHostGenericIfNeeded());
        }

        protected static Instruction LoadParamOrArg0(ILProcessor worker, ParameterDefinition parameter)
        {
            if (parameter == null)
            {
                return worker.Create(OpCodes.Ldarg_0);
            }
            else
            {
                return worker.Create(OpCodes.Ldarg, parameter);
            }
        }
    }
}

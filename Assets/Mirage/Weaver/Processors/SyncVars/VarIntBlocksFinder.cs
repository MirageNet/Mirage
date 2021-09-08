using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal static class VarIntBlocksFinder
    {
        public static (int? blockSize, OpCode? ConvertCode) GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<VarIntBlocksAttribute>();
            if (attribute == null)
                return default;

            ThrowIfNotIntType(syncVar.FieldType, syncVar);

            int blockSize = (int)attribute.ConstructorArguments[0].Value;

            if (blockSize <= 0)
                throw new VarIntBlocksException("Blocksize should be above 0", syncVar);

            // 32 is reasonable max value
            if (blockSize > 32)
                throw new VarIntBlocksException("Blocksize should be below 32", syncVar);

            return (blockSize, BitPackHelper.GetConvertType(syncVar.FieldType));
        }

        /// <summary>
        /// Any Int or enum based type is valid
        /// </summary>
        /// <param name="type"></param>
        /// <param name="syncVar"></param>
        static void ThrowIfNotIntType(TypeReference type, FieldDefinition syncVar)
        {
            // throw if unsigned
            if (type.Is<byte>()
             || type.Is<short>()
             || type.Is<ushort>()
             || type.Is<int>()
             || type.Is<uint>()
             || type.Is<long>()
             || type.Is<ulong>())
                return;

            if (type.Resolve().IsEnum)
            {
                // check underlying field is signed
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                ThrowIfNotIntType(enumType, syncVar);
                return;
            }

            throw new VarIntBlocksException($"{type.FullName} is not supported for [VarIntBlocks]", syncVar);
        }
    }
}

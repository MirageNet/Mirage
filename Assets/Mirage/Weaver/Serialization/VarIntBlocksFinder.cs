using Mirage.CodeGen;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver.Serialization
{
    internal static class VarIntBlocksFinder
    {
        public static ValueSerializer GetSerializer(ICustomAttributeProvider attributeProvider, TypeReference fieldType)
        {
            var attribute = attributeProvider.GetCustomAttribute<VarIntBlocksAttribute>();
            if (attribute == null)
                return default;

            ThrowIfNotIntType(fieldType);

            var blockSize = (int)attribute.ConstructorArguments[0].Value;

            if (blockSize <= 0)
                throw new VarIntBlocksException("Blocksize should be above 0");

            // 32 is reasonable max value
            if (blockSize > 32)
                throw new VarIntBlocksException("Blocksize should be below 32");

            return new BlockSizeSerializer(blockSize, BitPackHelper.GetConvertType(fieldType));
        }

        /// <summary>
        /// Any Int or enum based type is valid
        /// </summary>
        /// <param name="type"></param>
        /// <param name="syncVar"></param>
        private static void ThrowIfNotIntType(TypeReference type)
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
                var enumType = type.Resolve().GetEnumUnderlyingType();
                ThrowIfNotIntType(enumType);
                return;
            }

            throw new VarIntBlocksException($"{type.FullName} is not supported for [VarIntBlocks]");
        }
    }
}

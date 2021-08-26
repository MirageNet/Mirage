using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal static class BitCountFinder
    {
        public static (int? bitCount, OpCode? ConvertCode) GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<BitCountAttribute>();
            if (attribute == null)
                return default;

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new BitCountException("BitCount should be above 0", syncVar);

            int maxSize = GetTypeMaxSize(syncVar.FieldType, syncVar);

            if (bitCount > maxSize)
                throw new BitCountException($"BitCount can not be above target type size, bitCount:{bitCount}, max size:{maxSize}, type:{syncVar.FieldType.Name}", syncVar);

            return (bitCount, GetConvertType(syncVar.FieldType));
        }

        static int GetTypeMaxSize(TypeReference type, FieldDefinition syncVar)
        {
            if (type.Is<byte>()) return 8;
            if (type.Is<ushort>()) return 16;
            if (type.Is<short>()) return 16;
            if (type.Is<uint>()) return 32;
            if (type.Is<int>()) return 32;
            if (type.Is<ulong>()) return 64;
            if (type.Is<long>()) return 64;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMaxSize(enumType, syncVar);
            }

            throw new BitCountException($"{type.FullName} is not a supported type for [BitCount]", syncVar);
        }

        /// <summary>
        /// Read returns a ulong, so if field is a smaller type it must be converted to a int32. all smaller types are padded to anyway
        /// </summary>
        /// <param name="syncVar"></param>
        /// <returns></returns>
        public static OpCode? GetConvertType(TypeReference type)
        {
            if (type.Is<byte>()) return OpCodes.Conv_I4;
            if (type.Is<ushort>()) return OpCodes.Conv_I4;
            if (type.Is<short>()) return OpCodes.Conv_I4;
            if (type.Is<uint>()) return OpCodes.Conv_I4;
            if (type.Is<int>()) return OpCodes.Conv_I4;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for cast
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetConvertType(enumType);
            }

            return default;
        }
    }
}

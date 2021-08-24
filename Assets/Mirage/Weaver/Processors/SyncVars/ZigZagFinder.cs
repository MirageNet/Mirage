using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    public static class ZigZagFinder
    {
        public static bool HasZigZag(FieldDefinition syncVar, bool hasBitCount)
        {
            bool hasAttribute = syncVar.HasCustomAttribute<ZigZagEncodeAttribute>();
            if (!hasAttribute)
                return false;

            if (!hasBitCount)
                throw new ZigZagException($"[ZigZagEncode] can only be used with [BitCount]", syncVar);

            ThrowIfUnsignedType(syncVar.FieldType, syncVar);
            return true;
        }

        /// <summary>
        /// Any Int or enum based type is valid
        /// </summary>
        /// <param name="type"></param>
        /// <param name="syncVar"></param>
        static void ThrowIfUnsignedType(TypeReference type, FieldDefinition syncVar)
        {
            // throw if unsigned
            if (type.Is<byte>()
             || type.Is<ushort>()
             || type.Is<uint>()
             || type.Is<ulong>())
                throw new ZigZagException($"[ZigZagEncode] can only be used on a signed type", syncVar);

            if (type.Resolve().IsEnum)
            {
                // check underlying field is signed
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                ThrowIfUnsignedType(enumType, syncVar);
            }
        }
    }
}

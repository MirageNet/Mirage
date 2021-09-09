using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal static class ZigZagFinder
    {
        public static void CheckZigZag(FieldDefinition syncVar, ref ValueSerializer valueSerializer)
        {
            bool hasAttribute = syncVar.HasCustomAttribute<ZigZagEncodeAttribute>();
            if (!hasAttribute)
                return;

            if (valueSerializer is BitCountSerializer bitCountSerializer)
            {
                ThrowIfUnsignedType(syncVar.FieldType, syncVar);
                valueSerializer = bitCountSerializer.CopyWithZigZag();
            }
            else
                throw new ZigZagException($"[ZigZagEncode] can only be used with [BitCount]", syncVar);
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

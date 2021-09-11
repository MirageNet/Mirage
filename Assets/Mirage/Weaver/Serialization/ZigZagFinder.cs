using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal static class ZigZagFinder
    {
        public static void CheckZigZag(ICustomAttributeProvider attributeProvider, TypeReference fieldType, ref ValueSerializer valueSerializer)
        {
            bool hasAttribute = attributeProvider.HasCustomAttribute<ZigZagEncodeAttribute>();
            if (!hasAttribute)
                return;

            if (valueSerializer is BitCountSerializer bitCountSerializer)
            {
                ThrowIfUnsignedType(fieldType);
                valueSerializer = bitCountSerializer.CopyWithZigZag();
            }
            else
                throw new ZigZagException($"[ZigZagEncode] can only be used with [BitCount]");
        }

        /// <summary>
        /// Any Int or enum based type is valid
        /// </summary>
        /// <param name="type"></param>
        /// <param name="syncVar"></param>
        static void ThrowIfUnsignedType(TypeReference type)
        {
            // throw if unsigned
            if (type.Is<byte>()
             || type.Is<ushort>()
             || type.Is<uint>()
             || type.Is<ulong>())
                throw new ZigZagException($"[ZigZagEncode] can only be used on a signed type");

            if (type.Resolve().IsEnum)
            {
                // check underlying field is signed
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                ThrowIfUnsignedType(enumType);
            }
        }
    }
}

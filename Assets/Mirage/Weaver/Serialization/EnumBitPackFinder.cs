using System;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal static class EnumBitPackFinder
    {
        /// <summary>
        /// Gets serializer from the bit packing attributes on an enum type
        /// </summary>
        /// <returns>null if enum has no bit packing attributes</returns>
        /// <exception cref="ValueSerializerException">Throws when attribute is used incorrectly or when enum values don't fit</exception>
        public static BitCountSerializer GetSerializer(TypeDefinition enumDef)
        {
            ValueSerializer valueSerializer = null;

            if (enumDef.HasCustomAttribute<BitCountAttribute>())
                valueSerializer = BitCountFinder.GetSerializer(enumDef, enumDef);

            if (enumDef.HasCustomAttribute<BitCountFromRangeAttribute>())
            {
                if (valueSerializer != null)
                    throw new BitCountFromRangeException("[BitCountFromRange] can't be used with [BitCount]");

                valueSerializer = BitCountFromRangeFinder.GetSerializer(enumDef, enumDef);
            }

            ZigZagFinder.CheckZigZag(enumDef, enumDef, ref valueSerializer);

            var bitCountSerializer = (BitCountSerializer)valueSerializer;
            if (bitCountSerializer != null)
                ThrowIfValuesDontFit(enumDef, bitCountSerializer);

            return bitCountSerializer;
        }

        /// <summary>
        /// Checks every defined enum value can be sent using the serializer's bit count
        /// <para>Values are truncated when written, so a value that doesn't fit would be silently changed on the wire</para>
        /// </summary>
        /// <exception cref="BitCountException">Throws when a value does not fit</exception>
        public static void ThrowIfValuesDontFit(TypeDefinition enumDef, BitCountSerializer serializer)
        {
            foreach (var field in enumDef.Fields)
            {
                // enum values are const fields, this skips the instance `value__` field
                if (!field.HasConstant)
                    continue;

                var encoded = GetEncodedValue(enumDef, field, serializer);

                // 64 bits is handled separately because shifting a ulong by 64 does nothing
                var fits = serializer.BitCount >= 64 || encoded < (1UL << serializer.BitCount);
                if (!fits)
                    throw new BitCountException($"Enum value {enumDef.Name}.{field.Name} ({field.Constant}) does not fit in {serializer.BitCount} bits");
            }
        }

        /// <summary>
        /// Gets the value that will be written before it is truncated to bit count
        /// </summary>
        private static ulong GetEncodedValue(TypeDefinition enumDef, FieldDefinition field, BitCountSerializer serializer)
        {
            // ulong is the only underlying type that can be above long.MaxValue
            // it can't be used with ZigZag or BitCountFromRange, so it is written as is
            if (field.Constant is ulong ulongValue)
                return ulongValue;

            var value = Convert.ToInt64(field.Constant);

            if (serializer.UseZigZag)
                return ZigZag.Encode(value);

            if (serializer.MinValue.HasValue)
            {
                var offset = value - serializer.MinValue.Value;
                if (offset < 0)
                    throw new BitCountException($"Enum value {enumDef.Name}.{field.Name} ({value}) is below min value ({serializer.MinValue.Value})");

                return (ulong)offset;
            }

            if (value < 0)
                throw new BitCountException($"Enum value {enumDef.Name}.{field.Name} ({value}) is negative, use [ZigZagEncode] or [BitCountFromRange] to send negative values");

            return (ulong)value;
        }
    }
}

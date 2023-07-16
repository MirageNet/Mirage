using System;
using Mirage.CodeGen;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver.Serialization
{
    public static class BitPackHelper
    {
        public static int GetBitCount(long range, int max = 32)
        {
            // cast range to ulong using checked so it will throw if it is negative
            return GetBitCount(checked((ulong)range), max);
        }
        public static int GetBitCount(ulong range, int max = 32)
        {
            // make sure to cast max to long so incase range is bigger than int value
            var bitCount = (int)Math.Floor(Math.Log(range, 2)) + 1;
            if (bitCount < 0 || bitCount > max)
                throw new OverflowException($"Bit Count could not be calcualted, range:{range}, bitCount:{bitCount}");

            return bitCount;
        }

        public static int GetTypeMax(TypeReference type, string attributeName)
        {
            if (type.Is<byte>()) return byte.MaxValue;
            if (type.Is<ushort>()) return ushort.MaxValue;
            if (type.Is<short>()) return short.MaxValue;
            if (type.Is<int>()
             || type.Is<uint>()) return int.MaxValue;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                var enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMax(enumType, attributeName);
            }

            // long is not a support type because it is not commonly used and
            // would take a lot more code to make it works with all possible
            // ranges of values:
            // - min and max both negative long values
            // - min and max both above long.max
            // we would need a type bigger than long in order to easily handle
            // both of these, and that is before dealing with IL stuff

            throw new BitCountFromRangeException($"{type.FullName} is not a supported type for [{attributeName}]");
        }

        public static int GetTypeMin(TypeReference type, string attributeName)
        {
            if (type.Is<byte>()) return byte.MinValue;
            if (type.Is<ushort>()) return ushort.MinValue;
            if (type.Is<short>()) return short.MinValue;
            if (type.Is<int>()) return int.MinValue;
            if (type.Is<uint>()) return 0;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                var enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMin(enumType, attributeName);
            }

            throw new BitCountFromRangeException($"{type.FullName} is not a supported type for [{attributeName}]");
        }

        public static int GetTypeMaxSize(TypeReference type, string attributeName)
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
                var enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMaxSize(enumType, attributeName);
            }

            throw new BitCountException($"{type.FullName} is not a supported type for [{attributeName}]");
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
                var enumType = type.Resolve().GetEnumUnderlyingType();
                return GetConvertType(enumType);
            }

            return default;
        }
    }
}

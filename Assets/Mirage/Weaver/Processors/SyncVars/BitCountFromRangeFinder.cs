using System;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    public static class BitCountFromRangeFinder
    {
        internal static (int? BitCount, OpCode? BitCountConvert, int? MinValue) GetBitFoundFromRange(FieldDefinition syncVar, bool hasBitCount)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<BitCountFromRangeAttribute>();

            if (hasBitCount)
                throw new BitCountFromRangeException($"[BitCountFromRange] can't be used with [BitCount]", syncVar);

            int min = (int)attribute.ConstructorArguments[0].Value;
            int max = (int)attribute.ConstructorArguments[1].Value;

            if (min >= max)
                throw new BitCountFromRangeException("Max must be greater than min", syncVar);

            long minAllowedMin = GetTypeMin(syncVar.FieldType, syncVar);
            long maxAllowedMax = GetTypeMax(syncVar.FieldType, syncVar);

            if (min < minAllowedMin)
                throw new BitCountException($"Min must be less than types min value, min:{min}, min allowed:{minAllowedMin}, type:{syncVar.FieldType.Name}", syncVar);

            if (max > maxAllowedMax)
                throw new BitCountException($"Max must be greater than types max value, max:{max}, max allowed:{maxAllowedMax}, type:{syncVar.FieldType.Name}", syncVar);

            // make sure to cast max to long so incase range is bigger than int value
            long range = checked((long)max - min);
            int bitCount = (int)Math.Floor(Math.Log(range, 2)) + 1;
            if (bitCount < 0 || bitCount > 32)
                throw new OverflowException($"Bit Count could not be calcualted, min:{min}, max:{max}, bitCount:{bitCount}");

            int? minResult;
            if (min == 0) minResult = null;
            else minResult = min;

            return (bitCount, BitCountFinder.GetConvertType(syncVar.FieldType), minResult);
        }

        static int GetTypeMax(TypeReference type, FieldDefinition syncVar)
        {
            if (type.Is<byte>()) return byte.MaxValue;
            if (type.Is<ushort>()) return ushort.MaxValue;
            if (type.Is<short>()) return short.MaxValue;
            if (type.Is<int>()
             || type.Is<uint>()) return int.MaxValue;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMax(enumType, syncVar);
            }

            // long is not a support type because it is not commonly used and
            // would take a lot more code to make it works with all possible
            // ranges of values:
            // - min and max both negative long values
            // - min and max both above long.max
            // we would need a type bigger than long in order to easily handle
            // both of these, and that is before dealing with IL stuff

            throw new BitCountFromRangeException($"{type.FullName} is not a supported type for [BitCountFromRange]", syncVar);
        }

        static int GetTypeMin(TypeReference type, FieldDefinition syncVar)
        {
            if (type.Is<byte>()) return byte.MinValue;
            if (type.Is<ushort>()) return ushort.MinValue;
            if (type.Is<short>()) return short.MinValue;
            if (type.Is<int>()) return int.MinValue;
            if (type.Is<uint>()) return 0;

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetTypeMin(enumType, syncVar);
            }

            throw new BitCountFromRangeException($"{type.FullName} is not a supported type for [BitCountFromRange]", syncVar);
        }
    }
}

using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal static class BitCountFromRangeFinder
    {
        public static ValueSerializer GetSerializer(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<BitCountFromRangeAttribute>();

            int min = (int)attribute.ConstructorArguments[0].Value;
            int max = (int)attribute.ConstructorArguments[1].Value;

            if (min >= max)
                throw new BitCountFromRangeException("Max must be greater than min", syncVar);

            long minAllowedMin = BitPackHelper.GetTypeMin(syncVar.FieldType, syncVar, "BitCountFromRange");
            long maxAllowedMax = BitPackHelper.GetTypeMax(syncVar.FieldType, syncVar, "BitCountFromRange");

            if (min < minAllowedMin)
                throw new BitCountFromRangeException($"Min must be less than types min value, min:{min}, min allowed:{minAllowedMin}, type:{syncVar.FieldType.Name}", syncVar);

            if (max > maxAllowedMax)
                throw new BitCountFromRangeException($"Max must be greater than types max value, max:{max}, max allowed:{maxAllowedMax}, type:{syncVar.FieldType.Name}", syncVar);

            int bitCount = BitPackHelper.GetBitCount(checked((long)max - min));

            int? minResult;
            if (min == 0) minResult = null;
            else minResult = min;

            return new BitCountSerializer(bitCount, BitPackHelper.GetConvertType(syncVar.FieldType), minResult);
        }
    }
}

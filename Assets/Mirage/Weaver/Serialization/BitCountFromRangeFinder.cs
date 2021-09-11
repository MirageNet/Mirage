using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal static class BitCountFromRangeFinder
    {
        public static ValueSerializer GetSerializer(ICustomAttributeProvider attributeProvider, TypeReference fieldType)
        {
            CustomAttribute attribute = attributeProvider.GetCustomAttribute<BitCountFromRangeAttribute>();

            int min = (int)attribute.ConstructorArguments[0].Value;
            int max = (int)attribute.ConstructorArguments[1].Value;

            if (min >= max)
                throw new BitCountFromRangeException("Max must be greater than min");

            long minAllowedMin = BitPackHelper.GetTypeMin(fieldType, "BitCountFromRange");
            long maxAllowedMax = BitPackHelper.GetTypeMax(fieldType, "BitCountFromRange");

            if (min < minAllowedMin)
                throw new BitCountFromRangeException($"Min must be less than types min value, min:{min}, min allowed:{minAllowedMin}, type:{fieldType.Name}");

            if (max > maxAllowedMax)
                throw new BitCountFromRangeException($"Max must be greater than types max value, max:{max}, max allowed:{maxAllowedMax}, type:{fieldType.Name}");

            int bitCount = BitPackHelper.GetBitCount(checked((long)max - min));

            int? minResult;
            if (min == 0) minResult = null;
            else minResult = min;

            return new BitCountSerializer(bitCount, BitPackHelper.GetConvertType(fieldType), minResult);
        }
    }
}

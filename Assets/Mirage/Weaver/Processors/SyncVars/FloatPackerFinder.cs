using System;
using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal static class FloatPackFinder
    {
        public static FloatPackSettings? GetPackerSettings(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<FloatPackAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<float>())
            {
                throw new FloatPackException($"{syncVar.FieldType} is not a supported type for [FloatPack]", syncVar);
            }

            var settings = new FloatPackSettings();
            settings.max = (float)attribute.ConstructorArguments[0].Value;
            if (settings.max <= 0)
            {
                throw new FloatPackException($"Max must be above 0, max:{settings.max}", syncVar);
            }

            CustomAttributeArgument arg1 = attribute.ConstructorArguments[1];
            if (arg1.Type.Is<float>())
            {
                float precision = (float)arg1.Value;
                ValidatePrecision(syncVar, settings.max, precision, (s, m) => new FloatPackException(s, m));
                settings.precision = precision;
            }
            else
            {
                int bitCount = (int)arg1.Value;
                ValidateBitCount(syncVar, bitCount, (s, m) => new FloatPackException(s, m));
                settings.bitCount = bitCount;
            }

            return settings;
        }

        public static void ValidatePrecision<T>(FieldDefinition syncVar, float max, float precision, Func<string, MemberReference, T> CreateException) where T : WeaverException
        {
            if (precision < 0)
            {
                throw CreateException.Invoke($"Precsion must be positive, precision:{precision}", syncVar);
            }
            // todo is there a better way to check if Precsion is too small?
            double expectedBitCount = Math.Floor(Math.Log(2 * max / precision, 2)) + 1;
            if (expectedBitCount > 30)
            {
                throw CreateException.Invoke($"Precsion is too small, precision:{precision}", syncVar);
            }
        }
        public static void ValidateBitCount<T>(FieldDefinition syncVar, int bitCount, Func<string, MemberReference, T> CreateException) where T : WeaverException
        {
            if (bitCount > 30)
            {
                throw CreateException.Invoke($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}", syncVar);
            }
            if (bitCount < 1)
            {
                throw CreateException.Invoke($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}", syncVar);
            }
        }
    }
}

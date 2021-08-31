using System;
using Mirage.Serialization;
using Mono.Cecil;
using UnityEngine;

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

    internal static class Vector2Finder
    {
        public static Vector2PackSettings? GetPackerSettings(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<Vector2PackAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<Vector2>())
            {
                throw new Vector2PackException($"{syncVar.FieldType} is not a supported type for [Vector2Pack]", syncVar);
            }

            var settings = new Vector2PackSettings();
            for (int i = 0; i < 2; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector2PackException($"Max must be above 0, max:{settings.max}", syncVar);
                }
            }

            if (attribute.ConstructorArguments.Count == 3)
            {
                CustomAttributeArgument arg = attribute.ConstructorArguments[2];
                if (arg.Type.Is<float>())
                {
                    Precisionfrom1(syncVar, ref settings, arg);
                }
                else
                {
                    BitCountfrom1(syncVar, ref settings, arg);
                }
            }
            else
            {
                CustomAttributeArgument xArg = attribute.ConstructorArguments[2];
                CustomAttributeArgument yArg = attribute.ConstructorArguments[3];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom2(syncVar, ref settings, xArg, yArg);
                }
                else
                {
                    BitCountFrom2(syncVar, ref settings, xArg, yArg);
                }
            }

            return settings;
        }

        private static void Precisionfrom1(FieldDefinition syncVar, ref Vector2PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            float precision = (float)arg.Value;
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.x, precision, (s, m) => new Vector2PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.y, precision, (s, m) => new Vector2PackException(s, m));
            settings.precision = new Vector2(precision, precision);
        }
        private static void BitCountfrom1(FieldDefinition syncVar, ref Vector2PackSettings settings, CustomAttributeArgument arg)
        {
            int bitCount = (int)arg.Value;
            FloatPackFinder.ValidateBitCount(syncVar, bitCount, (s, m) => new Vector2PackException(s, m));
            settings.bitCount = new Vector2Int(bitCount, bitCount);
        }
        private static void PrecisionFrom2(FieldDefinition syncVar, ref Vector2PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg)
        {
            // check vs all 3 axis
            var precision = new Vector2(
                (float)xArg.Value,
                (float)yArg.Value);
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.x, precision.x, (s, m) => new Vector2PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.y, precision.y, (s, m) => new Vector2PackException(s, m));
            settings.precision = precision;
        }
        private static void BitCountFrom2(FieldDefinition syncVar, ref Vector2PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg)
        {
            // check vs all 3 axis
            FloatPackFinder.ValidateBitCount(syncVar, (int)xArg.Value, (s, m) => new Vector2PackException(s, m));
            FloatPackFinder.ValidateBitCount(syncVar, (int)yArg.Value, (s, m) => new Vector2PackException(s, m));
            settings.bitCount = new Vector2Int(
                (int)xArg.Value,
                (int)yArg.Value);
        }

    }

    internal static class Vector3Finder
    {
        public static Vector3PackSettings? GetPackerSettings(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<Vector3PackAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<Vector3>())
            {
                throw new Vector3PackException($"{syncVar.FieldType} is not a supported type for [Vector3Pack]", syncVar);
            }

            var settings = new Vector3PackSettings();
            for (int i = 0; i < 3; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector3PackException($"Max must be above 0, max:{settings.max}", syncVar);
                }
            }

            if (attribute.ConstructorArguments.Count == 4)
            {
                CustomAttributeArgument arg = attribute.ConstructorArguments[3];
                if (arg.Type.Is<float>())
                {
                    Precisionfrom1(syncVar, ref settings, arg);
                }
                else
                {
                    BitCountfrom1(syncVar, ref settings, arg);
                }
            }
            else
            {
                CustomAttributeArgument xArg = attribute.ConstructorArguments[3];
                CustomAttributeArgument yArg = attribute.ConstructorArguments[4];
                CustomAttributeArgument zArg = attribute.ConstructorArguments[5];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom3(syncVar, ref settings, xArg, yArg, zArg);
                }
                else
                {
                    BitCountFrom3(syncVar, ref settings, xArg, yArg, zArg);
                }
            }

            return settings;
        }

        private static void Precisionfrom1(FieldDefinition syncVar, ref Vector3PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            float precision = (float)arg.Value;
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.x, precision, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.y, precision, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.z, precision, (s, m) => new Vector3PackException(s, m));
            settings.precision = new Vector3(precision, precision, precision);
        }
        private static void BitCountfrom1(FieldDefinition syncVar, ref Vector3PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            var bitCount = (int)arg.Value;
            FloatPackFinder.ValidateBitCount(syncVar, bitCount, (s, m) => new Vector3PackException(s, m));
            settings.bitCount = new Vector3Int(bitCount, bitCount, bitCount);
        }
        private static void PrecisionFrom3(FieldDefinition syncVar, ref Vector3PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg, CustomAttributeArgument zArg)
        {
            // check vs all 3 axis
            var precision = new Vector3(
                (float)xArg.Value,
                (float)yArg.Value,
                (float)zArg.Value);
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.x, precision.x, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.y, precision.y, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidatePrecision(syncVar, settings.max.z, precision.z, (s, m) => new Vector3PackException(s, m));
            settings.precision = precision;
        }
        private static void BitCountFrom3(FieldDefinition syncVar, ref Vector3PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg, CustomAttributeArgument zArg)
        {
            // check vs all 3 axis
            FloatPackFinder.ValidateBitCount(syncVar, (int)xArg.Value, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidateBitCount(syncVar, (int)yArg.Value, (s, m) => new Vector3PackException(s, m));
            FloatPackFinder.ValidateBitCount(syncVar, (int)zArg.Value, (s, m) => new Vector3PackException(s, m));
            settings.bitCount = new Vector3Int(
                (int)xArg.Value,
                (int)yArg.Value,
                (int)zArg.Value);
        }
    }

    internal static class QuaternionFinder
    {
        public static int? GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<QuaternionPackAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<Quaternion>())
            {
                throw new QuaternionPackException($"{syncVar.FieldType} is not a supported type for [QuaternionPack]", syncVar);
            }

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new QuaternionPackException("BitCount should be above 0", syncVar);

            // no reason for 20, but seems higher than anyone should need
            if (bitCount > 20)
                throw new QuaternionPackException("BitCount should be below 20", syncVar);

            return bitCount;
        }
    }
}

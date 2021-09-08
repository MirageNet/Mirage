using Mirage.Serialization;
using Mono.Cecil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
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
}

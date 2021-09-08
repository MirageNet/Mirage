using Mirage.Serialization;
using Mono.Cecil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
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
            int bitCount = (int)arg.Value;
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
}

using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal static class Vector3Finder
    {
        public static ValueSerializer GetSerializer(FoundSyncVar syncVar)
        {
            FieldDefinition fieldDefinition = syncVar.FieldDefinition;
            CustomAttribute attribute = fieldDefinition.GetCustomAttribute<Vector3PackAttribute>();
            if (attribute == null)
                return default;

            if (!fieldDefinition.FieldType.Is<Vector3>())
            {
                throw new Vector3PackException($"{fieldDefinition.FieldType} is not a supported type for [Vector3Pack]", fieldDefinition);
            }

            var settings = new Vector3PackSettings();
            for (int i = 0; i < 3; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector3PackException($"Max must be above 0, max:{settings.max}", fieldDefinition);
                }
            }

            if (attribute.ConstructorArguments.Count == 4)
            {
                CustomAttributeArgument arg = attribute.ConstructorArguments[3];
                if (arg.Type.Is<float>())
                {
                    Precisionfrom1(fieldDefinition, ref settings, arg);
                }
                else
                {
                    BitCountfrom1(fieldDefinition, ref settings, arg);
                }
            }
            else
            {
                CustomAttributeArgument xArg = attribute.ConstructorArguments[3];
                CustomAttributeArgument yArg = attribute.ConstructorArguments[4];
                CustomAttributeArgument zArg = attribute.ConstructorArguments[5];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom3(fieldDefinition, ref settings, xArg, yArg, zArg);
                }
                else
                {
                    BitCountFrom3(fieldDefinition, ref settings, xArg, yArg, zArg);
                }
            }

            Expression<Action<Vector3Packer>> packMethod = (Vector3Packer p) => p.Pack(default, default);
            Expression<Action<Vector3Packer>> unpackMethod = (Vector3Packer p) => p.Unpack(default(NetworkReader));
            FieldDefinition packerField = CreatePackerField(syncVar, settings);

            return new PackerSerializer(packerField, packMethod, unpackMethod, false);
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

        private static FieldDefinition CreatePackerField(FoundSyncVar syncVar, Vector3PackSettings settings)
        {
            FieldDefinition packerField = syncVar.Behaviour.AddPackerField<Vector3Packer>(syncVar.FieldDefinition.Name);

            NetworkBehaviourProcessor.AddToStaticConstructor(syncVar.Behaviour.TypeDefinition, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.x));
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.y));
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.z));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.precision.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.y));
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.z));
                    packerCtor = syncVar.Module.ImportReference(() => new Vector3Packer(default(float), default(float), default(float), default(float), default(float), default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.y));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.z));
                    packerCtor = syncVar.Module.ImportReference(() => new Vector3Packer(default(float), default(float), default(float), default(int), default(int), default(int)));
                }
                else
                {
                    throw new InvalidOperationException($"Invalid Vector3PackSettings");
                }
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}

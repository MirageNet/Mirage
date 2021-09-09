using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal static class Vector2Finder
    {
        public static ValueSerializer GetSerializer(FoundSyncVar syncVar)
        {
            FieldDefinition fieldDefinition = syncVar.FieldDefinition;
            CustomAttribute attribute = fieldDefinition.GetCustomAttribute<Vector2PackAttribute>();
            if (attribute == null)
                return default;

            if (!fieldDefinition.FieldType.Is<Vector2>())
            {
                throw new Vector2PackException($"{fieldDefinition.FieldType} is not a supported type for [Vector2Pack]", fieldDefinition);
            }

            var settings = new Vector2PackSettings();
            for (int i = 0; i < 2; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector2PackException($"Max must be above 0, max:{settings.max}", fieldDefinition);
                }
            }

            if (attribute.ConstructorArguments.Count == 3)
            {
                CustomAttributeArgument arg = attribute.ConstructorArguments[2];
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
                CustomAttributeArgument xArg = attribute.ConstructorArguments[2];
                CustomAttributeArgument yArg = attribute.ConstructorArguments[3];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom2(fieldDefinition, ref settings, xArg, yArg);
                }
                else
                {
                    BitCountFrom2(fieldDefinition, ref settings, xArg, yArg);
                }
            }

            Expression<Action<Vector2Packer>> packMethod = (Vector2Packer p) => p.Pack(default, default);
            Expression<Action<Vector2Packer>> unpackMethod = (Vector2Packer p) => p.Unpack(default(NetworkReader));
            FieldDefinition packerField = CreatePackerField(syncVar, settings);

            return new PackerSerializer(packerField, packMethod, unpackMethod, false);
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

        private static FieldDefinition CreatePackerField(FoundSyncVar syncVar, Vector2PackSettings settings)
        {
            FieldDefinition packerField = syncVar.Behaviour.AddPackerField<Vector2Packer>(syncVar.FieldDefinition.Name);

            NetworkBehaviourProcessor.AddToStaticConstructor(syncVar.Behaviour.TypeDefinition, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.x));
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.y));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.precision.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.y));
                    packerCtor = syncVar.Module.ImportReference(() => new Vector2Packer(default(float), default(float), default(float), default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.y));
                    packerCtor = syncVar.Module.ImportReference(() => new Vector2Packer(default(float), default(float), default(int), default(int)));
                }
                else
                {
                    throw new InvalidOperationException($"Invalid Vector2PackSettings");
                }
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}

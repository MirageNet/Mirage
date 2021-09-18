using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.Serialization
{
    internal class Vector2Finder : PackerFinderBase<Vector2PackAttribute, Vector2Finder.Vector2PackSettings>
    {
        public struct Vector2PackSettings
        {
            public Vector2 max;
            public Vector2? precision;
            public Vector2Int? bitCount;
        }
        protected override bool IsIntType => false;
        protected override Vector2PackSettings GetSettings(TypeReference fieldType, CustomAttribute attribute)
        {
            if (!fieldType.Is<Vector2>())
            {
                throw new Vector2PackException($"{fieldType} is not a supported type for [Vector2Pack]");
            }

            var settings = new Vector2PackSettings();
            for (int i = 0; i < 2; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector2PackException($"Max must be above 0, max:{settings.max}");
                }
            }

            if (attribute.ConstructorArguments.Count == 3)
            {
                CustomAttributeArgument arg = attribute.ConstructorArguments[2];
                if (arg.Type.Is<float>())
                {
                    Precisionfrom1(ref settings, arg);
                }
                else
                {
                    BitCountfrom1(ref settings, arg);
                }
            }
            else
            {
                CustomAttributeArgument xArg = attribute.ConstructorArguments[2];
                CustomAttributeArgument yArg = attribute.ConstructorArguments[3];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom2(ref settings, xArg, yArg);
                }
                else
                {
                    BitCountFrom2(ref settings, xArg, yArg);
                }
            }

            return settings;
        }

        private static void Precisionfrom1(ref Vector2PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            float precision = (float)arg.Value;
            ValidatePrecision(settings.max.x, precision, (s) => new Vector2PackException(s));
            ValidatePrecision(settings.max.y, precision, (s) => new Vector2PackException(s));
            settings.precision = new Vector2(precision, precision);
        }
        private static void BitCountfrom1(ref Vector2PackSettings settings, CustomAttributeArgument arg)
        {
            int bitCount = (int)arg.Value;
            ValidateBitCount(bitCount, (s) => new Vector2PackException(s));
            settings.bitCount = new Vector2Int(bitCount, bitCount);
        }
        private static void PrecisionFrom2(ref Vector2PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg)
        {
            // check vs all 3 axis
            var precision = new Vector2(
                (float)xArg.Value,
                (float)yArg.Value);
            ValidatePrecision(settings.max.x, precision.x, (s) => new Vector2PackException(s));
            ValidatePrecision(settings.max.y, precision.y, (s) => new Vector2PackException(s));
            settings.precision = precision;
        }
        private static void BitCountFrom2(ref Vector2PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg)
        {
            // check vs all 3 axis
            ValidateBitCount((int)xArg.Value, (s) => new Vector2PackException(s));
            ValidateBitCount((int)yArg.Value, (s) => new Vector2PackException(s));
            settings.bitCount = new Vector2Int(
                (int)xArg.Value,
                (int)yArg.Value);
        }

        protected override LambdaExpression GetPackMethod(TypeReference fieldType)
        {
            Expression<Action<Vector2Packer>> packMethod = (Vector2Packer p) => p.Pack(default, default);
            return packMethod;
        }
        protected override LambdaExpression GetUnpackMethod(TypeReference fieldType)
        {
            Expression<Action<Vector2Packer>> unpackMethod = (Vector2Packer p) => p.Unpack(default(NetworkReader));
            return unpackMethod;
        }
        protected override FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, Vector2PackSettings settings)
        {
            FieldDefinition packerField = AddPackerField<Vector2Packer>(holder, fieldName);

            NetworkBehaviourProcessor.AddToStaticConstructor(holder, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.x));
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max.y));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.precision.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value.y));
                    packerCtor = module.ImportReference(() => new Vector2Packer(default(float), default(float), default(float), default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.y));
                    packerCtor = module.ImportReference(() => new Vector2Packer(default(float), default(float), default(int), default(int)));
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

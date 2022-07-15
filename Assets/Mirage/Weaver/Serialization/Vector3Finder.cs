using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.Serialization
{
    internal class Vector3Finder : PackerFinderBase<Vector3PackAttribute, Vector3Finder.Vector3PackSettings>
    {
        protected override bool IsIntType => false;

        public struct Vector3PackSettings
        {
            public Vector3 max;
            public Vector3? precision;
            public Vector3Int? bitCount;
        }

        protected override Vector3PackSettings GetSettings(TypeReference fieldType, CustomAttribute attribute)
        {
            if (!fieldType.Is<Vector3>())
            {
                throw new Vector3PackException($"{fieldType} is not a supported type for [Vector3Pack]");
            }

            var settings = new Vector3PackSettings();
            for (var i = 0; i < 3; i++)
            {
                settings.max[i] = (float)attribute.ConstructorArguments[i].Value;
                if (settings.max[i] <= 0)
                {
                    throw new Vector3PackException($"Max must be above 0, max:{settings.max}");
                }
            }

            if (attribute.ConstructorArguments.Count == 4)
            {
                var arg = attribute.ConstructorArguments[3];
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
                var xArg = attribute.ConstructorArguments[3];
                var yArg = attribute.ConstructorArguments[4];
                var zArg = attribute.ConstructorArguments[5];
                if (xArg.Type.Is<float>())
                {
                    PrecisionFrom3(ref settings, xArg, yArg, zArg);
                }
                else
                {
                    BitCountFrom3(ref settings, xArg, yArg, zArg);
                }
            }

            return settings;
        }

        private static void Precisionfrom1(ref Vector3PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            var precision = (float)arg.Value;
            ValidatePrecision(settings.max.x, precision, (s) => new Vector3PackException(s));
            ValidatePrecision(settings.max.y, precision, (s) => new Vector3PackException(s));
            ValidatePrecision(settings.max.z, precision, (s) => new Vector3PackException(s));
            settings.precision = new Vector3(precision, precision, precision);
        }
        private static void BitCountfrom1(ref Vector3PackSettings settings, CustomAttributeArgument arg)
        {
            // check vs all 3 axis
            var bitCount = (int)arg.Value;
            ValidateBitCount(bitCount, (s) => new Vector3PackException(s));
            settings.bitCount = new Vector3Int(bitCount, bitCount, bitCount);
        }
        private static void PrecisionFrom3(ref Vector3PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg, CustomAttributeArgument zArg)
        {
            // check vs all 3 axis
            var precision = new Vector3(
                (float)xArg.Value,
                (float)yArg.Value,
                (float)zArg.Value);
            ValidatePrecision(settings.max.x, precision.x, (s) => new Vector3PackException(s));
            ValidatePrecision(settings.max.y, precision.y, (s) => new Vector3PackException(s));
            ValidatePrecision(settings.max.z, precision.z, (s) => new Vector3PackException(s));
            settings.precision = precision;
        }
        private static void BitCountFrom3(ref Vector3PackSettings settings, CustomAttributeArgument xArg, CustomAttributeArgument yArg, CustomAttributeArgument zArg)
        {
            // check vs all 3 axis
            ValidateBitCount((int)xArg.Value, (s) => new Vector3PackException(s));
            ValidateBitCount((int)yArg.Value, (s) => new Vector3PackException(s));
            ValidateBitCount((int)zArg.Value, (s) => new Vector3PackException(s));
            settings.bitCount = new Vector3Int(
                (int)xArg.Value,
                (int)yArg.Value,
                (int)zArg.Value);
        }

        protected override LambdaExpression GetPackMethod(TypeReference fieldType)
        {
            Expression<Action<Vector3Packer>> packMethod = (Vector3Packer p) => p.Pack(default, default);
            return packMethod;
        }

        protected override LambdaExpression GetUnpackMethod(TypeReference fieldType)
        {
            Expression<Action<Vector3Packer>> unpackMethod = (Vector3Packer p) => p.Unpack(default(NetworkReader));
            return unpackMethod;
        }

        protected override FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, Vector3PackSettings settings)
        {
            var packerField = AddPackerField<Vector3Packer>(holder, fieldName);

            holder.AddToStaticConstructor((worker) =>
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
                    packerCtor = module.ImportReference(() => new Vector3Packer(default(float), default(float), default(float), default(float), default(float), default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.x));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.y));
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value.z));
                    packerCtor = module.ImportReference(() => new Vector3Packer(default(float), default(float), default(float), default(int), default(int), default(int)));
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

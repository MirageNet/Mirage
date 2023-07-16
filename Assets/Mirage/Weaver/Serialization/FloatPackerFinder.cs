using System;
using System.Linq.Expressions;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    internal class FloatPackFinder : PackerFinderBase<FloatPackAttribute, FloatPackFinder.FloatPackSettings>
    {
        protected override bool IsIntType => false;

        public struct FloatPackSettings
        {
            public float max;
            public float? precision;
            public int? bitCount;
        }

        protected override FloatPackSettings GetSettings(TypeReference fieldType, CustomAttribute attribute)
        {
            if (!fieldType.Is<float>())
            {
                throw new FloatPackException($"{fieldType} is not a supported type for [FloatPack]");
            }

            var settings = new FloatPackSettings();
            settings.max = (float)attribute.ConstructorArguments[0].Value;
            if (settings.max <= 0)
            {
                throw new FloatPackException($"Max must be above 0, max:{settings.max}");
            }

            var arg1 = attribute.ConstructorArguments[1];
            if (arg1.Type.Is<float>())
            {
                var precision = (float)arg1.Value;
                ValidatePrecision(settings.max, precision, (s) => new FloatPackException(s));
                settings.precision = precision;
            }
            else
            {
                var bitCount = (int)arg1.Value;
                ValidateBitCount(bitCount, (s) => new FloatPackException(s));
                settings.bitCount = bitCount;
            }

            return settings;
        }

        protected override LambdaExpression GetPackMethod(TypeReference fieldType)
        {
            Expression<Action<FloatPacker>> packMethod = (FloatPacker p) => p.Pack(default, default);
            return packMethod;
        }

        protected override LambdaExpression GetUnpackMethod(TypeReference fieldType)
        {
            Expression<Action<FloatPacker>> unpackMethod = (FloatPacker p) => p.Unpack(default(NetworkReader));
            return unpackMethod;
        }

        protected override FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, FloatPackSettings settings)
        {
            var packerField = AddPackerField<FloatPacker>(holder, fieldName);

            holder.AddToStaticConstructor((worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.precision.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value));
                    packerCtor = module.ImportReference(() => new FloatPacker(default, default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value));
                    packerCtor = module.ImportReference(() => new FloatPacker(default, default(int)));
                }
                else
                {
                    throw new InvalidOperationException($"Invalid FloatPackSettings");
                }
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}

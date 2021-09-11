using System;
using System.Linq.Expressions;
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

        public static void ValidatePrecision<T>(float max, float precision, Func<string, T> CreateException) where T : WeaverException
        {
            if (precision < 0)
            {
                throw CreateException.Invoke($"Precsion must be positive, precision:{precision}");
            }
            // todo is there a better way to check if Precsion is too small?
            double expectedBitCount = Math.Floor(Math.Log(2 * max / precision, 2)) + 1;
            if (expectedBitCount > 30)
            {
                throw CreateException.Invoke($"Precsion is too small, precision:{precision}");
            }
        }
        public static void ValidateBitCount<T>(int bitCount, Func<string, T> CreateException) where T : WeaverException
        {
            if (bitCount > 30)
            {
                throw CreateException.Invoke($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}");
            }
            if (bitCount < 1)
            {
                throw CreateException.Invoke($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}");
            }
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

            CustomAttributeArgument arg1 = attribute.ConstructorArguments[1];
            if (arg1.Type.Is<float>())
            {
                float precision = (float)arg1.Value;
                ValidatePrecision(settings.max, precision, (s) => new FloatPackException(s));
                settings.precision = precision;
            }
            else
            {
                int bitCount = (int)arg1.Value;
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
            FieldDefinition packerField = PackerSerializer.AddPackerField<FloatPacker>(holder, fieldName);

            NetworkBehaviourProcessor.AddToStaticConstructor(holder, (worker) =>
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

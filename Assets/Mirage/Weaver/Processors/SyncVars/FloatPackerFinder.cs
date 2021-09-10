using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal static class FloatPackFinder
    {
        struct FloatPackSettings
        {
            public float max;
            public float? precision;
            public int? bitCount;
        }

        public static ValueSerializer GetSerializer(FoundSyncVar syncVar)
        {
            FieldDefinition fieldDefinition = syncVar.FieldDefinition;
            CustomAttribute attribute = fieldDefinition.GetCustomAttribute<FloatPackAttribute>();
            if (attribute == null)
                return default;

            if (!fieldDefinition.FieldType.Is<float>())
            {
                throw new FloatPackException($"{fieldDefinition.FieldType} is not a supported type for [FloatPack]", fieldDefinition);
            }

            var settings = new FloatPackSettings();
            settings.max = (float)attribute.ConstructorArguments[0].Value;
            if (settings.max <= 0)
            {
                throw new FloatPackException($"Max must be above 0, max:{settings.max}", fieldDefinition);
            }

            CustomAttributeArgument arg1 = attribute.ConstructorArguments[1];
            if (arg1.Type.Is<float>())
            {
                float precision = (float)arg1.Value;
                ValidatePrecision(fieldDefinition, settings.max, precision, (s, m) => new FloatPackException(s, m));
                settings.precision = precision;
            }
            else
            {
                int bitCount = (int)arg1.Value;
                ValidateBitCount(fieldDefinition, bitCount, (s, m) => new FloatPackException(s, m));
                settings.bitCount = bitCount;
            }

            Expression<Action<FloatPacker>> packMethod = (FloatPacker p) => p.Pack(default, default);
            Expression<Action<FloatPacker>> unpackMethod = (FloatPacker p) => p.Unpack(default(NetworkReader));
            FieldDefinition packerField = CreatePackerField(syncVar, settings);

            return new PackerSerializer(packerField, packMethod, unpackMethod, false);
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


        private static FieldDefinition CreatePackerField(FoundSyncVar syncVar, FloatPackSettings settings)
        {
            FieldDefinition packerField = syncVar.Behaviour.AddPackerField<FloatPacker>(syncVar.FieldDefinition.Name);

            NetworkBehaviourProcessor.AddToStaticConstructor(syncVar.Behaviour.TypeDefinition, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_R4, settings.max));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.precision.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_R4, settings.precision.Value));
                    packerCtor = syncVar.Module.ImportReference(() => new FloatPacker(default, default(float)));
                }
                else if (settings.bitCount.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I4, settings.bitCount.Value));
                    packerCtor = syncVar.Module.ImportReference(() => new FloatPacker(default, default(int)));
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

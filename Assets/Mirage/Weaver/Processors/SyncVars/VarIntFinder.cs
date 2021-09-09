using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;


namespace Mirage.Weaver.SyncVars
{
    internal static class VarIntFinder
    {
        public static ValueSerializer GetSerializer(FoundSyncVar syncVar)
        {
            FieldDefinition fieldDefinition = syncVar.FieldDefinition;
            CustomAttribute attribute = fieldDefinition.GetCustomAttribute<VarIntAttribute>();
            if (attribute == null)
                return default;

            VarIntSettings settings = default;
            settings.small = (ulong)attribute.ConstructorArguments[0].Value;
            settings.medium = (ulong)attribute.ConstructorArguments[1].Value;
            if (attribute.ConstructorArguments.Count == 4)
            {
                settings.large = (ulong)attribute.ConstructorArguments[2].Value;
                settings.throwIfOverLarge = (bool)attribute.ConstructorArguments[3].Value;
            }
            else
            {
                settings.large = null;
            }


            if (settings.small <= 0)
                throw new VarIntException("Small value should be greater than 0", fieldDefinition);
            if (settings.medium <= 0)
                throw new VarIntException("Medium value should be greater than 0", fieldDefinition);
            if (settings.large.HasValue && settings.large.Value <= 0)
                throw new VarIntException("Large value should be greater than 0", fieldDefinition);

            int smallBits = BitPackHelper.GetBitCount(settings.small, 64);
            int mediumBits = BitPackHelper.GetBitCount(settings.medium, 64);
            int? largeBits = settings.large.HasValue ? BitPackHelper.GetBitCount(settings.large.Value, 64) : default(int?);

            if (smallBits >= mediumBits)
                throw new VarIntException("The small bit count should be less than medium bit count", fieldDefinition);
            if (largeBits.HasValue && mediumBits >= largeBits.Value)
                throw new VarIntException("The medium bit count should be less than large bit count", fieldDefinition);

            int maxBits = BitPackHelper.GetTypeMaxSize(fieldDefinition.FieldType, fieldDefinition, "VarInt");

            if (smallBits > maxBits)
                throw new VarIntException($"Small bit count can not be above target type size, bitCount:{smallBits}, max size:{maxBits}, type:{fieldDefinition.FieldType.Name}", fieldDefinition);
            if (mediumBits > maxBits)
                throw new VarIntException($"Medium bit count can not be above target type size, bitCount:{mediumBits}, max size:{maxBits}, type:{fieldDefinition.FieldType.Name}", fieldDefinition);
            if (largeBits.HasValue && largeBits.Value > maxBits)
                throw new VarIntException($"Large bit count can not be above target type size, bitCount:{largeBits.Value}, max size:{maxBits}, type:{fieldDefinition.FieldType.Name}", fieldDefinition);


            Expression<Action<VarIntPacker>> packMethod = GetPackMethod(fieldDefinition.FieldType, fieldDefinition);
            Expression<Action<VarIntPacker>> unpackMethod = GetUnpackMethod(fieldDefinition.FieldType, fieldDefinition);
            FieldDefinition packerField = CreatePackerField(syncVar, settings);
            return new PackerSerializer(packerField, packMethod, unpackMethod, true);
        }

        private static Expression<Action<VarIntPacker>> GetPackMethod(TypeReference type, FieldDefinition syncVar)
        {
            if (type.Is<byte>()
             || type.Is<short>()
             || type.Is<ushort>())
                return (VarIntPacker p) => p.PackUshort(default, default);

            if (type.Is<int>()
             || type.Is<uint>())
                return (VarIntPacker p) => p.PackUint(default, default);

            if (type.Is<long>()
             || type.Is<ulong>())
                return (VarIntPacker p) => p.PackUlong(default, default);

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetPackMethod(enumType, syncVar);
            }

            throw new VarIntException($"{type.FullName} is not a supported type for [VarInt]", syncVar);
        }

        private static Expression<Action<VarIntPacker>> GetUnpackMethod(TypeReference type, FieldDefinition syncVar)
        {
            if (type.Is<byte>()
             || type.Is<short>()
             || type.Is<ushort>())
                return (VarIntPacker p) => p.UnpackUshort(default);

            if (type.Is<int>()
             || type.Is<uint>())
                return (VarIntPacker p) => p.UnpackUint(default);

            if (type.Is<long>()
             || type.Is<ulong>())
                return (VarIntPacker p) => p.UnpackUlong(default);

            if (type.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                TypeReference enumType = type.Resolve().GetEnumUnderlyingType();
                return GetUnpackMethod(enumType, syncVar);
            }

            throw new VarIntException($"{type.FullName} is not a supported type for [VarInt]", syncVar);
        }

        private static FieldDefinition CreatePackerField(FoundSyncVar syncVar, VarIntSettings settings)
        {
            FieldDefinition packerField = syncVar.Behaviour.AddPackerField<VarIntPacker>(syncVar.FieldDefinition.Name);

            NetworkBehaviourProcessor.AddToStaticConstructor(syncVar.Behaviour.TypeDefinition, (worker) =>
            {
                // cast ulong to long so it can be passed to Create function
                worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.small));
                worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.medium));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.large.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.large.Value));
                    worker.Append(worker.Create(settings.throwIfOverLarge ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                    packerCtor = syncVar.Module.ImportReference(() => new VarIntPacker(default, default, default, default));
                }
                else
                {
                    packerCtor = syncVar.Module.ImportReference(() => new VarIntPacker(default, default));
                }
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}

using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;

namespace Mirage.Weaver.SyncVars
{
    internal static class VarIntFinder
    {
        public static VarIntSettings? GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<VarIntAttribute>();
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
                throw new VarIntException("Small value should be greater than 0", syncVar);
            if (settings.medium <= 0)
                throw new VarIntException("Medium value should be greater than 0", syncVar);
            if (settings.large.HasValue && settings.large.Value <= 0)
                throw new VarIntException("Large value should be greater than 0", syncVar);

            int smallBits = BitPackHelper.GetBitCount(settings.small, 64);
            int mediumBits = BitPackHelper.GetBitCount(settings.medium, 64);
            int? largeBits = settings.large.HasValue ? BitPackHelper.GetBitCount(settings.large.Value, 64) : default(int?);

            if (smallBits >= mediumBits)
                throw new VarIntException("The small bit count should be less than medium bit count", syncVar);
            if (largeBits.HasValue && mediumBits >= largeBits.Value)
                throw new VarIntException("The medium bit count should be less than large bit count", syncVar);

            int maxBits = BitPackHelper.GetTypeMaxSize(syncVar.FieldType, syncVar, "VarInt");

            if (smallBits > maxBits)
                throw new VarIntException($"Small bit count can not be above target type size, bitCount:{smallBits}, max size:{maxBits}, type:{syncVar.FieldType.Name}", syncVar);
            if (mediumBits > maxBits)
                throw new VarIntException($"Medium bit count can not be above target type size, bitCount:{mediumBits}, max size:{maxBits}, type:{syncVar.FieldType.Name}", syncVar);
            if (largeBits.HasValue && largeBits.Value > maxBits)
                throw new VarIntException($"Large bit count can not be above target type size, bitCount:{largeBits.Value}, max size:{maxBits}, type:{syncVar.FieldType.Name}", syncVar);


            settings.packMethod = GetPackMethod(syncVar.FieldType, syncVar);
            settings.unpackMethod = GetUnpackMethod(syncVar.FieldType, syncVar);
            return settings;
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
    }
}

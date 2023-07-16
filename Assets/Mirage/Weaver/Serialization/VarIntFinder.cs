using System;
using System.Linq.Expressions;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver.Serialization
{
    internal class VarIntFinder : PackerFinderBase<VarIntAttribute, VarIntFinder.VarIntSettings>
    {
        public struct VarIntSettings
        {
            public ulong small;
            public ulong medium;
            public ulong? large;
            public bool throwIfOverLarge;
        }

        protected override bool IsIntType => true;

        protected override VarIntSettings GetSettings(TypeReference fieldType, CustomAttribute attribute)
        {
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
                throw new VarIntException("Small value should be greater than 0");
            if (settings.medium <= 0)
                throw new VarIntException("Medium value should be greater than 0");
            if (settings.large.HasValue && settings.large.Value <= 0)
                throw new VarIntException("Large value should be greater than 0");

            var smallBits = BitPackHelper.GetBitCount(settings.small, 64);
            var mediumBits = BitPackHelper.GetBitCount(settings.medium, 64);
            var largeBits = settings.large.HasValue ? BitPackHelper.GetBitCount(settings.large.Value, 64) : default(int?);

            if (smallBits >= mediumBits)
                throw new VarIntException("The small bit count should be less than medium bit count");
            if (largeBits.HasValue && mediumBits >= largeBits.Value)
                throw new VarIntException("The medium bit count should be less than large bit count");

            var maxBits = BitPackHelper.GetTypeMaxSize(fieldType, "VarInt");

            if (smallBits > maxBits)
                throw new VarIntException($"Small bit count can not be above target type size, bitCount:{smallBits}, max size:{maxBits}, type:{fieldType.Name}");
            if (mediumBits > maxBits)
                throw new VarIntException($"Medium bit count can not be above target type size, bitCount:{mediumBits}, max size:{maxBits}, type:{fieldType.Name}");
            if (largeBits.HasValue && largeBits.Value > maxBits)
                throw new VarIntException($"Large bit count can not be above target type size, bitCount:{largeBits.Value}, max size:{maxBits}, type:{fieldType.Name}");

            return settings;
        }

        protected override LambdaExpression GetPackMethod(TypeReference fieldType)
        {
            if (fieldType.Is<byte>()
             || fieldType.Is<short>()
             || fieldType.Is<ushort>())
            {
                Expression<Action<VarIntPacker>> pack = (VarIntPacker p) => p.PackUshort(default, default);
                return pack;
            }

            if (fieldType.Is<int>()
             || fieldType.Is<uint>())
            {
                Expression<Action<VarIntPacker>> pack = (VarIntPacker p) => p.PackUint(default, default);
                return pack;
            }

            if (fieldType.Is<long>()
             || fieldType.Is<ulong>())
            {
                Expression<Action<VarIntPacker>> pack = (VarIntPacker p) => p.PackUlong(default, default);
                return pack;
            }

            if (fieldType.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                var enumType = fieldType.Resolve().GetEnumUnderlyingType();
                return GetPackMethod(enumType);
            }

            throw new VarIntException($"{fieldType.FullName} is not a supported type for [VarInt]");
        }

        protected override LambdaExpression GetUnpackMethod(TypeReference fieldType)
        {
            if (fieldType.Is<byte>()
             || fieldType.Is<short>()
             || fieldType.Is<ushort>())
            {
                Expression<Action<VarIntPacker>> unpack = (VarIntPacker p) => p.UnpackUshort(default);
                return unpack;
            }

            if (fieldType.Is<int>()
             || fieldType.Is<uint>())
            {
                Expression<Action<VarIntPacker>> unpack = (VarIntPacker p) => p.UnpackUint(default);
                return unpack;
            }

            if (fieldType.Is<long>()
             || fieldType.Is<ulong>())
            {
                Expression<Action<VarIntPacker>> unpack = (VarIntPacker p) => p.UnpackUlong(default);
                return unpack;
            }


            if (fieldType.Resolve().IsEnum)
            {
                // use underlying enum type for max size
                var enumType = fieldType.Resolve().GetEnumUnderlyingType();
                return GetUnpackMethod(enumType);
            }

            throw new VarIntException($"{fieldType.FullName} is not a supported type for [VarInt]");
        }

        protected override FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, VarIntSettings settings)
        {
            var packerField = AddPackerField<VarIntPacker>(holder, fieldName);

            holder.AddToStaticConstructor((worker) =>
            {
                // cast ulong to long so it can be passed to Create function
                worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.small));
                worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.medium));

                // packer has 2 constructors, get the one that matches the attribute type
                MethodReference packerCtor = null;
                if (settings.large.HasValue)
                {
                    worker.Append(worker.Create(OpCodes.Ldc_I8, (long)settings.large.Value));
                    worker.Append(worker.Create(settings.throwIfOverLarge.OpCode_Ldc()));
                    packerCtor = module.ImportReference(() => new VarIntPacker(default, default, default, default));
                }
                else
                {
                    packerCtor = module.ImportReference(() => new VarIntPacker(default, default));
                }
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}

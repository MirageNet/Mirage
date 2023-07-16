using System;
using System.Linq;
using System.Linq.Expressions;
using Mirage.CodeGen;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal abstract class PackerFinderBase<TAttribute, TSettings>
    {
        public ValueSerializer GetSerializer(ModuleDefinition module, TypeDefinition holder, ICustomAttributeProvider attributeProvider, TypeReference fieldType, string fieldName)
        {
            var attribute = attributeProvider.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return default;

            var settings = GetSettings(fieldType, attribute);
            var packMethod = GetPackMethod(fieldType);
            var unpackMethod = GetUnpackMethod(fieldType);
            // field might be created by another finder, so we can re-use it
            if (!TryGetPackerField(holder, fieldName, out var packerField))
            {
                packerField = CreatePackerField(module, fieldName, holder, settings);
            }

            return new PackerSerializer(packerField, packMethod, unpackMethod, IsIntType);
        }

        protected abstract bool IsIntType { get; }
        protected abstract TSettings GetSettings(TypeReference fieldType, CustomAttribute attribute);
        protected abstract LambdaExpression GetPackMethod(TypeReference fieldType);
        protected abstract LambdaExpression GetUnpackMethod(TypeReference fieldType);
        protected abstract FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, TSettings settings);

        public static bool TryGetPackerField(TypeDefinition typeDefinition, string name, out FieldDefinition fieldDefinition)
        {
            fieldDefinition = typeDefinition.Fields.FirstOrDefault(x => x.Name == $"{name}__Packer");
            return fieldDefinition != null;
        }

        public static FieldDefinition AddPackerField<T>(TypeDefinition typeDefinition, string name)
        {
            return typeDefinition.AddField<T>($"{name}__Packer", FieldAttributes.Assembly | FieldAttributes.Static);
        }

        public static void ValidatePrecision<TException>(float max, float precision, Func<string, TException> CreateException) where TException : WeaverException
        {
            if (precision < 0)
            {
                throw CreateException.Invoke($"Precsion must be positive, precision:{precision}");
            }

            var expectedBitCount = Math.Floor(Math.Log(2 * max / (double)precision, 2)) + 1;
            // 30 should be large enough, if someone is trying to use more they might as well just send the whole float
            if (expectedBitCount > 30)
            {
                throw CreateException.Invoke($"Precsion is too small, precision:{precision}");
            }
        }
        public static void ValidateBitCount<TException>(int bitCount, Func<string, TException> CreateException) where TException : WeaverException
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
    }
}

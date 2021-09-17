using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal abstract class PackerFinderBase<TAttribute, TSettings>
    {
        public ValueSerializer GetSerializer(ModuleDefinition module, TypeDefinition holder, ICustomAttributeProvider attributeProvider, TypeReference fieldType, string fieldName)
        {
            CustomAttribute attribute = attributeProvider.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return default;

            TSettings settings = GetSettings(fieldType, attribute);
            LambdaExpression packMethod = GetPackMethod(fieldType);
            LambdaExpression unpackMethod = GetUnpackMethod(fieldType);
            // field might be created by another finder, so we can re-use it
            if (!TryGetPackerField(holder, fieldName, out FieldDefinition packerField))
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
    }
}

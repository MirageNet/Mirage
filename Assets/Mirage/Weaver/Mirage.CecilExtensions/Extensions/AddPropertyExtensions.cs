using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.CodeGen
{
    public static class AddPropertyExtensions
    {
        public const MethodAttributes DEFAULT_PROPERTY_METHOD_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;

        public static PropertyDefinition AddProperty<T>(this TypeDefinition typeDefinition, string name, PropertyAttributes attributes = PropertyAttributes.None)
            => AddProperty(typeDefinition, name, typeof(T), attributes);

        public static PropertyDefinition AddProperty(this TypeDefinition typeDefinition, string name, Type type, PropertyAttributes attributes = PropertyAttributes.None)
            => AddProperty(typeDefinition, name, typeDefinition.Module.ImportReference(type), attributes);

        public static PropertyDefinition AddProperty(this TypeDefinition typeDefinition, string name, TypeReference propertyType, PropertyAttributes attributes = PropertyAttributes.None)
        {
            var property = new PropertyDefinition(name, attributes, propertyType);
            property.DeclaringType = typeDefinition;
            typeDefinition.Properties.Add(property);
            return property;
        }

        public static FieldDefinition AddBackingField(this PropertyDefinition propertyDefinition, bool addGet, bool addSet)
        {
            var declaringType = propertyDefinition.DeclaringType;

            const FieldAttributes attributes = FieldAttributes.Private;
            var field = declaringType.AddField($"<{propertyDefinition.Name}>k__BackingField", propertyDefinition.PropertyType, attributes);

            if (addGet)
            {
                var get = AddGetMethod(propertyDefinition);
                get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
                get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            if (addSet)
            {
                var set = AddSetMethod(propertyDefinition);
                set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                set.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                set.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, field));
                set.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }

            return field;
        }

        public static MethodDefinition AddGetMethod(this PropertyDefinition propertyDefinition, MethodAttributes methodAttributes = DEFAULT_PROPERTY_METHOD_ATTRIBUTES)
        {
            var declaringType = propertyDefinition.DeclaringType;

            var get = declaringType.AddMethod($"get_{propertyDefinition.Name}", methodAttributes, propertyDefinition.PropertyType);
            var body = new MethodBody(get);
            get.Body = body;

            propertyDefinition.GetMethod = get;
            return get;
        }

        public static MethodDefinition AddSetMethod(this PropertyDefinition propertyDefinition, MethodAttributes methodAttributes = DEFAULT_PROPERTY_METHOD_ATTRIBUTES)
        {
            var declaringType = propertyDefinition.DeclaringType;

            var set = declaringType.AddMethod($"set_{propertyDefinition.Name}", methodAttributes);
            set.AddParam(propertyDefinition.PropertyType, "value");

            var body = new MethodBody(set);
            set.Body = body;

            propertyDefinition.SetMethod = set;
            return set;
        }
    }
}

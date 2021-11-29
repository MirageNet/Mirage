using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver
{
    /// <summary>
    /// convenience methods for type definitions
    /// </summary>
    public static class TypeExtensions
    {

        public static MethodDefinition GetMethod(this TypeDefinition td, string methodName)
        {
            // Linq allocations don't matter in weaver
            return td.Methods.FirstOrDefault(method => method.Name == methodName);
        }

        public static MethodDefinition[] GetMethods(this TypeDefinition td, string methodName)
        {
            // Linq allocations don't matter in weaver
            return td.Methods.Where(method => method.Name == methodName).ToArray();
        }

        /// <summary>
        /// Finds a method in base type
        /// <para>
        /// IMPORTANT: dont resolve <paramref name="typeReference"/> before calling this or methods can not be made into generic methods
        /// </para>
        /// </summary>
        /// <param name="typeReference">Unresolved type reference, dont resolve if generic</param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodReference GetMethodInBaseType(this TypeReference typeReference, string methodName)
        {
            TypeDefinition typedef = typeReference.Resolve();
            TypeReference typeRef = typeReference;
            while (typedef != null)
            {
                foreach (MethodDefinition md in typedef.Methods)
                {
                    if (md.Name == methodName)
                    {
                        MethodReference method = md;
                        if (typeRef.IsGenericInstance)
                        {
                            // use in reference here to make method generic
                            var baseTypeInstance = (GenericInstanceType)typeReference;
                            method = method.MakeHostInstanceGeneric(baseTypeInstance);
                        }

                        return method;
                    }
                }

                try
                {
                    TypeReference parent = typedef.BaseType;
                    typeRef = parent;
                    typedef = parent?.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    // this can happen for plugins.
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds public fields in type and base type
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeReference variable)
        {
            return FindAllPublicFields(variable.Resolve());
        }

        /// <summary>
        /// Finds public fields in type and base type
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeDefinition typeDefinition)
        {
            while (typeDefinition != null)
            {
                foreach (FieldDefinition field in typeDefinition.Fields.ToArray())
                {
                    if (field.IsStatic || field.IsPrivate)
                        continue;

                    if (field.IsNotSerialized)
                        continue;

                    yield return field;
                }

                try
                {
                    typeDefinition = typeDefinition.BaseType?.Resolve();
                }
                catch
                {
                    break;
                }
            }
        }

        public static TypeDefinition AddType(this TypeDefinition typeDefinition, string name, TypeAttributes typeAttributes, bool valueType) =>
            AddType(typeDefinition, name, typeAttributes, valueType ? typeDefinition.Module.ImportReference(typeof(ValueType)) : null);
        public static TypeDefinition AddType(this TypeDefinition typeDefinition, string name, TypeAttributes typeAttributes, TypeReference baseType)
        {
            var type = new TypeDefinition("", name, typeAttributes, baseType)
            {
                DeclaringType = typeDefinition
            };
            typeDefinition.NestedTypes.Add(type);
            return type;
        }

        public static MethodDefinition AddMethod(this TypeDefinition typeDefinition, string name, MethodAttributes attributes, TypeReference returnType)
        {
            var method = new MethodDefinition(name, attributes, returnType);
            typeDefinition.Methods.Add(method);
            return method;
        }

        public static MethodDefinition AddMethod(this TypeDefinition typeDefinition, string name, MethodAttributes attributes) =>
            AddMethod(typeDefinition, name, attributes, typeDefinition.Module.ImportReference(typeof(void)));

        public static FieldDefinition AddField<T>(this TypeDefinition typeDefinition, string name, FieldAttributes attributes) =>
            AddField(typeDefinition, typeDefinition.Module.ImportReference(typeof(T)), name, attributes);

        public static FieldDefinition AddField(this TypeDefinition typeDefinition, TypeReference fieldType, string name, FieldAttributes attributes)
        {
            var field = new FieldDefinition(name, attributes, fieldType);
            field.DeclaringType = typeDefinition;
            typeDefinition.Fields.Add(field);
            return field;
        }

        /// <summary>
        /// Creates a generic type out of another type, if needed.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeReference ConvertToGenericIfNeeded(this TypeDefinition type)
        {
            if (type.HasGenericParameters)
            {
                // get all the generic parameters and make a generic instance out of it
                var genericTypes = new TypeReference[type.GenericParameters.Count];
                for (int i = 0; i < type.GenericParameters.Count; i++)
                {
                    genericTypes[i] = type.GenericParameters[i].GetElementType();
                }

                return type.MakeGenericInstanceType(genericTypes);
            }
            else
            {
                return type;
            }
        }

        public static FieldReference GetField(this TypeDefinition type, string fieldName)
        {
            if (type.HasFields)
            {
                for (int i = 0; i < type.Fields.Count; i++)
                {
                    if (type.Fields[i].Name == fieldName)
                    {
                        return type.Fields[i];
                    }
                }
            }

            return null;
        }
    }
}

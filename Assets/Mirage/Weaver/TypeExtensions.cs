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
            return GetMethodInBaseType(typeReference, (md) => md.Name == methodName);
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
        public static MethodReference GetMethodInBaseType(this TypeReference typeReference, Predicate<MethodDefinition> match)
        {
            TypeDefinition typedef = typeReference.Resolve();
            TypeReference typeRef = typeReference;
            while (typedef != null)
            {
                foreach (MethodDefinition md in typedef.Methods)
                {
                    if (match.Invoke(md))
                    {
                        MethodReference method = md;
                        if (typeRef.IsGenericInstance)
                        {
                            var generic = (GenericInstanceType)typeRef;
                            method = method.MakeHostInstanceGeneric(generic);
                        }

                        return method;
                    }
                }

                try
                {
                    TypeReference parent = typedef.BaseType;
                    if (parent.IsGenericInstance)
                    {
                        parent = MatchGenericParameters((GenericInstanceType)parent, typeRef);
                    }
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
        /// Takes generic argments from child class and applies them to base class
        /// <br/>
        /// eg makes `Base{T}` in <c>Child{int} : Base{int}</c> have `int` instead of `T`
        /// </summary>
        /// <param name="parentReference"></param>
        /// <param name="childReference"></param>
        /// <returns></returns>
        public static GenericInstanceType MatchGenericParameters(this GenericInstanceType parentReference, TypeReference childReference)
        {
            if (!parentReference.IsGenericInstance)
                throw new InvalidOperationException("Can't make non generic type into generic");

            // make new type so we can replace the args on it
            // resolve it so we have non-generic instance (eg just instance with <T> instead of <int>)
            // if we dont cecil will make it double generic (eg INVALID IL)
            var generic = new GenericInstanceType(parentReference.Resolve());
            foreach (TypeReference arg in parentReference.GenericArguments)
                generic.GenericArguments.Add(arg);

            for (int i = 0; i < generic.GenericArguments.Count; i++)
            {
                // if arg is not generic
                // eg List<int> would be int so not generic.
                // But List<T> would be T so is generic
                if (!generic.GenericArguments[i].IsGenericParameter)
                    continue;

                // get the generic name, eg T
                string name = generic.GenericArguments[i].Name;
                // find what type T is, eg turn it into `int` if `List<int>`
                TypeReference arg = FindMatchingGenericArgument(childReference, name);

                // import just to be safe
                TypeReference imported = parentReference.Module.ImportReference(arg);
                // set arg on generic, parent ref will be Base<int> instead of just Base<T>
                generic.GenericArguments[i] = imported;
            }

            return generic;

        }
        static TypeReference FindMatchingGenericArgument(TypeReference childReference, string paramName)
        {
            TypeDefinition def = childReference.Resolve();
            // child class must be generic if we are in this part of the code
            // eg Child<T> : Base<T>  <--- child must have generic if Base has T
            // vs Child : Base<int> <--- wont be here if Base has int (we check if T exists before calling this)
            if (!def.HasGenericParameters)
                throw new InvalidOperationException("Base class had generic parameters, but could not find them in child class");

            // go through parameters in child class, and find the generic that matches the name
            for (int i = 0; i < def.GenericParameters.Count; i++)
            {
                GenericParameter param = def.GenericParameters[i];
                if (param.Name == paramName)
                {
                    var generic = (GenericInstanceType)childReference;
                    // return generic arg with same index
                    return generic.GenericArguments[i];
                }
            }

            // this should never happen, if it does it means that this code is bugged
            throw new InvalidOperationException("Did not find matching generic");
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

        public static MethodDefinition AddMethod(this TypeDefinition typeDefinition, string name, MethodAttributes attributes, Type returnType)
        => typeDefinition.AddMethod(name, attributes, typeDefinition.Module.ImportReference(returnType));

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

        public static FieldDefinition GetField(this TypeDefinition type, string fieldName)
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

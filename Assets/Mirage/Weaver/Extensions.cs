using System;
using System.Linq;
using Mono.Cecil;

namespace Mirage.Weaver
{
    public static class Extensions
    {
        public static bool Is(this TypeReference td, Type t)
        {
            if (t.IsGenericType)
            {
                return td.GetElementType().FullName == t.FullName;
            }
            return td.FullName == t.FullName;
        }

        public static bool Is<T>(this TypeReference td) => Is(td, typeof(T));

        public static bool Is(this MethodReference method, Type t, string name) =>
            method.DeclaringType.Is(t) && method.Name == name;

        public static bool Is<T>(this MethodReference method, string name) =>
            method.DeclaringType.Is<T>() && method.Name == name;

        public static bool IsDerivedFrom<T>(this TypeDefinition td) => IsDerivedFrom(td, typeof(T));

        public static bool IsDerivedFrom(this TypeDefinition td, Type baseClass)
        {
            if (td == null)
                return false;

            if (!td.IsClass)
                return false;

            // are ANY parent classes of baseClass?
            TypeReference parent = td.BaseType;

            if (parent == null)
                return false;

            if (parent.Is(baseClass))
                return true;

            if (parent.CanBeResolved())
                return IsDerivedFrom(parent.Resolve(), baseClass);

            return false;
        }

        /// <summary>
        /// Resolves type using try/catch check
        /// Replacement for <see cref="CanBeResolved(TypeReference)"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeDefinition TryResolve(this TypeReference type)
        {
            if (type.Scope.Name == "Windows")
            {
                return null;
            }

            try
            {
                return type.Resolve();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Uses <see cref="TryResolve(TypeReference)"/> to find the Base Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeReference TryResolveParent(this TypeReference type)
        {
            return type.TryResolve()?.BaseType;
        }

        // set the value of a constant in a class
        public static void SetConst<T>(this TypeDefinition td, string fieldName, T value) where T : struct
        {
            FieldDefinition field = td.Fields.FirstOrDefault(f => f.Name == fieldName);

            if (field == null)
            {
                field = new FieldDefinition(fieldName, FieldAttributes.Literal | FieldAttributes.NotSerialized | FieldAttributes.Private, td.Module.ImportReference<T>());
                td.Fields.Add(field);
            }

            field.Constant = value;
        }

        public static T GetConst<T>(this TypeDefinition td, string fieldName) where T : struct
        {
            FieldDefinition field = td.Fields.FirstOrDefault(f => f.Name == fieldName);

            if (field == null)
            {
                return default;
            }

            var value = field.Constant as T?;

            return value.GetValueOrDefault();
        }

        public static TypeReference GetEnumUnderlyingType(this TypeDefinition td)
        {
            foreach (FieldDefinition field in td.Fields)
            {
                if (!field.IsStatic)
                    return field.FieldType;
            }
            throw new ArgumentException($"Invalid enum {td.FullName}");
        }

        public static bool ImplementsInterface<TInterface>(this TypeDefinition td)
        {
            if (td == null)
                return false;

            if (td.Is<TInterface>())
                return true;

            TypeDefinition typedef = td;

            while (typedef != null)
            {
                foreach (InterfaceImplementation iface in typedef.Interfaces)
                {
                    if (iface.InterfaceType.Is<TInterface>())
                        return true;
                }

                try
                {
                    TypeReference parent = typedef.BaseType;
                    typedef = parent?.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    // this can happen for pluins.
                    break;
                }
            }

            return false;
        }

        public static bool IsMultidimensionalArray(this TypeReference tr)
        {
            return tr is ArrayType arrayType && arrayType.Rank > 1;
        }

        public static bool CanBeResolved(this TypeReference parent)
        {
            while (parent != null)
            {
                if (parent.Scope.Name == "Windows")
                {
                    return false;
                }

                if (parent.Scope.Name == "mscorlib")
                {
                    TypeDefinition resolved = parent.Resolve();
                    return resolved != null;
                }

                try
                {
                    parent = parent.Resolve().BaseType;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Given a method of a generic class such as ArraySegment`T.get_Count,
        /// and a generic instance such as ArraySegment`int
        /// Creates a reference to the specialized method  ArraySegment`int`.get_Count
        /// <para> Note that calling ArraySegment`T.get_Count directly gives an invalid IL error </para>
        /// </summary>
        /// <param name="self"></param>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, GenericInstanceType instanceType)
        {
            var reference = new MethodReference(self.Name, self.ReturnType, instanceType)
            {
                CallingConvention = self.CallingConvention,
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis
            };

            foreach (ParameterDefinition parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (GenericParameter generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return self.Module.ImportReference(reference);
        }

        public static MethodReference MakeHostInstanceSelfGeneric(this MethodReference self)
        {
            TypeReference type = self.DeclaringType;
            if (!type.HasGenericParameters)
            {
                // if type isn't generic we dont need to do anything
                return self;
            }
            else
            {
                var genericType = (GenericInstanceType)type.MakeSelfGeneric();
                return self.MakeHostInstanceGeneric(genericType);
            }
        }

        public static TypeReference MakeSelfGeneric(this TypeReference self)
        {
            if (!self.HasGenericParameters)
            {
                // if type isn't generic we dont need to do anything
                return self;
            }
            else
            {
                // make generic instance of type, and give it the generic params as args
                var genericType = new GenericInstanceType(self);
                foreach (GenericParameter param in self.GenericParameters)
                    genericType.GenericArguments.Add(param);

                return genericType;
            }
        }

        public static bool TryGetCustomAttribute<TAttribute>(this ICustomAttributeProvider method, out CustomAttribute customAttribute)
        {
            foreach (CustomAttribute ca in method.CustomAttributes)
            {
                if (ca.AttributeType.Is<TAttribute>())
                {
                    customAttribute = ca;
                    return true;
                }
            }

            customAttribute = null;
            return false;
        }

        public static CustomAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider method)
        {
            _ = method.TryGetCustomAttribute<TAttribute>(out CustomAttribute customAttribute);
            return customAttribute;
        }

        public static bool HasCustomAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider)
        {
            return HasCustomAttribute(attributeProvider, typeof(TAttribute));
        }

        public static bool HasCustomAttribute(this ICustomAttributeProvider attributeProvider, Type t)
        {
            return attributeProvider.CustomAttributes.Any(attr => attr.AttributeType.Is(t));
        }

        public static T GetField<T>(this CustomAttribute ca, string field, T defaultValue)
        {
            foreach (CustomAttributeNamedArgument customField in ca.Fields)
            {
                if (customField.Name == field)
                {
                    return (T)customField.Argument.Value;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Imports a field and makes it a member of its orignal type.
        /// <para>This is needed if orignal type is a generic instance, this will ensure that it stays a member of that instance, eg MyMessage{int}.Value</para>
        /// </summary>
        /// <param name="module"></param>
        /// <param name="field"></param>
        /// <param name="orignalType">Type that the field orignal belonged to, NOT the resolved type</param>
        /// <returns></returns>
        public static FieldReference ImportField(this ModuleDefinition module, FieldDefinition field, TypeReference orignalType)
        {
            if (orignalType.Module != module)
                orignalType = module.ImportReference(orignalType);

            TypeReference fieldType = module.ImportReference(field.FieldType);
            return new FieldReference(field.Name, fieldType, orignalType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="orignalType">make sure orignalType is already imported</param>
        /// <returns></returns>
        public static TypeReference GetFieldTypeIncludingGeneric(this FieldDefinition field, TypeReference orignalType)
        {
            // if generic, then check if it has a type from orignalType 
            if (field.FieldType.IsGenericParameter && orignalType.IsGenericInstance)
            {
                if (FindGenericArgmentWithMatchingName(field.FieldType, orignalType, out TypeReference found))
                    return found;
            }

            // if not generic, or no matching found just return its type
            return field.FieldType;
        }

        private static bool FindGenericArgmentWithMatchingName(TypeReference genericParameter, TypeReference orignalType, out TypeReference found)
        {
            // resolve to get GenericParameters
            TypeDefinition resolved = orignalType.Resolve();

            string typeName = genericParameter.Name;
            for (int i = 0; i < resolved.GenericParameters.Count; i++)
            {
                GenericParameter param = resolved.GenericParameters[i];
                if (param.Name == typeName)
                {
                    var generic = (GenericInstanceType)orignalType;
                    found = generic.GenericArguments[i];
                    return true;
                }
            }

            found = null;
            return false;
        }

        /// <summary>
        /// Makes a field part of generic defintion
        /// <para>
        /// NOTE: this only works when you need the type to be part of a generic defintion, NOT a generic instance, eg member of List{T} works, but List{int} doesn't
        /// </para>
        /// </summary>
        /// <param name="fd"></param>
        /// <returns></returns>
        public static FieldReference MakeHostGenericIfNeeded(this FieldReference fd)
        {
            if (fd.DeclaringType.HasGenericParameters)
            {
                return new FieldReference(fd.Name, fd.FieldType, fd.DeclaringType.Resolve().ConvertToGenericIfNeeded());
            }

            return fd;
        }
    }
}

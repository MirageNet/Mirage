using System;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    internal static class ValueSerializerFinder
    {
        /// <exception cref="ValueSerializerException">Throws when attribute is used incorrectly</exception>
        /// <exception cref="SerializeFunctionException">Throws when can not generate read or write function</exception>
        public static ValueSerializer GetSerializer(FoundSyncVar syncVar, Writers writers, Readers readers)
        {
            return GetSerializer(syncVar.Module, syncVar.Behaviour.TypeDefinition, syncVar.FieldDefinition, syncVar.FieldDefinition.FieldType, syncVar.FieldDefinition.Name, writers, readers);
        }

        /// <exception cref="ValueSerializerException">Throws when attribute is used incorrectly</exception>
        /// <exception cref="SerializeFunctionException">Throws when can not generate read or write function</exception>
        public static ValueSerializer GetSerializer(ModuleDefinition module, FieldReference field, TypeReference fieldType, Writers writers, Readers readers)
        {
            // note: we have to `Resolve()` DeclaringType first, because imported referencev `Module` will be equal.
            var holder = field.DeclaringType.Resolve();
            var name = field.Name;

            // if field is in this module use its type for Packer field,
            // else use the generated class
            if (holder.Module != module)
            {
                holder = module.GeneratedClass();
                name = $"{field.DeclaringType.FullName}_{field.Name}";
            }

            return GetSerializer(module, holder, field.Resolve(), fieldType, name, writers, readers);
        }

        /// <exception cref="ValueSerializerException">Throws when attribute is used incorrectly</exception>
        /// <exception cref="SerializeFunctionException">Throws when can not generate read or write function</exception>
        public static ValueSerializer GetSerializer(MethodDefinition method, ParameterDefinition param, Writers writers, Readers readers)
        {
            var name = $"{method.Name}_{param.Name}";
            return GetSerializer(method.DeclaringType.Module, method.DeclaringType, param, param.ParameterType, name, writers, readers);
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="module">dll being weaved</param>
        /// <param name="holder">class that will old any packer functions</param>
        /// <param name="attributeProvider">a field or param that might have attribute</param>
        /// <param name="fieldType">type of the field or param with the attribute</param>
        /// <param name="fieldName">name that will be used for packer field</param>
        /// <param name="writers"></param>
        /// <param name="readers"></param>
        /// <returns></returns>
        /// <exception cref="ValueSerializerException">Throws when attribute is used incorrectly</exception>
        /// <exception cref="SerializeFunctionException">Throws when can not generate read or write function</exception>
        /// <exception cref="InvalidOperationException">Throws when <paramref name="holder"/> is not in <paramref name="module"/></exception>
        public static ValueSerializer GetSerializer(ModuleDefinition module, TypeDefinition holder, ICustomAttributeProvider attributeProvider, TypeReference fieldType, string fieldName, Writers writers, Readers readers)
        {
            if (holder.Module != module) throw new InvalidOperationException($"{holder.Name} was not in the weaving module, holderModule: {holder.Module}, weaver Module: {module}");

            // Store result in variable but DONT return early
            // We need to check if other attributes are also used
            // if user adds 2 attributes that dont work together weaver should then throw error
            ValueSerializer valueSerializer = null;

            // attributeProvider is null for generic fields,
            // but that is find because they wont have any of these attributes anyway
            if (attributeProvider != null)
                valueSerializer = GetUsingAttribute(module, holder, attributeProvider, fieldType, fieldName, valueSerializer);

            if (valueSerializer == null)
            {
                valueSerializer = FindSerializeFunctions(writers, readers, fieldType);
            }

            return valueSerializer;
        }

        private static ValueSerializer GetUsingAttribute(ModuleDefinition module, TypeDefinition holder, ICustomAttributeProvider attributeProvider, TypeReference fieldType, string fieldName, ValueSerializer valueSerializer)
        {
            if (attributeProvider.HasCustomAttribute<BitCountAttribute>())
                valueSerializer = BitCountFinder.GetSerializer(attributeProvider, fieldType);

            if (attributeProvider.HasCustomAttribute<VarIntAttribute>())
            {
                if (HasIntAttribute(valueSerializer))
                    throw new VarIntException($"[VarInt] can't be used with [BitCount], [VarIntBlocks] or [BitCountFromRange]");

                valueSerializer = new VarIntFinder().GetSerializer(module, holder, attributeProvider, fieldType, fieldName);
            }

            if (attributeProvider.HasCustomAttribute<VarIntBlocksAttribute>())
            {
                if (HasIntAttribute(valueSerializer))
                    throw new VarIntBlocksException($"[VarIntBlocks] can't be used with [BitCount], [VarInt] or [BitCountFromRange]");

                valueSerializer = VarIntBlocksFinder.GetSerializer(attributeProvider, fieldType);
            }

            if (attributeProvider.HasCustomAttribute<BitCountFromRangeAttribute>())
            {
                if (HasIntAttribute(valueSerializer))
                    throw new BitCountFromRangeException($"[BitCountFromRange] can't be used with [BitCount], [VarInt] or [VarIntBlocks]");

                valueSerializer = BitCountFromRangeFinder.GetSerializer(attributeProvider, fieldType);
            }

            ZigZagFinder.CheckZigZag(attributeProvider, fieldType, ref valueSerializer);

            if (attributeProvider.HasCustomAttribute<FloatPackAttribute>())
                valueSerializer = new FloatPackFinder().GetSerializer(module, holder, attributeProvider, fieldType, fieldName);

            if (attributeProvider.HasCustomAttribute<Vector2PackAttribute>())
                valueSerializer = new Vector2Finder().GetSerializer(module, holder, attributeProvider, fieldType, fieldName);

            if (attributeProvider.HasCustomAttribute<Vector3PackAttribute>())
                valueSerializer = new Vector3Finder().GetSerializer(module, holder, attributeProvider, fieldType, fieldName);

            if (attributeProvider.HasCustomAttribute<QuaternionPackAttribute>())
                valueSerializer = new QuaternionFinder().GetSerializer(module, holder, attributeProvider, fieldType, fieldName);
            return valueSerializer;
        }

        private static bool HasIntAttribute(ValueSerializer valueSerializer) => valueSerializer != null && valueSerializer.IsIntType;


        /// <exception cref="SerializeFunctionException">Throws when can not generate read or write function</exception>
        private static ValueSerializer FindSerializeFunctions(Writers writers, Readers readers, TypeReference fieldType)
        {
            // writers or readers might be null here, this is allowed because user of ValueSerializer might only be doing writing, or only doing reading
            var writeFunction = writers?.GetFunction_Throws(fieldType);
            var readFunction = readers?.GetFunction_Throws(fieldType);
            return new FunctionSerializer(writeFunction, readFunction);
        }
    }
}

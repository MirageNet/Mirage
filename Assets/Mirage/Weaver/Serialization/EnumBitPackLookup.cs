using System.Collections.Generic;
using Mirage.CodeGen;
using Mono.Cecil;

namespace Mirage.Weaver.Serialization
{
    /// <summary>
    /// Caches bit packing serializers for enum types, so each enum is checked and its errors logged only once
    /// <para>Shared by <see cref="Writers"/> and <see cref="Readers"/> so they both use the same result</para>
    /// </summary>
    public class EnumBitPackLookup
    {
        private readonly IWeaverLogger logger;
        private readonly Dictionary<TypeDefinition, BitCountSerializer> lookup = new Dictionary<TypeDefinition, BitCountSerializer>(new TypeReferenceComparer());

        public EnumBitPackLookup(IWeaverLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Checks all enums in module, so invalid enums give an error even if this module never sends them
        /// </summary>
        public void CheckAllEnums(ModuleDefinition module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsEnum)
                    _ = GetSerializer(type);
            }
        }

        /// <returns>null if enum has no bit packing attributes, or if they are invalid</returns>
        internal BitCountSerializer GetSerializer(TypeReference enumType)
        {
            var enumDef = enumType.Resolve();
            if (lookup.TryGetValue(enumDef, out var serializer))
                return serializer;

            try
            {
                serializer = EnumBitPackFinder.GetSerializer(enumDef);
            }
            catch (ValueSerializerException e)
            {
                // error is logged on the enum, and weaving has failed,
                // so caching null here is only to stop each use of the enum logging the same error again
                logger.Error(e.Message, enumDef);
                serializer = null;
            }

            lookup.Add(enumDef, serializer);
            return serializer;
        }
    }
}

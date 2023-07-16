// finds all readers and writers and register them
using System.Collections.Generic;
using Mono.Cecil;

namespace Mirage.CodeGen
{
    public class TypeReferenceComparer : IEqualityComparer<TypeReference>
    {
        public bool Equals(TypeReference x, TypeReference y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeReference obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}

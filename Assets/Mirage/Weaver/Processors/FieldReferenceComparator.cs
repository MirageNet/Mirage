using System.Collections.Generic;
using Mono.Cecil;

namespace Mirage.Weaver
{
    internal class FieldReferenceComparator : IEqualityComparer<FieldReference>
    {
        public bool Equals(FieldReference x, FieldReference y)
        {
            return x.DeclaringType.FullName == y.DeclaringType.FullName && x.Name == y.Name;
        }

        public int GetHashCode(FieldReference obj) => (obj.DeclaringType.FullName + "." + obj.Name).GetHashCode();
    }
}

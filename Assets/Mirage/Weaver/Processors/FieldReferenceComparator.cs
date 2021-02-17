using System.Collections.Generic;
using Mono.Cecil;

namespace Mirage.Weaver
{
    internal class FieldReferenceComparator : IEqualityComparer<FieldReference>
    {
        public bool Equals(FieldReference x, FieldReference y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(FieldReference obj) => obj.FullName.GetHashCode();
    }
}
using System.Collections.Generic;
using Mono.Cecil;

namespace Mirage.Weaver
{
    internal class FieldReferenceComparator : IEqualityComparer<FieldReference>
    {
        public bool Equals(FieldReference x, FieldReference y)
        {
            return x.Name == y.Name && x.DeclaringType.FullName == y.DeclaringType.FullName;
        }

        public int GetHashCode(FieldReference obj)
        {
            return HashCodeHelper.GetCombineHash(obj.Name, obj.DeclaringType.FullName);
        }
    }
}

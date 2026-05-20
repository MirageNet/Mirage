using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetInheritance
{
    class SyncSetInheritance : NetworkBehaviour
    {
        readonly SuperSet superSet = new SuperSet();


        public class SomeSet : SyncHashSet<string>
        {
            public SomeSet() : base(10) {}
        }

        public class SuperSet : SomeSet
        {
            public SuperSet() : base() {}
        }
    }
}

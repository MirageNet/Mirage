using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetGenericInheritance
{
    class SyncSetGenericInheritance : NetworkBehaviour
    {
        readonly SomeSetInt someSet = new SomeSetInt();


        public class SomeSet<T> : SyncHashSet<T>
        {
            public SomeSet() : base(10) {}
        }

        public class SomeSetInt : SomeSet<int>
        {
            public SomeSetInt() : base() {}
        }
    }
}

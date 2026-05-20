using Mirage;
using Mirage.Collections;

namespace SyncSetTests.SyncSetGenericAbstractInheritance
{
    class SyncSetGenericAbstractInheritance : NetworkBehaviour
    {
        readonly SomeSetInt superSyncSetString = new SomeSetInt();


        public abstract class SomeSet<T> : SyncHashSet<T>
        {
            protected SomeSet() : base(10) {}
        }

        public class SomeSetInt : SomeSet<int>
        {
            public SomeSetInt() : base() {}
        }
    }
}

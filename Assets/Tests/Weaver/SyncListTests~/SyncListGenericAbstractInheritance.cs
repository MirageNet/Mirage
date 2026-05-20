using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListGenericAbstractInheritance
{
    class SyncListGenericAbstractInheritance : NetworkBehaviour
    {
        readonly SomeListInt superSyncListString = new SomeListInt();


        public abstract class SomeList<T> : SyncList<T>
        {
            protected SomeList() : base(10) {}
        }

        public class SomeListInt : SomeList<int>
        {
            public SomeListInt() : base() {}
        }
    }
}

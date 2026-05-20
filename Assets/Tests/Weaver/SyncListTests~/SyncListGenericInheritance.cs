using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListGenericInheritance
{
    class SyncListGenericInheritance : NetworkBehaviour
    {
        readonly SomeListInt someList = new SomeListInt();


        public class SomeList<T> : SyncList<T>
        {
            public SomeList() : base(10) {}
        }

        public class SomeListInt : SomeList<int>
        {
            public SomeListInt() : base() {}
        }
    }
}

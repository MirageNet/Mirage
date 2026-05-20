using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListGenericInheritanceWithMultipleGeneric
{
    /*
    This test should pass
    */
    class SyncListGenericInheritanceWithMultipleGeneric : NetworkBehaviour
    {
        readonly SomeListInt someList = new SomeListInt();


        public class SomeList<G, T> : SyncList<T>
        {
            public SomeList() : base(10) {}
        }

        public class SomeListInt : SomeList<string, int>
        {
            public SomeListInt() : base() {}
        }
    }
}

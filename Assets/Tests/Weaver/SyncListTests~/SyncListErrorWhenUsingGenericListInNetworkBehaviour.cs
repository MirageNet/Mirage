using Mirage;
using Mirage.Collections;

namespace SyncListTests.SyncListErrorWhenUsingGenericListInNetworkBehaviour
{
    class SyncListErrorWhenUsingGenericListInNetworkBehaviour : NetworkBehaviour
    {
        readonly SomeList<int> someList = new SomeList<int>();


        public class SomeList<T> : SyncList<T>
        {
            public SomeList() : base(10) {}
        }
    }
}

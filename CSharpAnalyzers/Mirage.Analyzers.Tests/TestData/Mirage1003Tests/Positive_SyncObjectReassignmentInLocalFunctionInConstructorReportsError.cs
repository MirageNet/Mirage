using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public SyncList<int> {|#0:mySyncList|} = new SyncList<int>();

    public MyBehaviour()
    {
        void LocalMethod()
        {
            {|#1:mySyncList|} = new SyncList<int>();
        }
        LocalMethod();
    }
}

namespace Mirage
{
    public class NetworkBehaviour {}
}
namespace Mirage.Collections
{
    public interface ISyncObject {}
    public class SyncList<T> : ISyncObject
    {
        public T this[int index] { get => default; set {} }
    }
}

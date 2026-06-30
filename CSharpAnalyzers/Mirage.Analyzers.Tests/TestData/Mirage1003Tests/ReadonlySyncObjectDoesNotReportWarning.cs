using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList = new SyncList<int>();
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

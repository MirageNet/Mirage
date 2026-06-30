using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public int myNormalField = 0;

    public void Modify()
    {
        myNormalField = 10;
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

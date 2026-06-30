using System;
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public SyncList<int> {|#0:mySyncList|} = new SyncList<int>();

    public MyBehaviour()
    {
        Action act = () => {
            {|#1:mySyncList|} = new SyncList<int>();
        };
        act();
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

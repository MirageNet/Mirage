using System.Collections.Generic;

namespace Mirage.RemoteCalls
{
    public interface INetworkBehaviour
    {
        int ComponentIndex { get; }
        INetworkIdentity Identity { get; }
    }

    /// <summary>
    /// Classes that implement this can contain RPC functions
    /// </summary>
    public interface ICanHaveRpc : INetworkBehaviour
    {
    }
    public interface INetworkIdentity
    {
        NetworkServer Server { get; }
        ServerObjectManager ServerObjectManager { get; }

        NetworkClient Client { get; }
        ClientObjectManager ClientObjectManager { get; }

        HashSet<INetworkPlayer> Observers { get; }
        INetworkPlayer Owner { get; }
        bool HasAuthority { get; }

        RemoteCallCollection RemoteCallCollection { get; }

        uint NetId { get; }
    }
}

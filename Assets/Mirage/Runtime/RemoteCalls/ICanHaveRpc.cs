using System.Collections.Generic;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public interface INetworkBehaviour
    {
        int ComponentIndex { get; }
        INetworkIdentity Identity { get; }
    }

    /// <summary>
    /// Used by Weaver to know when to generate code for a NetworkBehaviour
    /// </summary>
    public interface IGeneratRpc
    {
        void RegisterRpc(RemoteCallCollection collection);
        int GetRpcCount();
    }

    /// <summary>
    /// Classes that implement this can contain RPC functions
    /// </summary>
    public interface ICanHaveRpc : IGeneratRpc, INetworkBehaviour
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

    // todo this might be a bad idea
    //      might be better instead to allow NetworkIdentity to be put on NetworkManager object directly,
    //      and then find a way to find NetworkIdentity, and componets on it

    public class NetworkManagerIdentity : MonoBehaviour, INetworkIdentity
    {
        public NetworkServer Server { get; set; }

        public ServerObjectManager ServerObjectManager { get; set; }

        public NetworkClient Client { get; set; }

        public ClientObjectManager ClientObjectManager { get; set; }

        public HashSet<INetworkPlayer> Observers => throw new System.NotImplementedException();

        public INetworkPlayer Owner => throw new System.NotImplementedException();

        public bool HasAuthority => throw new System.NotImplementedException();

        public RemoteCallCollection RemoteCallCollection => throw new System.NotImplementedException();

        public uint NetId => throw new System.NotImplementedException();
    }
    public abstract class NetworkManagerBehaviour : MonoBehaviour, ICanHaveRpc
    {
        public int ComponentIndex => throw new System.NotImplementedException();

        public INetworkIdentity Identity => throw new System.NotImplementedException();

        public int GetRpcCount()
        {
            throw new System.NotImplementedException();
        }

        public void RegisterRpc(RemoteCallCollection collection)
        {
            throw new System.NotImplementedException();
        }
    }
}

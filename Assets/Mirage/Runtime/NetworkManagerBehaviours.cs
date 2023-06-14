using System;
using Mirage.RemoteCalls;
using UnityEngine;

namespace Mirage.Components
{
    //public class NetworkManagerBehaviours : MonoBehaviour
    //{
    //    public NetworkServer Server;
    //    public NetworkClient Client;
    //    public ManagerBehaviour[] ManagerBehaviours;
    //}
    //public abstract class ManagerBehaviour : MonoBehaviour {

    //}
    //public interface IServerManager
    //{
    //    void ServerStarted();
    //    void ServerStopped();
    //}

    //public interface IClientManager
    //{
    //    void ClientStarted();
    //    void ClientStopped();
    //}
}
namespace Mirage.Components.ManagerRpcs
{
    [NetworkMessage]
    public struct ManagerRpcMessage
    {
        public int FunctionIndex;
        public int? ReplyId;
        public ArraySegment<byte> Payload;
    }
    [NetworkMessage]
    public struct ManagerRpcReply
    {
        public int ReplyId;
        public ArraySegment<byte> Payload;
    }

    public class ManagerRpcs : MonoBehaviour
    {
        public NetworkServer Server;
        public NetworkClient Client;

        public ManagerRpcsBehaviour[] RpcBehaviours;
        public RemoteCallCollection RemoteCallCollection;

        private void OnValidate()
        {
            RpcBehaviours = GetComponentsInChildren<ManagerRpcsBehaviour>(true);
        }

        private void Awake()
        {
            Server.Started.AddListener(ServerStarted);
            Server.Stopped.AddListener(ServerStopped);

            Client.Started.AddListener(ClientStarted);
            Client.Disconnected.AddListener(ClientStopped);
        }

        private void Register()
        {
            if (RemoteCallCollection != null)
                return;

            RemoteCallCollection = new RemoteCallCollection();
            RemoteCallCollection.RegisterAll(RpcBehaviours);
        }

        public void ServerStarted()
        {
            Register();
            Server.MessageHandler.RegisterHandler<ManagerRpcMessage>(OnServerRpcMessage);

        }
        public void ServerStopped()
        {

        }
        public void ClientStarted()
        {
            Register();
        }
        public void ClientStopped(ClientStoppedReason _)
        {

        }
    }



    public abstract class ManagerRpcsBehaviour : MonoBehaviour, IGeneratRpc
    {
        public virtual int GetRpcCount() => 0;
        public virtual void RegisterRpc(RemoteCallCollection collection) { }
    }

    public class DemoManagerRpc : ManagerRpcsBehaviour
    {
        [ClientRpc]
        public void ClientHello(string message)
        {
            Debug.Log($"Client: {message}");
        }

        [ServerRpc]
        public void ServerHello(string message)
        {
            Debug.Log($"Client: {message}");
        }
    }
}

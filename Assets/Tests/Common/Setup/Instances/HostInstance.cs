using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests
{
    public class HostInstance : ServerInstance, IClientInstance
    {
        public NetworkClient Client { get; }
        public ClientObjectManager ClientObjectManager { get; }

        public LocalPlayerObject HostPlayer => Players[0];

        public HostInstance(Config serverConfig) : base(serverConfig)
        {
            Client = GameObject.AddComponent<NetworkClient>();
            ClientObjectManager = GameObject.AddComponent<ClientObjectManager>();
            ClientObjectManager.Client = Client;
        }

        public override void StartServer()
        {
            Server.StartServer(Client);
        }
    }
}

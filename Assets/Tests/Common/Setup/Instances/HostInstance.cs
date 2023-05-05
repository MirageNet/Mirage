using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests
{
    public class HostInstance : ServerInstance, IClientInstance
    {
        public NetworkClient Client { get; }
        public ClientObjectManager ClientObjectManager { get; }

        public LocalPlayerObject HostPlayer { get; private set; }

        public HostInstance(Config serverConfig) : base(serverConfig)
        {
            Client = GameObject.AddComponent<NetworkClient>();
            ClientObjectManager = GameObject.AddComponent<ClientObjectManager>();
            Client.ObjectManager = ClientObjectManager;
        }

        public override void StartServer()
        {
            Server.StartServer(Client);
        }

        protected override void AddToPlayerList(INetworkPlayer player, LocalPlayerObject localPlayerObject)
        {
            if (player == Server.LocalPlayer)
            {
                HostPlayer = localPlayerObject;
            }
            else
            {
                base.AddToPlayerList(player, localPlayerObject);
            }
        }
    }
}

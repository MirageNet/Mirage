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

        protected override LocalPlayerObject GetOrAddLocalPlayer(INetworkPlayer player)
        {
            if (player == Server.LocalPlayer)
            {
                if (HostPlayer == null)
                    HostPlayer = new LocalPlayerObject(player);

                return HostPlayer;
            }
            else
            {
                return base.GetOrAddLocallayer(player);
            }
        }
    }
}

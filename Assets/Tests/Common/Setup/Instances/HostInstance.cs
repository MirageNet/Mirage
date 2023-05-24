using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests
{
    public class HostInstance : ServerInstance, IClientInstance
    {
        public NetworkClient Client { get; }
        public ClientObjectManager ClientObjectManager { get; }

        /// <summary>
        /// Player on server side, for host
        /// </summary>
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
                return base.GetOrAddLocalPlayer(player);
            }
        }

        void IClientInstance.SetupPlayer(bool withCharacter)
        {
            // host needs to do nothing with setup

            // this is used to set the gameobject and Identity on client,
            // but for host they are the same objects are server
        }
    }
}

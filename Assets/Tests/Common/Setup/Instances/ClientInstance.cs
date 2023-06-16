using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests
{
    /// <summary>
    /// Instance of Client for <see cref="ClientServerSetup"/>
    /// </summary>
    public class ClientInstance : BaseInstance, IClientInstance
    {
        public NetworkClient Client { get; }
        public ClientObjectManager ClientObjectManager { get; }

        public GameObject character;
        public NetworkIdentity identity;
        public INetworkPlayer player;

        public override NetworkWorld World => Client.World;

        public ClientInstance(Config config, TestSocketFactory socketFactory, string nameSuffix)
        {
            GameObject = new GameObject("client_" + nameSuffix, typeof(ClientObjectManager), typeof(NetworkClient));
            Client = GameObject.GetComponent<NetworkClient>();
            Client.RethrowException = true;
            if (config != null) Client.PeerConfig = config;
            Client.SocketFactory = socketFactory;

            ClientObjectManager = GameObject.GetComponent<ClientObjectManager>();
            Client.ObjectManager = ClientObjectManager;
        }

        public void SetupPlayer(bool withCharacter)
        {
            player = Client.Player;

            if (withCharacter)
            {
                identity = player.Identity;
                character = identity.gameObject;
                character.name = "player (client)";
            }
        }
    }

    public interface IClientInstance
    {
        /// <summary>
        /// NetworkManager game object
        /// </summary>
        GameObject GameObject { get; }
        NetworkClient Client { get; }
        ClientObjectManager ClientObjectManager { get; }

        void SetupPlayer(bool withCharacter);
    }
}

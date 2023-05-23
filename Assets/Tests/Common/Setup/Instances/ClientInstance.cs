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
            if (config != null) Client.PeerConfig = config;
            Client.SocketFactory = socketFactory;

            ClientObjectManager = GameObject.GetComponent<ClientObjectManager>();
            Client.ObjectManager = ClientObjectManager;
        }

        public void SetupNoCharacter()
        {
            player = Client.Player;
        }

        public void SetupCharacter()
        {
            player = Client.Player;
            identity = player.Identity;
            character = identity.gameObject;
            character.name = "player (client)";
        }
    }

    public interface IClientInstance
    {
        GameObject GameObject { get; }
        NetworkClient Client { get; }
        ClientObjectManager ClientObjectManager { get; }
    }
}

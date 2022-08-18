using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Tests
{
    /// <summary>
    /// Instance of Client for <see cref="ClientServerSetup{T}"/>
    /// </summary>
    public class ClientInstance<T>
    {
        public GameObject go;
        public NetworkClient client;
        public ClientObjectManager clientObjectManager;
        public GameObject character;
        public NetworkIdentity identity;
        public T component;
        public INetworkPlayer player;

        public ClientInstance(Config config, TestSocketFactory socketFactory)
        {
            go = new GameObject("client", typeof(ClientObjectManager), typeof(NetworkClient));
            client = go.GetComponent<NetworkClient>();
            if (config != null) client.PeerConfig = config;
            client.SocketFactory = socketFactory;

            clientObjectManager = go.GetComponent<ClientObjectManager>();
            clientObjectManager.Client = client;
        }

        public void SetupCharacter()
        {
            // get the connections so that we can spawn players
            player = client.Player;
            identity = player.Identity;
            character = identity.gameObject;
            character.name = "player (client)";
            component = character.GetComponent<T>();
        }
    }
}

using System.Linq;
using Mirage.SocketLayer;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    /// <summary>
    /// Instance of Server for <see cref="ClientServerSetup{T}"/>
    /// </summary>
    public class ServerInstance<T>
    {
        public GameObject go;
        public NetworkServer server;
        public ServerObjectManager serverObjectManager;
        public GameObject character;
        public NetworkIdentity identity;
        public T component;
        public INetworkPlayer FirstPlayer;
        /// <summary>
        /// Clients that want to connect to this Instance should use this socket factory
        /// </summary>
        public TestSocketFactory socketFactory;

        public ServerInstance(Config config)
        {
            go = new GameObject("server", typeof(ServerObjectManager), typeof(NetworkServer));
            server = go.GetComponent<NetworkServer>();
            if (config != null) server.PeerConfig = config;
            socketFactory = go.AddComponent<TestSocketFactory>();
            server.SocketFactory = socketFactory;

            serverObjectManager = go.GetComponent<ServerObjectManager>();
            serverObjectManager.Server = server;
        }

        public void SpawnPlayerForFirstClient(GameObject prefab)
        {
            FirstPlayer = server.Players.First();

            character = Object.Instantiate(prefab);
            character.name = "player (server)";
            identity = character.GetComponent<NetworkIdentity>();
            component = character.GetComponent<T>();
            serverObjectManager.AddCharacter(FirstPlayer, character);
        }
    }
}

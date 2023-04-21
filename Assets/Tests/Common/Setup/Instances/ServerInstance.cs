using System;
using System.Collections.Generic;
using Mirage.SocketLayer;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Mirage.Tests
{
    /// <summary>
    /// Instance of Server for <see cref="ClientServerSetup"/>
    /// </summary>
    public class ServerInstance : BaseInstance
    {
        public readonly NetworkServer Server;
        public readonly ServerObjectManager ServerObjectManager;
        /// <summary>
        /// Clients that want to connect to this Instance should use this socket factory
        /// </summary>
        public readonly TestSocketFactory SocketFactory;
        /// <summary>Set used by GetNewPlayer</summary>
        private readonly HashSet<INetworkPlayer> _foundPlayers = new HashSet<INetworkPlayer>();
        public readonly List<LocalPlayerObject> _players = new List<LocalPlayerObject>();

        /// <summary>
        /// Remote players (WILL NOT INCLUDE HOST)
        /// </summary>
        public IReadOnlyList<LocalPlayerObject> Players => _players;

        public override NetworkWorld World => Server.World;

        public ServerInstance(Config config)
        {
            GameObject = new GameObject("server", typeof(ServerObjectManager), typeof(NetworkServer));
            Server = GameObject.GetComponent<NetworkServer>();
            if (config != null) Server.PeerConfig = config;
            SocketFactory = GameObject.AddComponent<TestSocketFactory>();
            Server.SocketFactory = SocketFactory;

            ServerObjectManager = GameObject.GetComponent<ServerObjectManager>();
            ServerObjectManager.Server = Server;
        }

        public virtual void StartServer()
        {
            Server.StartServer();
        }

        /// <summary>
        /// Will return new players, Note this could return host player
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal INetworkPlayer GetNewPlayer()
        {
            var foundCount = 0;
            INetworkPlayer found = null;
            foreach (var player in Server.Players)
            {
                if (_foundPlayers.Contains(player))
                    continue;

                foundCount++;
                if (foundCount == 1)
                {
                    found = player;
                    _foundPlayers.Add(player);
                }
            }

            if (foundCount > 1)
                throw new InvalidOperationException("Could more than 1 new players. Dont add multiple test clients at same time");
            if (foundCount == 0)
                throw new InvalidOperationException("Could not find new player, wait for player count to go up before calling this");

            return found;
        }

        public void SpawnCharacter(INetworkPlayer player, NetworkIdentity prefab)
        {
            var identity = Object.Instantiate(prefab);
            identity.name = "player (server)";
            // prefab is not active, we set it active here
            // note: this is needed for SOM to spawn the object on cliente
            identity.gameObject.SetActive(true);
            ServerObjectManager.AddCharacter(player, identity);

            AddToPlayerList(player, new LocalPlayerObject
            {
                Player = player,
                Identity = identity,
                GameObject = identity.gameObject,
            });
        }

        protected virtual void AddToPlayerList(INetworkPlayer player, LocalPlayerObject localPlayerObject)
        {
            _players.Add(localPlayerObject);
        }


        /// <summary>
        /// Objects for a player on the server
        /// </summary>
        public class LocalPlayerObject
        {
            public INetworkPlayer Player;
            public GameObject GameObject;
            public NetworkIdentity Identity;
        }
    }
}

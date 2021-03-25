using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    public class NetworkWorld : IObjectLocator
    {
        static readonly ILogger logger = LogFactory.GetLogger<NetworkWorld>();

        /// <summary>
        /// Raised when the client spawns an object
        /// </summary>
        public event Action<NetworkIdentity> onSpawn;

        /// <summary>
        /// Raised when the client unspawns an object
        /// </summary>
        public event Action<NetworkIdentity> onUnspawn;

        private readonly Dictionary<uint, NetworkIdentity> SpawnedObjects = new Dictionary<uint, NetworkIdentity>();

        public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities => SpawnedObjects.Values;

        public bool TryGetIdentity(uint id, out NetworkIdentity identity)
        {
            return SpawnedObjects.TryGetValue(id, out identity) && identity != null;
        }
        /// <summary>
        /// Adds Identity to world and invokes spawned event
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="identity"></param>
        internal void AddIdentity(uint netId, NetworkIdentity identity)
        {
            SpawnedObjects.Add(netId, identity);
            onSpawn?.Invoke(identity);
        }
        internal void RemoveIdentity(NetworkIdentity identity)
        {
            uint netId = identity.NetId;
            SpawnedObjects.Remove(netId);
            onUnspawn?.Invoke(identity);
        }
        internal void RemoveIdentity(uint netId)
        {
            if (netId == 0) throw new ArgumentException("netid = 0 is invalid");

            SpawnedObjects.TryGetValue(netId, out NetworkIdentity identity);
            SpawnedObjects.Remove(netId);
            onUnspawn?.Invoke(identity);
        }


        internal void ClearSpawnedObjects()
        {
            SpawnedObjects.Clear();
        }

        public INetworkServer Server { get; }
        public INetworkClient Client { get; }

        public NetworkWorld(INetworkServer server, INetworkClient client)
        {
            Server = server;
            Client = client;
        }
    }
}

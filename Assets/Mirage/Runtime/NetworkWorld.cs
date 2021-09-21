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

        /// <summary>
        /// Time kept in this world
        /// </summary>
        public NetworkTime Time { get; } = new NetworkTime();

        private readonly Dictionary<uint, NetworkIdentity> SpawnedObjects = new Dictionary<uint, NetworkIdentity>();
        public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities => SpawnedObjects.Values;

        public bool TryGetIdentity(uint netId, out NetworkIdentity identity)
        {
            return SpawnedObjects.TryGetValue(netId, out identity) && identity != null;
        }

        /// <summary>
        /// Adds Identity to world and invokes spawned event
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="identity"></param>
        internal void AddIdentity(uint netId, NetworkIdentity identity)
        {
            if (netId == 0) throw new ArgumentException("id can not be zero", nameof(netId));
            if (SpawnedObjects.ContainsKey(netId)) throw new ArgumentException("An item with same id already exists", nameof(netId));
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (netId != identity.NetId) throw new ArgumentException("NetworkIdentity did not have matching netId", nameof(identity));

            SpawnedObjects.Add(netId, identity);
            onSpawn?.Invoke(identity);
        }

        internal void RemoveIdentity(NetworkIdentity identity)
        {
            uint netId = identity.NetId;
            bool removed = SpawnedObjects.Remove(netId);
            // only invoke event if values was successfully removed
            if (removed)
                onUnspawn?.Invoke(identity);
        }

        internal void RemoveIdentity(uint netId)
        {
            if (netId == 0) throw new ArgumentException("id can not be zero", nameof(netId));

            SpawnedObjects.TryGetValue(netId, out NetworkIdentity identity);
            bool removed = SpawnedObjects.Remove(netId);
            // only invoke event if values was successfully removed
            if (removed)
                onUnspawn?.Invoke(identity);
        }

        internal void ClearSpawnedObjects()
        {
            SpawnedObjects.Clear();
        }

        public NetworkWorld()
        {

        }
    }
}

using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Holds collection of spawned network objects
    /// <para>This class works on both server and client</para>
    /// </summary>
    public class NetworkWorld : IObjectLocator
    {
        private static readonly ILogger logger = LogFactory.GetLogger<NetworkWorld>();

        /// <summary>
        /// Raised when object is spawned
        /// </summary>
        public event Action<NetworkIdentity> onSpawn;

        /// <summary>
        /// Raised when object is unspawned or destroyed
        /// </summary>
        public event Action<NetworkIdentity> onUnspawn;

        /// <summary>
        /// Time kept in this world
        /// </summary>
        public NetworkTime Time { get; } = new NetworkTime();

        private readonly Dictionary<uint, NetworkIdentity> _spawnedObjects = new Dictionary<uint, NetworkIdentity>();
        public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities => _spawnedObjects.Values;

        public bool TryGetIdentity(uint netId, out NetworkIdentity identity)
        {
            return _spawnedObjects.TryGetValue(netId, out identity) && identity != null;
        }

        /// <summary>
        /// Adds Identity to world and invokes spawned event
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="identity"></param>
        internal void AddIdentity(uint netId, NetworkIdentity identity)
        {
            if (netId == 0) throw new ArgumentException("id can not be zero", nameof(netId));
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (netId != identity.NetId) throw new ArgumentException("NetworkIdentity did not have matching netId", nameof(identity));
            if (_spawnedObjects.TryGetValue(netId, out var existing) && existing != null) throw new ArgumentException("An Identity with same id already exists in network world", nameof(netId));

            // dont use add, netId might already exist but have been destroyed
            // this can happen client side. we check for this case in TryGetValue above
            _spawnedObjects[netId] = identity;
            onSpawn?.Invoke(identity);
        }

        internal void RemoveIdentity(NetworkIdentity identity)
        {
            var netId = identity.NetId;
            var removed = _spawnedObjects.Remove(netId);
            // only invoke event if values was successfully removed
            if (removed)
                onUnspawn?.Invoke(identity);
        }

        internal void RemoveDestroyedObjects()
        {
            var removalCollection = new List<NetworkIdentity>(SpawnedIdentities);

            foreach (var identity in removalCollection)
            {
                if (identity == null)
                    _spawnedObjects.Remove(identity.NetId);
            }
        }

        internal void RemoveIdentity(uint netId)
        {
            if (netId == 0) throw new ArgumentException("id can not be zero", nameof(netId));

            _spawnedObjects.TryGetValue(netId, out var identity);
            var removed = _spawnedObjects.Remove(netId);
            // only invoke event if values was successfully removed
            if (removed)
                onUnspawn?.Invoke(identity);
        }

        internal void ClearSpawnedObjects()
        {
            _spawnedObjects.Clear();
        }

        public NetworkWorld()
        {

        }
    }
}

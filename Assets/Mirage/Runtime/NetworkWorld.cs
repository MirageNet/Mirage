using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    public readonly struct CombinedKey
    {
        private readonly uint _keyOne;
        private readonly byte _keyTwo;

        public CombinedKey(uint keyOne, byte keyTwo)
        {
            _keyOne = keyOne;
            _keyTwo = keyTwo;
        }

        public bool Equals(CombinedKey other)
        {
            return _keyOne == other._keyOne && _keyTwo == other._keyTwo;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is CombinedKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((_keyOne * 31) ^ _keyTwo);
            }
        }
    }

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

        private readonly Dictionary<CombinedKey, NetworkIdentity> SpawnedObjects = new Dictionary<CombinedKey, NetworkIdentity>();
        public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities => SpawnedObjects.Values;

        public bool TryGetIdentity(uint netId, byte serverId, out NetworkIdentity identity)
        {
            bool isSpawned = SpawnedObjects.TryGetValue(new CombinedKey(netId, serverId), out identity);
            return isSpawned && identity != null;
        }

        /// <summary>
        /// Adds Identity to world and invokes spawned event
        /// </summary>
        /// <param name="netId"></param>
        /// <param name="serverId"></param>
        /// <param name="identity"></param>
        internal void AddIdentity(uint netId, byte serverId, NetworkIdentity identity)
        {
            if (netId == 0) throw new ArgumentException("net id can not be zero", nameof(netId));
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (netId != identity.NetId) throw new ArgumentException("NetworkIdentity did not have matching netId", nameof(identity));
            if (serverId == 0) throw new ArgumentException("server id can not be zero", nameof(serverId));

            CombinedKey uniqueId = new CombinedKey(netId, serverId);

            if (SpawnedObjects.TryGetValue(uniqueId, out NetworkIdentity existing) && existing != null) throw new ArgumentException($"NetId:{netId} ServerId:{serverId} resulted in an id already exists in dictionary.", nameof(uniqueId));

            // dont use add, netid might already exist but have been destroyed
            // this canhappen client side. we check for this case in TryGetValue above
            SpawnedObjects[uniqueId] = identity;
            onSpawn?.Invoke(identity);
        }

        internal void RemoveIdentity(NetworkIdentity identity)
        {
            uint netId = identity.NetId;
            byte serverId = identity.ServerId;
            bool removed = SpawnedObjects.Remove(new CombinedKey(netId, serverId));
            // only invoke event if values was successfully removed
            if (removed)
                onUnspawn?.Invoke(identity);
        }

        internal void RemoveDestroyedObjects()
        {
            var removalCollection = new List<NetworkIdentity>(SpawnedIdentities);

            foreach (NetworkIdentity identity in removalCollection)
            {
                if (identity == null)
                    SpawnedObjects.Remove(new CombinedKey(identity.NetId, identity.ServerId));
            }
        }

        internal void RemoveIdentity(uint netId, byte serverId)
        {
            if (netId == 0) throw new ArgumentException("net id can not be zero", nameof(netId));
            if (serverId == 0) throw new ArgumentException("server id can not be zero", nameof(serverId));

            CombinedKey uniqueId = new CombinedKey(netId, serverId);

            SpawnedObjects.TryGetValue(uniqueId, out NetworkIdentity identity);
            bool removed = SpawnedObjects.Remove(uniqueId);
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

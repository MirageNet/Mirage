using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Event that can be used to check authority
    /// </summary>
    /// <param name="hasAuthority">if the owner now has authority or if it was removed</param>
    /// <param name="owner">the new or old owner. Owner value might be null on client side. But will be set on server</param>
    public delegate void AuthorityChanged(NetworkIdentity identity, bool hasAuthority, INetworkPlayer owner);

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
        /// <para><b>WARNING:</b> onUnspawn might be called after Identity has been destroyed by unity</para>
        /// </summary>
        public event Action<NetworkIdentity> onUnspawn;

        /// <summary>
        /// Raised when authority is given or removed from an identity. It is invoked on both server and client
        /// <para>
        /// Can be used when you need to check for authority on all objects, rather than adding an event to each object.
        /// </para>
        /// </summary>
        public event AuthorityChanged OnAuthorityChanged;

        /// <summary>
        /// Time kept in this world
        /// </summary>
        public NetworkTime Time { get; } = new NetworkTime();

        private readonly Dictionary<uint, NetworkIdentity> _spawnedObjects = new Dictionary<uint, NetworkIdentity>();
        public IReadOnlyCollection<NetworkIdentity> SpawnedIdentities => _spawnedObjects.Values;

        private readonly List<NetworkIdentity> _sortedIdentities = new List<NetworkIdentity>();
        private bool _needsSorting = true;

        /// <summary>
        /// A list of spawned identities, sorted by netId.
        /// <para>This list is cached and will only be re-sorted if identities have changed</para>
        /// </summary>
        public IReadOnlyList<NetworkIdentity> GetSortedIdentities()
        {
            if (_needsSorting)
            {
                _needsSorting = false;
                _sortedIdentities.Clear();

                // increase Capacity first to avoid alloc when adding
                if (_sortedIdentities.Capacity < _spawnedObjects.Count)
                    _sortedIdentities.Capacity = _spawnedObjects.Count;

                _sortedIdentities.AddRange(_spawnedObjects.Values);
                _sortedIdentities.Sort((x, y) => x.NetId.CompareTo(y.NetId));
            }
            return _sortedIdentities;
        }

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

            if (logger.LogEnabled()) logger.Log($"Adding [netId={netId}, name={identity.name}] to World");

            // dont use add, netId might already exist but have been destroyed
            // this can happen client side. we check for this case in TryGetValue above
            _spawnedObjects[netId] = identity;
            _needsSorting = true;
            onSpawn?.Invoke(identity);

            // owner might be set before World is
            // so we need to invoke authChange now if the object has an owner
            if (identity.Owner != null)
                InvokeOnAuthorityChanged(identity, true, identity.Owner);
        }

        internal void RemoveIdentity(NetworkIdentity identity)
        {
            var netId = identity.NetId;
            RemoveInternal(netId, identity);
        }

        internal void RemoveIdentity(uint netId)
        {
            if (netId == 0) throw new ArgumentException("id can not be zero", nameof(netId));

            _spawnedObjects.TryGetValue(netId, out var identity);
            RemoveInternal(netId, identity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveInternal(uint netId, NetworkIdentity identity)
        {
            var removed = _spawnedObjects.Remove(netId);
            // only invoke event if values was successfully removed
            if (removed)
            {
                _needsSorting = true;
                if (logger.LogEnabled()) logger.Log($"Removing [netId={netId}, name={identity?.name}] from World");
                onUnspawn?.Invoke(identity);
            }
            else
            {
                if (logger.LogEnabled()) logger.Log($"Did not remove [netId={netId}, name={identity?.name}] from World. Maybe it was previously removed?");
            }
        }

        public void RemoveDestroyedObjects()
        {
            if (logger.LogEnabled()) logger.Log($"Removing destroyed objects");
            var removalCollection = new List<NetworkIdentity>(SpawnedIdentities);

            foreach (var identity in removalCollection)
            {
                if (identity == null)
                {
                    if (logger.LogEnabled()) logger.Log($"Removing destroyed object:[netId={identity.NetId}]");
                    RemoveIdentity(identity);
                }
            }
        }

        internal void ClearSpawnedObjects()
        {
            _spawnedObjects.Clear();
            _needsSorting = true;
        }

        internal void InvokeOnAuthorityChanged(NetworkIdentity identity, bool hasAuthority, INetworkPlayer owner)
        {
            OnAuthorityChanged?.Invoke(identity, hasAuthority, owner);
        }

        public NetworkWorld()
        {

        }
    }
}

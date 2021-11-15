using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components
{
    [Serializable]
    public class ProximitySettings
    {
        /// <summary>
        /// The maximum range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public float SightDistance = 10;
    }

    public class DistanceVisibilitySystem : VisibilitySystem
    {
        static readonly ILogger Logger = LogFactory.GetLogger(typeof(DistanceVisibilitySystem));

        private readonly float _updateInterval = 0;
        private float _nextUpdate = 0;
        private readonly Dictionary<NetworkIdentity, ProximitySettings> _proximityObjects = new Dictionary<NetworkIdentity, ProximitySettings>();

        /// <summary>
        ///     Starts up a new instance of a network proximity visibility system.
        /// </summary>
        /// <param name="serverObjectManager">The reference to <see cref="ServerObjectManager"/>.</param>
        /// <param name="updateInterval"></param>
        public DistanceVisibilitySystem(ServerObjectManager serverObjectManager, float updateInterval) : base(serverObjectManager)
        {
            _updateInterval = updateInterval;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool FastInDistanceXZ(Vector3 a, Vector3 b, float sqRange)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            float sqDist = dx * dx + dz * dz;
            return sqDist < sqRange;
        }

        #region Overrides of NetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            // does object have owner?
            if (identity.Owner != null)
            {
                OnAuthenticated(identity.Owner);
            }
        }

        /// <summary>
        ///     When new player authenticates we need to show them objects they should see.
        /// </summary>
        /// <param name="player">The player that just authenticated and we need to show objects to.</param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            Vector3 b = player.Identity.transform.position;

            foreach (KeyValuePair<NetworkIdentity, ProximitySettings> kvp in _proximityObjects)
            {
                NetworkIdentity identity = kvp.Key;
                ProximitySettings setting = kvp.Value;

                Vector3 a = identity.transform.position;

                if (!FastInDistanceXZ(a, b, setting.SightDistance * setting.SightDistance)) continue;

                if (!Observers.ContainsKey(identity))
                    Observers.Add(identity, new HashSet<INetworkPlayer>());
                else if (Observers.ContainsKey(identity) && !Observers[identity].Contains(player))
                    Observers[identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
            }
        }

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public override void Rebuild()
        {
            if (!(_nextUpdate < Time.time)) return;

            foreach (KeyValuePair<NetworkIdentity, ProximitySettings> kvp in _proximityObjects)
            {
                NetworkIdentity identity = kvp.Key;
                ProximitySettings setting = kvp.Value;
                if (!Observers.ContainsKey(identity)) continue;

                foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
                {
                    Observers.TryGetValue(identity, out HashSet<INetworkPlayer> players);

                    if (player.Identity == null || identity == null) continue;

                    if (FastInDistanceXZ(player.Identity.transform.position, identity.transform.position, setting.SightDistance * setting.SightDistance))
                    {
                        if (players != null && players.Contains(player)) continue;

                        Observers[identity].Add(player);
                        InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                    }
                    else
                    {
                        if (players != null && !players.Contains(player)) continue;

                        Observers[identity].Remove(player);
                        InterestManager.ServerObjectManager.HideToPlayer(identity, player);
                    }
                }
            }

            _nextUpdate += _updateInterval;
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        /// <para>Passing in specific settings for this network object.</para>
        public override void RegisterObject<TSettings>(NetworkIdentity identity, TSettings settings)
        {
            Logger.Assert(settings is ProximitySettings);
            _proximityObjects.Add(identity, settings as ProximitySettings);

            if (!Observers.ContainsKey(identity))
                Observers.Add(identity, new HashSet<INetworkPlayer>());
        }

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public override void UnregisterObject(NetworkIdentity identity)
        {
            _proximityObjects.Remove(identity);

            Observers.Remove(identity);
        }

        #endregion
    }
}

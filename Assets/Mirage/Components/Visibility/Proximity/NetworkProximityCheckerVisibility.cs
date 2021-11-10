using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components
{
    [Serializable]
    public class ProximitySettings : INetworkVisibility.BaseSettings
    {
        /// <summary>
        /// The maximum range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public float SightDistance = 10;
    }

    /// <summary>
    /// Component that controls visibility of networked objects for players.
    /// <para>Any object with this component on it will not be visible to players more than a (configurable) distance away.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkProximityChecker")]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkProximityChecker.html")]
    public class NetworkProximityCheckerVisibility : NetworkVisibility
    {
        static readonly ILogger Logger = LogFactory.GetLogger(typeof(NetworkProximityCheckerVisibility));

        private readonly float _updateInterval = 0;
        private float _nextUpdate = 0;
        private List<ProximitySettings> _proximityObjects = new List<ProximitySettings>();

        /// <summary>
        ///     Starts up a new instance of a network proximity visibility system.
        /// </summary>
        /// <param name="serverObjectManager">The reference to <see cref="ServerObjectManager"/>.</param>
        /// <param name="updateInterval"></param>
        public NetworkProximityCheckerVisibility(ServerObjectManager serverObjectManager, float updateInterval) : base(serverObjectManager)
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
        /// 
        /// </summary>
        /// <param name="player"></param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            Vector3 b = player.Identity.transform.position;

            foreach (ProximitySettings setting in _proximityObjects)
            {
                Vector3 a = setting.Identity.transform.position;

                if (!FastInDistanceXZ(a, b, setting.SightDistance * setting.SightDistance)) continue;

                if (!VisibilitySystemData.ContainsKey(setting.Identity))
                    VisibilitySystemData.Add(setting.Identity, new HashSet<INetworkPlayer>());
                else if (VisibilitySystemData.ContainsKey(setting.Identity) && !VisibilitySystemData[setting.Identity].Contains(player))
                    VisibilitySystemData[setting.Identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(setting.Identity, player);
            }

            // Always show self to them.
            InterestManager.ServerObjectManager.ShowToPlayer(player.Identity, player);
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
            if (!(_nextUpdate < Time.time)) return;

            foreach (ProximitySettings setting in _proximityObjects)
            {
                foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
                {
                    if (!VisibilitySystemData.ContainsKey(setting.Identity)) continue;

                    VisibilitySystemData.TryGetValue(setting.Identity, out HashSet<INetworkPlayer> players);

                    if (FastInDistanceXZ(player.Identity.transform.position, setting.Identity.transform.position, setting.SightDistance * setting.SightDistance))
                    {
                        if (players != null && players.Contains(player)) continue;

                        VisibilitySystemData[setting.Identity].Add(player);
                        InterestManager.ServerObjectManager.ShowToPlayer(setting.Identity, player);
                    }
                    else
                    {
                        if (players != null && !players.Contains(player)) continue;

                        VisibilitySystemData[setting.Identity].Remove(player);
                        InterestManager.ServerObjectManager.HideToPlayer(setting.Identity, player);
                    }
                }
            }

            _nextUpdate += _updateInterval;
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        /// <para>Passing in specific settings for this network object.</para>
        public override void RegisterObject(INetworkVisibility.BaseSettings proximitySettings)
        {
            _proximityObjects.Add(proximitySettings as ProximitySettings);
        }

        #endregion
    }
}

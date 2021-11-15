using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components
{
    [Serializable]
    public class ProximitySettings : BaseSettings
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
        private readonly HashSet<ProximitySettings> _proximityObjects = new HashSet<ProximitySettings>();

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
        }

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public override void CheckForObservers()
        {
            if (!(_nextUpdate < Time.time)) return;

            foreach (ProximitySettings setting in _proximityObjects)
            {
                if (!VisibilitySystemData.ContainsKey(setting.Identity)) continue;

                foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
                {
                    VisibilitySystemData.TryGetValue(setting.Identity, out HashSet<INetworkPlayer> players);

                    if (player.Identity == null || setting.Identity == null) continue;

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
        public override void RegisterObject(BaseSettings settings)
        {
            _proximityObjects.Add(settings as ProximitySettings);

            if (!VisibilitySystemData.ContainsKey(settings.Identity))
                VisibilitySystemData.Add(settings.Identity, new HashSet<INetworkPlayer>());
        }

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public override void UnRegisterObject(BaseSettings settings)
        {
            _proximityObjects.Remove(settings as ProximitySettings);

            VisibilitySystemData.Remove(settings.Identity);
        }

        #endregion
    }
}

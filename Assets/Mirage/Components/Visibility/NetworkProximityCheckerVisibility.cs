using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components
{
    /// <summary>
    /// Component that controls visibility of networked objects for players.
    /// <para>Any object with this component on it will not be visible to players more than a (configurable) distance away.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkProximityChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkProximityChecker.html")]
    public class NetworkProximityCheckerVisibility : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkProximityCheckerVisibility));

        private readonly float _sightDistnace = 10;
        private readonly float _updateInterval = 0;
        private float _nextUpdate = 0;
        private NetworkIdentity _identity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverObjectManager"></param>
        /// <param name="sightDistance"></param>
        /// <param name="updateInterval"></param>
        public NetworkProximityCheckerVisibility(ServerObjectManager serverObjectManager, float sightDistance, float updateInterval, NetworkIdentity objectTransform) : base(serverObjectManager)
        {
            _sightDistnace = sightDistance;
            _updateInterval = updateInterval;
            _identity = objectTransform;
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

            Vector3 a = identity.transform.position;

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                Vector3 b = player.Identity.transform.position;

                if (!FastInDistanceXZ(a, b, _sightDistnace * _sightDistnace)) continue;

                if (!VisibilitySystemData.ContainsKey(identity))
                    VisibilitySystemData.Add(identity, new HashSet<INetworkPlayer>());
                else if (VisibilitySystemData.ContainsKey(identity) && !VisibilitySystemData[identity].Contains(player))
                    VisibilitySystemData[identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
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

            foreach (NetworkIdentity identity in InterestManager.ServerObjectManager.Server.World.SpawnedIdentities)
            {
                Vector3 a = identity.transform.position;

                if (!FastInDistanceXZ(a, b, _sightDistnace * _sightDistnace)) continue;

                if (!VisibilitySystemData.ContainsKey(identity))
                    VisibilitySystemData.Add(identity, new HashSet<INetworkPlayer>());
                else if (VisibilitySystemData.ContainsKey(identity) && !VisibilitySystemData[identity].Contains(player))
                    VisibilitySystemData[identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
            }
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
            if (!(_nextUpdate < Time.time)) return;

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if(!VisibilitySystemData.ContainsKey(_identity)) continue;

                VisibilitySystemData.TryGetValue(_identity, out HashSet<INetworkPlayer> players);

                if (FastInDistanceXZ(player.Identity.transform.position, _identity.transform.position, _sightDistnace * _sightDistnace))
                {
                    if (players != null && players.Contains(player)) continue;

                    VisibilitySystemData[_identity].Add(player);
                    InterestManager.ServerObjectManager.ShowToPlayer(_identity, player);
                }
                else
                {
                    if(players !=null && !players.Contains(player)) continue;

                    VisibilitySystemData[_identity].Remove(player);
                    InterestManager.ServerObjectManager.HideToPlayer(_identity, player);
                }
            }

            _nextUpdate += _updateInterval;
        }

        #endregion
    }
}

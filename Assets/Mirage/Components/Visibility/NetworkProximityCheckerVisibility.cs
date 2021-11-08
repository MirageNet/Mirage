using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage
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
        private class NetIdComparer : IEqualityComparer<NetworkIdentity>
        {
            public bool Equals(NetworkIdentity x, NetworkIdentity y)
            {
                return x.NetId == y.NetId;
            }
            public int GetHashCode(NetworkIdentity obj)
            {
                return (int)obj.NetId;
            }
        }

        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkProximityCheckerVisibility));

        private readonly float _sightDistnace = 10;
        private readonly float _updateInterval = 0;
        private float _nextUpdate = 0;
        private readonly Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> lastFrame = new Dictionary<INetworkPlayer, HashSet<NetworkIdentity>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverObjectManager"></param>
        /// <param name="sightDistance"></param>
        /// <param name="updateInterval"></param>
        public NetworkProximityCheckerVisibility(ServerObjectManager serverObjectManager, float sightDistance, float updateInterval) : base(serverObjectManager)
        {
            _sightDistnace = sightDistance;
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
        private void Rebuild()
        {
            foreach (NetworkIdentity identity in InterestManager.ServerObjectManager.Server.World.SpawnedIdentities)
            {
                foreach (INetworkPlayer player in VisibilitySystemData.Keys)
                {
                    if (!VisibilitySystemData.TryGetValue(player, out HashSet<NetworkIdentity> nextSet))
                    {
                        nextSet = new HashSet<NetworkIdentity>(new NetIdComparer());
                        VisibilitySystemData[player] = nextSet;
                    }

                    nextSet.Add(identity);
                }
            }

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (!lastFrame.TryGetValue(player, out HashSet<NetworkIdentity> lastSet))
                {
                    lastSet = new HashSet<NetworkIdentity>(new NetIdComparer());
                    lastFrame[player] = lastSet;
                }

                if (!VisibilitySystemData.TryGetValue(player, out HashSet<NetworkIdentity> nextSet))
                {
                    nextSet = new HashSet<NetworkIdentity>(new NetIdComparer());
                    VisibilitySystemData[player] = nextSet;
                }


                foreach (NetworkIdentity identity in lastSet)
                {
                    if (!nextSet.Contains(identity))
                    {
                        InterestManager.ServerObjectManager.HideToPlayer(identity, player);
                    }
                }

                foreach (NetworkIdentity identity in nextSet)
                {
                    if (!lastSet.Contains(identity))
                    {
                        InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                    }
                }

                // reset collections
                lastSet.Clear();
                foreach (NetworkIdentity identity in nextSet)
                {
                    lastSet.Add(identity);
                }

                nextSet.Clear();
            }
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
            float sqRange = _sightDistnace * _sightDistnace;

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                Vector3 b = player.Identity.transform.position;

                if (FastInDistanceXZ(a, b, sqRange))
                {
                    InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                }
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
            float sqRange = _sightDistnace * _sightDistnace;

            foreach (NetworkIdentity identity in InterestManager.ServerObjectManager.Server.World.SpawnedIdentities)
            {
                Vector3 a = identity.transform.position;

                if (FastInDistanceXZ(a, b, sqRange))
                {
                    InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                }
            }
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
            if (!(_nextUpdate < Time.time)) return;

            Rebuild();

            _nextUpdate += _updateInterval;
        }

        #endregion
    }
}

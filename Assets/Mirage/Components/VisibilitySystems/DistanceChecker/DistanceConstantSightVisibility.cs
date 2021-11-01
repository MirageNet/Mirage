using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    /// <summary>
    /// Brute force distance check on all objects, all players have same sight distance
    /// </summary>
    public class DistanceConstantSightVisibility : NetworkVisibility
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

        private readonly float _sightDistnace = 10;
        private readonly float _updateInterval = 0;
        private float _nextUpdate = 0;

        private readonly Dictionary<INetworkPlayer, HashSet<NetworkIdentity>> _lastFrame = new Dictionary<INetworkPlayer, HashSet<NetworkIdentity>>();

        public DistanceConstantSightVisibility(ServerObjectManager serverObjectManager, float sightDistance, float updateInterval) : base(serverObjectManager)
        {
            _sightDistnace = sightDistance;
            _updateInterval = updateInterval;
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
                if (!_lastFrame.TryGetValue(player, out HashSet<NetworkIdentity> lastSet))
                {
                    lastSet = new HashSet<NetworkIdentity>(new NetIdComparer());
                    _lastFrame[player] = lastSet;
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

        List<INetworkPlayer> temp = new List<INetworkPlayer>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool fastInDistanceXZ(Vector3 a, Vector3 b, float sqRange)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            float sqDist = dx * dx + dz * dz;
            return sqDist < sqRange;
        }

        public override void CheckForObservers()
        {
            if (!(_nextUpdate < Time.time)) return;

            Rebuild();
            _nextUpdate += _updateInterval;
        }


        public override void OnAuthenticated(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            Vector3 b = player.Identity.transform.position;
            float sqRange = _sightDistnace * _sightDistnace;

            foreach (NetworkIdentity identity in InterestManager.ServerObjectManager.Server.World.SpawnedIdentities)
            {
                Vector3 a = identity.transform.position;

                if (fastInDistanceXZ(a, b, sqRange))
                {
                    InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                }
            }
        }

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

                if (fastInDistanceXZ(a, b, sqRange))
                {
                    InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
                }
            }
        }
    }
}

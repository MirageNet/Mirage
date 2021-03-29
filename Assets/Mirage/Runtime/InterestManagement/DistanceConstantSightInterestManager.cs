using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.InterestManagement
{
    /// <summary>
    /// Brute force distance check on all objects, all players have same sight distance
    /// </summary>
    public class DistanceConstantSightInterestManager : InterestManager
    {
        public float SightDistnace = 10;

        List<INetworkPlayer> temp = new List<INetworkPlayer>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool fastInDistanceXZ(Vector3 a, Vector3 b, float sqRange)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            float sqDist = dx * dx + dz * dz;
            return sqDist < sqRange;
        }

        public override IReadOnlyCollection<INetworkPlayer> Observers(NetworkIdentity identity)
        {
            if (identity == null) { return Array.Empty<INetworkPlayer>(); }

            Vector3 A = identity.transform.position;
            float sqRange = SightDistnace * SightDistnace;
            temp.Clear();
            foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
            {
                if (player.Identity == null) { continue; }

                Vector3 B = player.Identity.transform.position;

                if (fastInDistanceXZ(A, B, sqRange))
                {
                    temp.Add(player);
                }
            }

            return temp;
        }


        protected override void OnAuthenticated(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            Vector3 B = player.Identity.transform.position;
            float sqRange = SightDistnace * SightDistnace;

            foreach (NetworkIdentity identity in ServerObjectManager.SpawnedObjects.Values)
            {
                Vector3 A = identity.transform.position;

                if (fastInDistanceXZ(A, B, sqRange))
                {
                    ServerObjectManager.ShowForConnection(identity, player);
                }
            }
        }

        protected override void OnSpawned(NetworkIdentity identity)
        {
            // does object have owner?
            if (identity.ConnectionToClient != null)
            {
                OnAuthenticated(identity.ConnectionToClient);
            }

            Vector3 A = identity.transform.position;
            float sqRange = SightDistnace * SightDistnace;

            foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
            {
                Vector3 B = player.Identity.transform.position;

                if (fastInDistanceXZ(A, B, sqRange))
                {
                    ServerObjectManager.ShowForConnection(identity, player);
                }
            }
        }

        protected override int Send(NetworkIdentity identity, ArraySegment<byte> data, int channelId = 0, INetworkPlayer skip = null)
        {
            int count = 0;

            foreach (INetworkPlayer player in Observers(identity))
            {
                if (player != skip && player != ServerObjectManager.Server.LocalPlayer)
                {
                    // send to all connections, but don't wait for them
                    player.Send(data, channelId);
                    count++;
                }
            }
            return count;
        }
    }
}

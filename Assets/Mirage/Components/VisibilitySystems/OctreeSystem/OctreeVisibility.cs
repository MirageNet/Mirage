using System.Collections.Generic;
using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    public class OctreeVisibility : NetworkVisibility
    {
        #region Fields

        internal BoundsOctree<NetworkIdentity> Octree;
        private readonly HashSet<NetworkIdentity> _octreeObjectsShow = new HashSet<NetworkIdentity>();

        private readonly Dictionary<INetworkPlayer, OctreeInterestChecker> _spawnedPlayers =
            new Dictionary<INetworkPlayer, OctreeInterestChecker>();

        #endregion

        #region Overrides of NetworkVisibility

        public OctreeVisibility(ServerObjectManager interestManager, float initialWorldSize, Vector3 position,
            float minimumNodeSize, float looseness) : base(interestManager)
        {
            Octree = new BoundsOctree<NetworkIdentity>(initialWorldSize, position, minimumNodeSize, looseness);
        }

        /// <summary>
        /// Invoked when a player joins the server
        /// It should show all objects relevant to that player
        /// </summary>
        /// <param name="player"></param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            //NOOP
        }

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            identity.TryGetComponent(out OctreeInterestChecker colliderComponent);

            // Let's add player to our list to ilerate over later in update.
            if (identity.Owner != null && !_spawnedPlayers.ContainsKey(identity.Owner))
                _spawnedPlayers.Add(identity.Owner, colliderComponent);

            if (colliderComponent is null)
            {
                return;
            }

            Octree.Add(identity,
                new Bounds(identity.transform.position, colliderComponent.CurrentBounds.size));

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (!VisibilitySystemData.ContainsKey(player))
                    VisibilitySystemData.Add(player, new HashSet<NetworkIdentity>());
                else if (VisibilitySystemData.ContainsKey(player) && !VisibilitySystemData[player].Contains(identity))
                    VisibilitySystemData[player].Add(identity);

                if (!Octree.IsColliding(colliderComponent.CurrentBounds, player.Identity) &&
                    player.Identity != identity) continue;

                InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
            }
        }

        /// <summary>
        /// Find out all the players that can see an object
        /// </summary>
        /// <returns></returns>
        public override void CheckForObservers()
        {
            if (InterestManager == null || !InterestManager.ServerObjectManager.Server.Active) return;

            foreach (KeyValuePair<INetworkPlayer, OctreeInterestChecker> player in _spawnedPlayers)
            {
                _octreeObjectsShow.Clear();

                Octree.GetColliding(_octreeObjectsShow, player.Value.CurrentBounds);

                VisibilitySystemData.TryGetValue(player.Key, out HashSet<NetworkIdentity> spawnedIdentities);

                if (spawnedIdentities != null)
                {
                    foreach (NetworkIdentity identity in spawnedIdentities)
                    {
                        if (identity == player.Key.Identity) continue;

                        InterestManager.ServerObjectManager.HideToPlayer(identity, player.Key);
                    }
                }

                VisibilitySystemData[player.Key].Clear();

                // We need to check against our current populated list to make sure 
                foreach (NetworkIdentity identity in _octreeObjectsShow)
                {

                    InterestManager.ServerObjectManager.ShowToPlayer(identity, player.Key);

                    VisibilitySystemData[player.Key].Add(identity);
                }
            }
        }

        #endregion
    }
}

using System.Collections.Generic;
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
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkProximityChecker.html")]
    public class NetworkProximityChecker : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkProximityChecker));

        /// <summary>
        /// The maximim range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public float VisibilityRange = 10;

        /// <summary>
        /// Flag to force this object to be hidden for players.
        /// <para>If this object is a player object, it will not be hidden for that player.</para>
        /// </summary>
        [Tooltip("Enable to force this object to be hidden from players.")]
        public bool ForceHidden;

        private readonly Dictionary<INetworkPlayer, bool> _oldNetworkPlayers = new Dictionary<INetworkPlayer, bool>();

        #region Overrides of BaseNetworkVisibility

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (Vector3.Distance(player.Identity.transform.position, identity.transform.position) < VisibilityRange)
                    InterestManager.ServerObjectManager.ShowForConnection(identity, player);
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="position"></param>
        /// <param name="players"></param>
        public override void CheckForObservers(NetworkIdentity identity, Vector3 position, out HashSet<INetworkPlayer> players)
        {
            players = new HashSet<INetworkPlayer>();

            // if force hidden then return without adding any observers.
            if (ForceHidden)
                return;

            // brute force distance check
            // -> only player connections can be observers, so it's enough if we
            //    go through all connections instead of all spawned identities.
            // -> compared to UNET's sphere cast checking, this one is orders of
            //    magnitude faster. if we have 10k monsters and run a sphere
            //    cast 10k times, we will see a noticeable lag even with physics
            //    layers. but checking to every connection is fast.
            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (player == null || player.Identity == null || identity == player.Identity) continue;

                if (!_oldNetworkPlayers.ContainsKey(player))
                    _oldNetworkPlayers.Add(player, false);

                // check distance
                if (Vector3.SqrMagnitude(player.Identity.transform.position - position) < VisibilityRange * VisibilityRange)
                {
                    if (!_oldNetworkPlayers[player])
                    {
                        _oldNetworkPlayers[player] = true;

                        identity.ServerObjectManager.ShowForConnection(identity, player);
                    }

                    players.Add(player);
                }
                else
                {
                    if (!_oldNetworkPlayers[player]) continue;

                    _oldNetworkPlayers[player] = false;
                    identity.ServerObjectManager.HideForConnection(identity, player);
                }
            }
        }

        #endregion
    }
}

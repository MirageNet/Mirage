using System.Collections.Generic;
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
    [HelpURL("https://miragenet.github.io/Mirage/docs/components/network-proximity-checker")]
    public class NetworkProximityChecker : NetworkVisibility
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkProximityChecker));

        /// <summary>
        /// The maximim range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public int VisibilityRange = 10;

        /// <summary>
        /// How often (in seconds) that this object should update the list of observers that can see it.
        /// </summary>
        [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
        public float VisibilityUpdateInterval = 1;

        /// <summary>
        /// Flag to force this object to be hidden for players.
        /// <para>If this object is a player object, it will not be hidden for that player.</para>
        /// </summary>
        [Tooltip("Enable to force this object to be hidden from players.")]
        public bool ForceHidden;

        public void Awake()
        {
            Identity.OnStartServer.AddListener(() =>
            {
                InvokeRepeating(nameof(RebuildObservers), 0, VisibilityUpdateInterval);
            });

            Identity.OnStopServer.AddListener(() =>
            {
                CancelInvoke(nameof(RebuildObservers));
            });
        }

        private void RebuildObservers()
        {
            Identity.RebuildObservers(false);
        }

        /// <summary>
        /// Callback used by the visibility system to determine if an observer (player) can see this object.
        /// <para>If this function returns true, the network connection will be added as an observer.</para>
        /// </summary>

        /// <param name="player">Network connection of a player.</param>
        /// <returns>True if the player can see this object.</returns>
        public override bool OnCheckObserver(INetworkPlayer player)
        {
            if (ForceHidden)
                return false;

            return Vector3.Distance(player.Identity.transform.position, transform.position) < VisibilityRange;
        }

        /// <summary>
        /// Callback used by the visibility system to (re)construct the set of observers that can see this object.
        /// <para>Implementations of this callback should add network connections of players that can see this object to the observers set.</para>
        /// </summary>
        /// <param name="observers">The new set of observers for this object.</param>
        /// <param name="initialize">True if the set of observers is being built for the first time.</param>
        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            // if force hidden then return without adding any observers.
            if (ForceHidden)
                return;

            // 'transform.' calls GetComponent, only do it once
            var position = transform.position;

            // brute force distance check
            // -> only player connections can be observers, so it's enough if we
            //    go through all connections instead of all spawned identities.
            // -> compared to UNET's sphere cast checking, this one is orders of
            //    magnitude faster. if we have 10k monsters and run a sphere
            //    cast 10k times, we will see a noticeable lag even with physics
            //    layers. but checking to every connection is fast.
            foreach (var player in Server.AuthenticatedPlayers)
            {
                // check distance
                if (player != null && player.HasCharacter && Vector3.Distance(player.Identity.transform.position, position) < VisibilityRange)
                {
                    observers.Add(player);
                }
            }
        }
    }
}

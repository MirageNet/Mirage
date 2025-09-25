using System;
using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Visibility.SpatialHash
{
    public class SpatialHashVisibility : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(SpatialHashVisibility));

        [Tooltip("How many grid away the player can be to see this object. Real distance is this mutlipled by SpatialHashSystem")]
        public int GridVisibleRange = 1;

        [ReadOnlyInspector]
        public SpatialHashSystem System;

        /// <param name="player">Network connection of a player.</param>
        /// <returns>True if the player can see this object.</returns>
        public override bool OnCheckObserver(INetworkPlayer player)
        {
            ThrowIfNoSystem();

            if (player.Identity == null)
                return false;


            Vector2 thisPosition = transform.position.ToXZ();
            Vector2 playerPosition = player.Identity.transform.position.ToXZ();

            return System.Grid.IsVisible(thisPosition, playerPosition, GridVisibleRange);
        }

        /// <summary>
        /// Callback used by the visibility system to (re)construct the set of observers that can see this object.
        /// <para>Implementations of this callback should add network connections of players that can see this object to the observers set.</para>
        /// </summary>
        /// <param name="observers">The new set of observers for this object.</param>
        /// <param name="initialize">True if the set of observers is being built for the first time.</param>
        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            ThrowIfNoSystem();

            System.Grid.BuildObservers(observers, transform.position.ToXZ(), GridVisibleRange);
        }

        private void ThrowIfNoSystem()
        {
            if (System is null)
            {
                throw new InvalidOperationException("No SpatialHashSystem Set on SpatialHashVisibility. Add SpatialHashSystem to your NetworkManager before using this component.");
            }
        }
    }
}

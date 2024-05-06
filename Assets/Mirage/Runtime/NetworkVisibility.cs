using System.Collections.Generic;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// NetworkBehaviour that calculates if the gameObject should be visible to different players or not
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class NetworkVisibility : NetworkBehaviour, INetworkVisibility
    {
        public delegate void VisibilityChanged(INetworkPlayer player, bool visible);

        /// <summary>
        /// Invoked on server when visibility changes for player
        /// <para>Invoked before Spawn/Show/Hide message is sent to client</para>
        /// <para>Invoked when object is spawned, but not when it is desotroyed</para>
        /// </summary>
        public event VisibilityChanged OnVisibilityChanged;

        internal void InvokeVisibilityChanged(INetworkPlayer player, bool visible)
        {
            OnVisibilityChanged?.Invoke(player, visible);
        }

        /// <summary>
        /// Callback used by the visibility system to determine if an observer (player) can see this object.
        /// <para>If this function returns true, the network connection will be added as an observer.</para>
        /// </summary>
        /// <param name="player">Network connection of a player.</param>
        /// <returns>True if the player can see this object.</returns>
        public abstract bool OnCheckObserver(INetworkPlayer player);

        /// <summary>
        /// Callback used by the visibility system to (re)construct the set of observers that can see this object.
        /// <para>Implementations of this callback should add network connections of players that can see this object to the observers set.</para>
        /// <para>
        /// NOTE: override this function if you want to optimize this loop in your visibility,
        /// for example if you need to call GetComponent on this object you can call it once at the start of the loop
        /// </para>
        /// </summary>
        /// <param name="observers">The new set of observers for this object.</param>
        /// <param name="initialize">True if the set of observers is being built for the first time.</param>
        public virtual void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            foreach (var player in Server.AllPlayers)
            {
                if (OnCheckObserver(player))
                {
                    observers.Add(player);
                }
            }
        }
    }
}

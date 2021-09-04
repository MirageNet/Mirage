﻿using System.Collections.Generic;
using UnityEngine;

namespace Mirage
{
    // the name NetworkProximityCheck implies that it's only about objects in
    // proximity to the player. But we might have room based, guild based,
    // instanced based checks too, so NetworkVisibility is more fitting.
    //
    // note: we inherit from NetworkBehaviour so we can reuse .Identity, etc.
    // note: unlike UNET, we only allow 1 proximity checker per NetworkIdentity.
    [DisallowMultipleComponent]
    public abstract class NetworkVisibility : NetworkBehaviour
    {
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
            foreach (INetworkPlayer player in Server.Players)
            {
                if (OnCheckObserver(player))
                {
                    observers.Add(player);
                }
            }
        }
    }
}

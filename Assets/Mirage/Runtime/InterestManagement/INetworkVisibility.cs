using System.Collections.Generic;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public interface INetworkVisibility
    {
        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        void OnSpawned(NetworkIdentity identity);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="identity">The identity of the object that has spawned in.</param>
        /// <param name="position">The position in which the player spawned in at. We use <see cref="Transform.localPosition"/></param>
        /// <param name="players"></param>
        void CheckForObservers(NetworkIdentity identity, Vector3 position, out HashSet<INetworkPlayer> players);
    }
}

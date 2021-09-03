using System;
using System.Collections.Generic;
using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Component that controls visibility of networked objects based on match id.
    /// <para>Any object with this component on it will only be visible to other objects in the same match.</para>
    /// <para>This would be used to isolate players to their respective matches within a single game server instance. </para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkMatchChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkMatchChecker.html")]
    public class NetworkMatchChecker : NetworkVisibility
    {
        static readonly Dictionary<Guid, HashSet<NetworkIdentity>> matchPlayers = new Dictionary<Guid, HashSet<NetworkIdentity>>();

        Guid currentMatch = Guid.Empty;

        [Header("Diagnostics")]
        public string currentMatchDebug;

        /// <summary>
        /// Set this to the same value on all networked objects that belong to a given match
        /// </summary>
        public Guid MatchId
        {
            get { return currentMatch; }
            set
            {
                if (currentMatch == value) return;

                // cache previous match so observers in that match can be rebuilt
                Guid previousMatch = currentMatch;

                // Set this to the new match this object just entered ...
                currentMatch = value;
                // ... and copy the string for the inspector because Unity can't show Guid directly
                currentMatchDebug = currentMatch.ToString();

                if (previousMatch != Guid.Empty)
                {
                    // Remove this object from the hashset of the match it just left
                    //TODO Implement.

                    // RebuildObservers of all NetworkIdentity's in the match this object just left
                }

                if (currentMatch != Guid.Empty)
                {
                    // Make sure this new match is in the dictionary
                    if (!matchPlayers.ContainsKey(currentMatch))
                        matchPlayers.Add(currentMatch, new HashSet<NetworkIdentity>());

                    // Add this object to the hashset of the new match
                    //TODO Implement.

                    // RebuildObservers of all NetworkIdentity's in the match this object just entered
                    //TODO Implement.
                }
            }
        }

        #region Overrides of BaseNetworkVisibility

        /// <summary>
        ///    Invoke when an object spawns on server.
        /// </summary>
        /// <param name="identity">The identity of the object that has spawned in.</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                NetworkMatchChecker networkMatchChecker = player.Identity.GetComponent<NetworkMatchChecker>();

                if (networkMatchChecker == null)
                    continue;

                if (networkMatchChecker.MatchId == MatchId) continue;

                if (!matchPlayers.ContainsKey(currentMatch))
                    matchPlayers.Add(currentMatch, new HashSet<NetworkIdentity>());

                matchPlayers[currentMatch].Add(identity);
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
            throw new NotImplementedException();
        }

        #endregion
    }
}

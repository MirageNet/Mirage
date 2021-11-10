using System;
using System.Collections.Generic;
using Mirage.InterestManagement;
using UnityEngine;

namespace Mirage.Components
{
    /// <summary>
    /// Component that controls visibility of networked objects based on match id.
    /// <para>Any object with this component on it will only be visible to other objects in the same match.</para>
    /// <para>This would be used to isolate players to their respective matches within a single game server instance. </para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkMatchChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://miragenet.github.io/Mirage/Articles/Components/NetworkMatchChecker.html")]
    public class NetworkMatchCheckerVisibility : NetworkVisibility
    {
        static readonly Dictionary<Guid, HashSet<NetworkIdentity>> matchPlayers = new Dictionary<Guid, HashSet<NetworkIdentity>>();

        private NetworkIdentity Identity;
        Guid currentMatch = Guid.Empty;

        [Header("Diagnostics")]
        //[SyncVar]
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
                    matchPlayers[previousMatch].Remove(Identity);
                }

                if (currentMatch != Guid.Empty)
                {
                    // Make sure this new match is in the dictionary
                    if (!matchPlayers.ContainsKey(currentMatch))
                        matchPlayers.Add(currentMatch, new HashSet<NetworkIdentity>());

                    // Add this object to the hashset of the new match
                    matchPlayers[currentMatch].Add(Identity);
                }
            }
        }

        public NetworkMatchCheckerVisibility(ServerObjectManager serverObjectManager) : base(serverObjectManager)
        {
        }

        #region Overrides of NetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            if (currentMatch == Guid.Empty) return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            if (currentMatch == Guid.Empty) return;
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
            if (currentMatch == Guid.Empty) return;
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        public override void RegisterObject(BaseSettings settings)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

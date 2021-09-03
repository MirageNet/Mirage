using System.Collections.Generic;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    /// <summary>
    /// Component that controls visibility of networked objects between scenes.
    /// <para>Any object with this component on it will only be visible to other objects in the same scene</para>
    /// <para>This would be used when the server has multiple additive subscenes loaded to isolate players to their respective subscenes</para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkSceneChecker")]
    [RequireComponent(typeof(NetworkIdentity))]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkSceneChecker.html")]
    public class NetworkSceneChecker : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkSceneChecker));

        /// <summary>
        /// Flag to force this object to be hidden from all observers.
        /// <para>If this object is a player object, it will not be hidden for that client.</para>
        /// </summary>
        [Tooltip("Enable to force this object to be hidden from all observers.")]
        public bool ForceHidden;

        // Use Scene instead of string scene.name because when additively loading multiples of a subscene the name won't be unique
        static readonly Dictionary<Scene, HashSet<NetworkIdentity>> SceneCheckerObjects = new Dictionary<Scene, HashSet<NetworkIdentity>>();

        #region Overrides of BaseNetworkVisibility

        /// <summary>
        ///    Invoke when an object spawns on server.
        /// </summary>
        /// <param name="identity">The identity of the object that has spawned in.</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if(player.Identity.gameObject.scene != identity.gameObject.scene) continue;

                if (!SceneCheckerObjects.ContainsKey(identity.gameObject.scene))
                    SceneCheckerObjects.Add(identity.gameObject.scene, new HashSet<NetworkIdentity>());

                SceneCheckerObjects[identity.gameObject.scene].Add(identity);

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

            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (player.Identity.gameObject.scene == identity.gameObject.scene)
                {
                    if (!SceneCheckerObjects.ContainsKey(identity.gameObject.scene)) continue;

                    if (SceneCheckerObjects[identity.gameObject.scene].Contains(player.Identity))
                    {
                        players.Add(player);
                    }
                    else
                    {
                        SceneCheckerObjects[identity.gameObject.scene].Add(player.Identity);
                        players.Add(player);
                    }
                }
                else
                {
                    if (SceneCheckerObjects.ContainsKey(identity.gameObject.scene))
                    {
                        SceneCheckerObjects[identity.gameObject.scene].Remove(identity);
                    }

                    InterestManager.ServerObjectManager.HideForConnection(identity, player);
                }
            }
        }

        #endregion
    }
}

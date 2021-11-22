using System;
using System.Collections.Generic;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

// todo move this to sub namespace
namespace Mirage
{
    public class SceneVisibilitySystem : VisibilitySystem
    {
        static readonly ILogger Logger = LogFactory.GetLogger<SceneVisibilitySystem>();

        #region Fields

        private readonly Dictionary<NetworkIdentity, Scene> _sceneObjects = new Dictionary<NetworkIdentity, Scene>();

        #endregion

        /// <summary>
        /// Call this function on an object to move it to a new scene and rebuild its observers
        /// </summary>
        /// <param name="scene">The scene we want to move object to.</param>
        /// <param name="identity">The object we want to move to scene.</param>
        public void MoveToScene(Scene scene, NetworkIdentity identity)
        {
            // Remove object from all player's
            if (Observers.ContainsKey(identity))
            {
                foreach (INetworkPlayer player in Observers[identity])
                {
                    InterestManager.HideToPlayer(identity, player);
                }

                // Reset list to empty now.
                Observers[identity] = new HashSet<INetworkPlayer>();
            }

            // move player to new scene
            SceneManager.MoveGameObjectToScene(identity.gameObject, scene);

            // spawn new objects for player
            foreach (INetworkPlayer player in InterestManager.ServerObjectManager.Server.Players)
            {
                if (player.Identity.gameObject.scene.handle != scene.handle) continue;

                InterestManager.ShowToPlayer(identity, player);
            }
        }

        public SceneVisibilitySystem(ServerObjectManager serverObjectManager) : base(serverObjectManager)
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
            // does object have owner?
            if (identity.Owner != null)
            {
                RebuildForPlayer(identity.Owner);
            }
        }

        /// <summary>
        ///     When new player authenticates we need to show them objects they should see.
        /// </summary>
        /// <param name="player">The player that just authenticated and we need to show objects to.</param>
        public override void RebuildForPlayer(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            foreach (KeyValuePair<NetworkIdentity, Scene> kvp in _sceneObjects)
            {
                NetworkIdentity identity = kvp.Key;
                Scene scene = kvp.Value;
                if (scene != player.Identity.gameObject.scene) continue;

                if (!Observers.ContainsKey(identity))
                    Observers.Add(identity, new HashSet<INetworkPlayer>());
                else if (Observers.ContainsKey(identity) && !Observers[identity].Contains(player))
                    Observers[identity].Add(player);

                InterestManager.ShowToPlayer(identity, player);
            }

            // Always show self to them.
            InterestManager.ShowToPlayer(player.Identity, player);
        }

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public override void RebuildAll()
        {
            //NOOP realistically this should only ever change if devs
            // move game object to another scene manually.
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        public override void RegisterObject<TSettings>(NetworkIdentity identity, TSettings settings)
        {
            if (settings is Scene scene)
            {
                _sceneObjects.Add(identity, scene);
            }
            else
            {
                throw new ArgumentException($"Settings should be {nameof(Scene)}", nameof(settings));
            }

            if (!Observers.ContainsKey(identity))
                Observers.Add(identity, new HashSet<INetworkPlayer>());
        }

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public override void UnregisterObject(NetworkIdentity identity)
        {
            _sceneObjects.Remove(identity);

            Observers.Remove(identity);
        }

        #endregion
    }
}

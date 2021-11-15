using System;
using System.Collections.Generic;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    [Serializable]
    public class SceneSettings : BaseSettings
    {
        /// <summary>
        /// The maximum range that objects will be visible at.
        /// </summary>
        [Tooltip("The maximum range that objects will be visible at.")]
        public Scene Scene;
    }

    public class SceneVisibilitySystem : VisibilitySystem
    {
        static readonly ILogger Logger = LogFactory.GetLogger<SceneVisibilitySystem>();

        #region Fields

        private readonly HashSet<SceneSettings> _sceneObjects = new HashSet<SceneSettings>();

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
                    InterestManager.ServerObjectManager.HideToPlayer(identity, player);
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

                InterestManager.ServerObjectManager.ShowToPlayer(identity, player);
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
                OnAuthenticated(identity.Owner);
            }
        }

        /// <summary>
        ///     When new player authenticates we need to show them objects they should see.
        /// </summary>
        /// <param name="player">The player that just authenticated and we need to show objects to.</param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            // no owned object, nothing to see
            if (player.Identity == null) { return; }

            foreach (SceneSettings setting in _sceneObjects)
            {
                if (setting.Scene.handle != player.Identity.gameObject.scene.handle) continue;

                if (!Observers.ContainsKey(setting.Identity))
                    Observers.Add(setting.Identity, new HashSet<INetworkPlayer>());
                else if (Observers.ContainsKey(setting.Identity) && !Observers[setting.Identity].Contains(player))
                    Observers[setting.Identity].Add(player);

                InterestManager.ServerObjectManager.ShowToPlayer(setting.Identity, player);
            }

            // Always show self to them.
            InterestManager.ServerObjectManager.ShowToPlayer(player.Identity, player);
        }

        /// <summary>
        ///     Checks for observers for each registered network object.
        /// </summary>
        public override void Rebuild()
        {
            //NOOP realistically this should only ever change if devs
            // move game object to another scene manually.
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        public override void RegisterObject(BaseSettings settings)
        {
            _sceneObjects.Add(settings as SceneSettings);

            if (!Observers.ContainsKey(settings.Identity))
                Observers.Add(settings.Identity, new HashSet<INetworkPlayer>());
        }

        /// <summary>
        ///     Controls un-register objects from this network visibility system
        /// </summary>
        public override void UnRegisterObject(BaseSettings settings)
        {
            _sceneObjects.Remove(settings as SceneSettings);

            Observers.Remove(settings.Identity);
        }

        #endregion
    }
}
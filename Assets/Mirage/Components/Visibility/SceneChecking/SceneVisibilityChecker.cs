using System.Collections.Generic;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    public class SceneVisibilityChecker : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger<SceneVisibilityChecker>();

        #region Fields

        private Scene _objectCurrentScene;
        private NetworkIdentity Identity;

        #endregion

        /// <summary>
        /// Call this function on an object to move it to a new scene and rebuild its observers
        /// </summary>
        /// <param name="scene"></param>
        public void MoveToScene(Scene scene)
        {
            //INetworkPlayer owner = Identity.Owner;

            //// remove player from other clients
            //removeObservers(Identity);

            //// remove other objects from player
            //if (owner != null)
            //    owner.RemoveAllVisibleObjects();

            //// move player to new scene
            //SceneManager.MoveGameObjectToScene(Identity.gameObject, scene);

            //// spawn new objects for player
            //if (owner != null)
            //    ServerObjectManager.SpawnVisibleObjects(Identity.Owner);
        }

        public SceneVisibilityChecker(ServerObjectManager serverObjectManager, Scene objectScene) : base(serverObjectManager)
        {
            _objectCurrentScene = objectScene;
        }

        #region Overrides of NetworkVisibility

        /// <summary>
        ///     Invoked when an object is spawned in the server
        ///     It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        public override void OnSpawned(NetworkIdentity identity)
        {
            // NOOP
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            if(player.Identity.gameObject.scene.handle != _objectCurrentScene.handle) return;
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
        }

        /// <summary>
        ///     Controls register new objects to this network visibility system
        /// </summary>
        public override void RegisterObject(BaseSettings settings)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

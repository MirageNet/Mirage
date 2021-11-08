using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    public class SceneVisibilityChecker : NetworkVisibility
    {
        static readonly ILogger logger = LogFactory.GetLogger<SceneVisibilityChecker>();

        //public override bool OnCheckObserver(INetworkPlayer player)
        //{
        //    NetworkIdentity character = player.Identity;
        //    if (character == null)
        //    {
        //        if (logger.LogEnabled()) logger.Log($"SceneChecker: {player} had no character");
        //        return false;
        //    }

        //    Scene playerScene = character.gameObject.scene;
        //    if (!playerScene.IsValid())
        //    {
        //        if (logger.WarnEnabled()) logger.LogWarning($"SceneChecker: Could not find scene for {player}");
        //        return false;
        //    }

        //    Scene thisScene = gameObject.scene;
        //    bool visible = playerScene == thisScene;
        //    if (logger.LogEnabled()) logger.Log($"SceneChecker: {player} can see '{this}': {visible}");
        //    return visible;
        //}

        //public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        //{
        //    foreach (INetworkPlayer player in Server.Players)
        //    {
        //        if (OnCheckObserver(player))
        //        {
        //            observers.Add(player);
        //        }
        //    }
        //}

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

        private void removeObservers(NetworkIdentity identity)
        {
            //HashSet<INetworkPlayer> observers = identity.observers;
            //foreach (INetworkPlayer observer in observers)
            //{
            //    observer.RemoveFromVisList(identity);
            //}
        }

        public SceneVisibilityChecker(ServerObjectManager serverObjectManager) : base(serverObjectManager)
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
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public override void OnAuthenticated(INetworkPlayer player)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     
        /// </summary>
        public override void CheckForObservers()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

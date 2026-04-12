using System.Collections.Generic;
using Mirage.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage
{
    public class SceneVisibilityChecker : NetworkVisibility
    {
        private static readonly ILogger logger = LogFactory.GetLogger<SceneVisibilityChecker>();

        public override bool OnCheckObserver(INetworkPlayer player)
        {
            var looker = player.VisibilityIdentity;
            if (looker == null)
            {
                if (logger.LogEnabled()) logger.Log($"SceneChecker: {player} had no character or looker");
                return false;
            }

            var playerScene = looker.gameObject.scene;
            if (!playerScene.IsValid())
            {
                if (logger.WarnEnabled()) logger.LogWarning($"SceneChecker: Could not find scene for {player}");
                return false;
            }

            var thisScene = gameObject.scene;
            var visible = playerScene == thisScene;
            if (logger.LogEnabled()) logger.Log($"SceneChecker: {player} can see '{this}': {visible}");
            return visible;
        }

        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            foreach (var player in Server.AuthenticatedPlayers)
            {
                if (OnCheckObserver(player))
                {
                    observers.Add(player);
                }
            }
        }

        /// <summary>
        /// Call this function on an object to move it to a new scene and rebuild its observers
        /// </summary>
        /// <param name="scene"></param>
        public void MoveToScene(Scene scene)
        {
            var owner = Identity.Owner;

            // remove player from other clients
            removeObservers(Identity);

            // remove other objects from player
            if (owner != null)
                owner.RemoveAllVisibleObjects();

            // move player to new scene
            SceneManager.MoveGameObjectToScene(Identity.gameObject, scene);

            // spawn new objects for player
            if (owner != null)
                ServerObjectManager.SpawnVisibleObjects(Identity.Owner);
        }

        private void removeObservers(NetworkIdentity identity)
        {
            var observers = identity.observers;
            foreach (var observer in observers)
            {
                observer.RemoveFromVisList(identity);
            }
        }
    }
}
